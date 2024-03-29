﻿using System.Collections.Generic;

using Raccoon.Components;
using Raccoon.Util;

namespace Raccoon {
    public sealed partial class Physics {
        #region Public Members

        public static int FixedDeltaTime = 1000 / 60,
                          ConstraintSolverAccuracy = 3;

        public static float FixedDeltaTimeSeconds = FixedDeltaTime / 1000f;

        #endregion Public Members

        #region Private Members

        private delegate bool CollisionCheckDelegate(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts);

        private static readonly System.Lazy<Physics> _lazy = new System.Lazy<Physics>(() => new Physics());

        // tags
        private Dictionary<BitTag, BitTag> _collisionTagTable = new Dictionary<BitTag, BitTag>();

        // colliders
        private List<Body> _colliders = new List<Body>();
        private Dictionary<BitTag, List<Body>> _collidersByTag = new Dictionary<BitTag, List<Body>>();

        // collision checking
        private Dictionary<System.Type, Dictionary<System.Type, CollisionCheckDelegate>> _collisionFunctions = new Dictionary<System.Type, Dictionary<System.Type, CollisionCheckDelegate>>();
        private int _leftOverDeltaTime;
        private List<Body> _narrowPhaseBodies = new List<Body>();
        private List<InternalCollisionInfo> _internalCollisionInfo = new List<InternalCollisionInfo>();
        private HashSet<Body> _collidedOnThisFrame = new HashSet<Body>();

        #endregion Private Members

        #region Constructors

        private Physics() {
            IsRunning = true;

            // collision functions dictionary
            System.Type box = typeof(BoxShape),
                        circle = typeof(CircleShape),
                        polygon = typeof(PolygonShape),
                        grid = typeof(GridShape);

            System.Type[] colliderTypes = {
                box,
                circle,
                polygon,
                grid
            };

            foreach (System.Type type in colliderTypes) {
                _collisionFunctions.Add(type, new Dictionary<System.Type, CollisionCheckDelegate>());
            }

            // box vs others
            _collisionFunctions[box].Add(box, CheckBoxBox);
            _collisionFunctions[box].Add(circle, CheckBoxCircle);
            _collisionFunctions[box].Add(polygon, CheckBoxPolygon);
            _collisionFunctions[box].Add(grid, CheckBoxGrid);

            // grid vs others
            _collisionFunctions[grid].Add(grid, CheckGridGrid);
            _collisionFunctions[grid].Add(box, CheckGridBox);
            _collisionFunctions[grid].Add(circle, CheckGridCircle);
            _collisionFunctions[grid].Add(polygon, CheckGridPolygon);

            // circle vs others
            _collisionFunctions[circle].Add(circle, CheckCircleCircle);
            _collisionFunctions[circle].Add(box, CheckCircleBox);
            _collisionFunctions[circle].Add(polygon, CheckCirclePolygon);
            _collisionFunctions[circle].Add(grid, CheckCircleGrid);

            // polygon vs others
            _collisionFunctions[polygon].Add(polygon, CheckPolygonPolygon);
            _collisionFunctions[polygon].Add(box, CheckPolygonBox);
            _collisionFunctions[polygon].Add(circle, CheckPolygonCircle);
            _collisionFunctions[polygon].Add(grid, CheckPolygonGrid);
        }

        #endregion Constructors

        #region Public Static Properties

        public static Physics Instance { get { return _lazy.Value; } }
        public static bool IsRunning { get; set; }

        #endregion Public Static Properties

        #region Internal Properties

#if DEBUG
        internal static long UpdatePositionExecutionTime { get; private set; }
        internal static long SolveConstraintsExecutionTime { get; private set; }
        internal static long CollisionDetectionBroadPhaseExecutionTime { get; private set; }
        internal static long CollisionDetectionNarrowPhaseExecutionTime { get; private set; }
        internal static int CollidersBroadPhaseCount { get; private set; }
        internal static int CollidersNarrowPhaseCount { get; private set; }
#endif

        internal int Timesteps { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        /// <summary>
        /// Checks if one or more tag exists.
        /// </summary>
        /// <param name="tags">One or more bit tags.</param>
        /// <returns>True if all tags exists, False otherwise.</returns>
        public bool HasTag(BitTag tags) {
            foreach (BitTag tag in tags) {
                if (!_collidersByTag.ContainsKey(tag)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if at least one tag of a set exists.
        /// </summary>
        /// <param name="tags">One or more bit tags.</param>
        /// <returns>True if at least one tag exists, False otherwise.</returns>
        public bool HasAnyTag(BitTag tags) {
            foreach (BitTag tag in tags) {
                if (_collidersByTag.ContainsKey(tag)) {
                    return true;
                }
            }

            return false;
        }

        public void RegisterTags<T>() {
            System.Type tagType = typeof(T);

            if (!tagType.IsEnum) {
                throw new System.ArgumentException($"Expecting a valid {nameof(System.Enum)}, but found '{tagType.Name}'.");
            }

            if (!tagType.IsDefined(typeof(System.FlagsAttribute), false)) {
                throw new System.ArgumentException("Tags Type must contains System.FlagsAttribute and all values declared as power of 2.");
            }

            foreach (System.Enum enumValue in System.Enum.GetValues(tagType)) {
                BitTag tag = enumValue;

                if (tag == BitTag.None) {
                    continue;
                }

                _collidersByTag.Add(tag, new List<Body>());
                _collisionTagTable.Add(tag, new BitTag(0, tagType));
            }
        }

        public void ClearTags() {
            _collidersByTag.Clear();
            _collisionTagTable.Clear();
        }

        public BitTag GetCollidableTags(BitTag tags) {
            BitTag collidableTags = new BitTag(0, tags.EnumType);

            foreach (BitTag tag in tags) {
                collidableTags += _collisionTagTable[tag];
            }

            return collidableTags;
        }

        public void RegisterCollision(BitTag tagsA, BitTag tagsB) {
            ValidateTags(tagsA, "tagsA");
            ValidateTags(tagsB, "tagsB");
            Debug.Assert(tagsA != BitTag.None && tagsB != BitTag.None, $"Can't register a collision with tag None.");

            foreach (BitTag singleTagA in tagsA) {
                _collisionTagTable[singleTagA] += tagsB;
            }

            foreach (BitTag singleTagB in tagsB) {
                _collisionTagTable[singleTagB] += tagsA;
            }
        }

        public void RemoveCollision(BitTag tagsA, BitTag tagsB) {
            ValidateTags(tagsA, "tagsA");
            ValidateTags(tagsB, "tagsB");

            foreach (BitTag singleTagA in tagsA) {
                _collisionTagTable[singleTagA] -= tagsB;
            }

            foreach (BitTag singleTagB in tagsB) {
                _collisionTagTable[singleTagB] -= tagsA;
            }
        }

        public bool IsCollidable(BitTag tagsA, BitTag tagsB) {
            ValidateTags(tagsA, "tagsA");
            ValidateTags(tagsB, "tagsB");

            if (tagsA == BitTag.None || tagsB == BitTag.None) {
                return false;
            }

            foreach (BitTag singleTagA in tagsA) {
                if (_collisionTagTable[singleTagA].HasAny(tagsB)) {
                    return true;
                }
            }

            return false;
        }

        public void ClearCollisions() {
            BitTag[] keys = new BitTag[_collisionTagTable.Keys.Count];
            _collisionTagTable.Keys.CopyTo(keys, 0);

            foreach (BitTag collisionTag in keys) {
                _collisionTagTable[collisionTag] = new BitTag(0, collisionTag.EnumType);
            }
        }

        public void SetCollisions((System.Enum, System.Enum)[] collisions) {
            ClearCollisions();

            foreach ((System.Enum Tag, System.Enum With) collision in collisions) {
                RegisterCollision(collision.Tag, collision.With);
            }
        }

        public void SetCollisions((BitTag, BitTag)[] collisions) {
            ClearCollisions();

            foreach ((BitTag Tag, BitTag With) collision in collisions) {
                RegisterCollision(collision.Tag, collision.With);
            }
        }

        public void AddCollider(Body collider) {
            AddCollider(collider, collider.Tags);
            _colliders.Add(collider);
        }

        public void RemoveCollider(Body collider) {
            RemoveCollider(collider, collider.Tags);
            _colliders.Remove(collider);
        }

        public void ClearColliders() {
            foreach (List<Body> bodies in _collidersByTag.Values) {
                bodies.Clear();
            }

            _colliders.Clear();
        }

        public void UpdateColliderTagsEntry(Body collider, BitTag oldTags = default) {
            if (oldTags == BitTag.All || oldTags == BitTag.None) {
                foreach (KeyValuePair<BitTag, List<Body>> tagColliders in _collidersByTag) {
                    tagColliders.Value.Remove(collider);
                }
            } else {
                foreach (BitTag oldTag in oldTags) {
                    RemoveCollider(collider, oldTag);
                }
            }

            foreach (BitTag tag in collider.Tags) {
                if (!HasTag(tag)) {
                    throw new System.InvalidOperationException($"Tag '{tag}' (from Body Tags '{collider.Tags}', Enum Type: {collider.Tags.EnumType}) isn't registered.\nAccepted values are: {string.Join(", ", _collidersByTag.Keys)}");
                }

                AddCollider(collider, tag);
            }
        }

        public int GetCollidersCount(BitTag tags) {
            int collidersCount = 0;
            foreach (BitTag tag in tags) {
                ValidateTag(tag);
                collidersCount += _collidersByTag[tag].Count;
            }

            return collidersCount;
        }

        public override string ToString() {
            string info = $"Physics:\n  Colliders: {_colliders.Count}\n  Collision Tag Table:\n";
            foreach (KeyValuePair<BitTag, BitTag> entry in _collisionTagTable) {
                info += $"    {entry.Key} => {entry.Value}\n";
            }

            info += "  Colliders By Tag:\n";
            foreach (KeyValuePair<BitTag, List<Body>> tagColliders in _collidersByTag) {
                info += $"    {tagColliders.Key}: {tagColliders.Value.Count}\n";
            }

            return info;
        }

        #region Direct Collision Test

        public bool Collides(IShape shapeA, Vector2 positionA, IShape shapeB, Vector2 positionB, out ContactList contacts) {
            return CheckCollision(shapeA, positionA, shapeB, positionB, out contacts);
        }

        #endregion Direct Collision Test

        #region Queries [Single Output]

        public bool QueryCollision(IShape shape, Vector2 position, BitTag tags, out ContactList contacts) {
            if (tags == BitTag.None) {
                contacts = null;
                return false;
            }

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape || !otherCollider.Active || otherCollider.Entity == null || !otherCollider.Entity.Active) {
                        continue;
                    }

                    if (CheckCollision(shape, position - shape.Origin, otherCollider, out contacts)) {
                        return true;
                    }
                }
            }

            contacts = null;
            return false;
        }

        public bool QueryCollision(IShape shape, Vector2 position, BitTag tags, out CollisionInfo<Body> collisionInfo) {
            if (tags == BitTag.None) {
                collisionInfo = null;
                return false;
            }

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape || !otherCollider.Active || otherCollider.Entity == null || !otherCollider.Entity.Active) {
                        continue;
                    }

                    if (CheckCollision(shape, position - shape.Origin, otherCollider, out ContactList contacts)) {
                        collisionInfo = new CollisionInfo<Body>(otherCollider, contacts);
                        return true;
                    }
                }
            }

            collisionInfo = null;
            return false;
        }

        public bool QueryCollision<T>(IShape shape, Vector2 position, BitTag tags, out CollisionInfo<T> collisionInfo) where T : Entity {
            if (tags == BitTag.None) {
                collisionInfo = null;
                return false;
            }

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape || !otherCollider.Active || otherCollider.Entity == null || !otherCollider.Entity.Active) {
                        continue;
                    }

                    if (otherCollider.Entity is T && CheckCollision(shape, position - shape.Origin, otherCollider, out ContactList contacts)) {
                        collisionInfo = new CollisionInfo<T>(otherCollider.Entity as T, contacts);
                        return true;
                    }
                }
            }

            collisionInfo = null;
            return false;
        }

        #endregion Queries [Single Output]

        #region Queries [Multiple Output]

        public bool QueryMultipleCollision(IShape shape, Vector2 position, BitTag tags, out CollisionList<Body> collisionList) {
            collisionList = new CollisionList<Body>();

            if (tags == BitTag.None) {
                collisionList = null;
                return false;
            }

            HashSet<Body> verifiedBodies = new HashSet<Body>();

            foreach (BitTag tag in tags) {
                ValidateTag(tag);
                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape || !otherCollider.Active || otherCollider.Entity == null || !otherCollider.Entity.Active) {
                        continue;
                    }

                    // already verified
                    if (!verifiedBodies.Add(otherCollider)) {
                        continue;
                    }

                    if (CheckCollision(shape, position - shape.Origin, otherCollider, out ContactList contact)) {
                        collisionList.Add(otherCollider, contact);
                    }
                }
            }

            return collisionList.Count > 0;
        }

        public bool QueryMultipleCollision<T>(IShape shape, Vector2 position, BitTag tags, out CollisionList<T> collisionList) where T : Entity {
            collisionList = new CollisionList<T>();

            if (tags == BitTag.None) {
                return false;
            }

            HashSet<Body> verifiedBodies = new HashSet<Body>();

            foreach (BitTag tag in tags) {
                ValidateTag(tag);
                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape || !otherCollider.Active || otherCollider.Entity == null || !otherCollider.Entity.Active) {
                        continue;
                    }

                    // already verified
                    if (!verifiedBodies.Add(otherCollider)) {
                        continue;
                    }

                    if (otherCollider.Entity is T
                      && CheckCollision(shape, position - shape.Origin, otherCollider, out ContactList contacts)) {
                        collisionList.Add(otherCollider.Entity as T, contacts);
                    }
                }
            }

            return collisionList.Count > 0;
        }

        #endregion Queries [Multiple Output]

        #region Raycast [Single Output]

        public bool Raycast(Vector2 position, Vector2 direction, BitTag tags, out CollisionInfo<Body> collisionInfo, float maxDistance = float.MaxValue) {
            if (tags == BitTag.None) {
                collisionInfo = null;
                return false;
            }

            Body collidedCollider = null;

            Vector2 endPos = position + direction * maxDistance;
            Contact? rayContact = null;
            float closerContactDist = float.PositiveInfinity;

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == null || !otherCollider.Active || otherCollider.Entity == null || !otherCollider.Entity.Active) {
                        continue;
                    }

                    Contact? contact = null;

                    if (otherCollider.Shape is GridShape gridShape) {
                        List<Contact> gridContacts = TestGrid(gridShape, otherCollider.Position, new Rectangle(position, position + direction * maxDistance),
                            (Polygon tilePolygon) => {
                                SAT.Test(position, endPos, tilePolygon, out Contact? tileContact);
                                return tileContact;
                            }
                        );

                        if (gridContacts.Count <= 0) {
                            continue;
                        }

                        // find closest contact
                        float closestContactSqrDist = float.PositiveInfinity;
                        foreach (Contact c in gridContacts) {
                            float dist = Math.DistanceSquared(position, c.Position);
                            if (dist < closestContactSqrDist) {
                                contact = c;
                                closestContactSqrDist = dist;
                            }
                        }
                    } else {
                        if (!SAT.Test(position, endPos, otherCollider.Shape, otherCollider.Position, out contact) || contact == null) {
                            continue;
                        }
                    }

                    //float distToContact = Vector2.Dot(direction, contact.Value.Position - position);
                    float distToContact = Math.DistanceSquared(position, contact.Value.Position);
                    if (rayContact == null || distToContact < closerContactDist) {
                        rayContact = contact;
                        collidedCollider = otherCollider;
                        closerContactDist = distToContact;
                    }
                }
            }

            if (rayContact != null) {
                collisionInfo = new CollisionInfo<Body>(collidedCollider, rayContact.Value);
                return true;
            }

            collisionInfo = null;
            return false;
        }

        public bool Raycast(Vector2 position, Vector2 direction, BitTag tags, out ContactList contacts, float maxDistance = float.MaxValue) {
            bool hit = Raycast(position, direction, tags, out CollisionInfo<Body> collisionInfo, maxDistance);

            if (collisionInfo != null) {
                contacts = collisionInfo.Contacts;
            } else {
                contacts = null;
            }

            return hit;
        }

        public bool Raycast<T>(Vector2 position, Vector2 direction, BitTag tags, out CollisionInfo<T> collisionInfo, float maxDistance = float.MaxValue) where T : Entity {
            if (Raycast(position, direction, tags, out CollisionInfo<Body> collidedCollider, maxDistance)
              && collidedCollider.Subject.Entity is T) {
                collisionInfo = new CollisionInfo<T>(collidedCollider.Subject.Entity as T, collidedCollider.Contacts);
                return true;
            }

            collisionInfo = null;
            return false;
        }

        #endregion Raycast [Single Output]

        #region Raycast [Multiple Output]

        public bool RaycastMultiple(Vector2 position, Vector2 direction, BitTag tags, out CollisionList<Body> collisionList, float maxDistance = float.MaxValue) {
            collisionList = new CollisionList<Body>();

            if (tags == BitTag.None) {
                return false;
            }

            Vector2 endPos = position + direction * maxDistance;

            HashSet<Body> verifiedBodies = new HashSet<Body>();

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == null) {
                        continue;
                    }

                    // already verified
                    if (!verifiedBodies.Add(otherCollider)) {
                        continue;
                    }

                    if (otherCollider.Shape is GridShape gridShape) {
                            List<Contact> gridContacts = TestGrid(
                            gridShape,
                            otherCollider.Position,
                            new Rectangle(position, endPos),
                            (Polygon tilePolygon) => {
                                if (!SAT.Test(position, endPos, tilePolygon, out Contact? tileContact)) {
                                    return null;
                                }

                                return tileContact;
                            }
                        );

                        if (gridContacts.Count <= 0) {
                            continue;
                        }

                        collisionList.Add(otherCollider, gridContacts);
                        continue;
                    }

                    if (!SAT.Test(position, endPos, otherCollider.Shape, otherCollider.Position, out Contact? contact) || contact == null) {
                        continue;
                    }

                    collisionList.Add(otherCollider, contact.Value);
                }
            }

            return collisionList.Count > 0;
        }


        public bool RaycastMultiple<T>(Vector2 position, Vector2 direction, BitTag tags, out CollisionList<T> collisionList, float maxDistance = float.MaxValue) where T : Entity {
            collisionList = new CollisionList<T>();

            if (tags == BitTag.None) {
                return false;
            }

            Vector2 endPos = position + direction * maxDistance;

            HashSet<Body> verifiedBodies = new HashSet<Body>();

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == null || !(otherCollider.Entity is T entity)) {
                        continue;
                    }

                    // already verified
                    if (!verifiedBodies.Add(otherCollider)) {
                        continue;
                    }

                    Contact? contact = null;

                    if (otherCollider.Shape is GridShape gridShape) {
                        List<Contact> gridContacts = TestGrid(gridShape, otherCollider.Position, new Rectangle(position, position + direction * maxDistance),
                            (Polygon tilePolygon) => {
                                SAT.Test(position, endPos, tilePolygon, out Contact? tileContact);
                                return tileContact;
                            }
                        );

                        if (gridContacts.Count <= 0) {
                            continue;
                        }

                        // find closest contact
                        float closestContactSqrDist = float.PositiveInfinity;
                        foreach (Contact c in gridContacts) {
                            float dist = Math.DistanceSquared(position, c.Position);
                            if (dist < closestContactSqrDist) {
                                contact = c;
                                closestContactSqrDist = dist;
                            }
                        }
                    } else {
                        if (!SAT.Test(position, endPos, otherCollider.Shape, otherCollider.Position, out contact) || contact == null) {
                            continue;
                        }
                    }

                    collisionList.Add(entity, contact.Value);
                }
            }

            return collisionList.Count > 0;
        }

        #endregion Raycast [Multiple Output]

        #region Boxcast [Single Output]

        public bool Boxcast(Vector2 position, Vector2 direction, float width, BitTag tags, out CollisionInfo<Body> collisionInfo, float maxDistance = float.MaxValue) {
            if (tags == BitTag.None) {
                collisionInfo = null;
                return false;
            }

            Body collidedCollider = null;

            //Vector2 endPos = position + direction * maxDistance;
            Contact? rayContact = null;
            float closerContactDist = float.PositiveInfinity;

            Polygon box;

            {
                Vector2 normal = direction.PerpendicularCCW();
                float halfWidth = width / 2f;

                box = new Polygon(
                    position - normal * halfWidth,
                    position + normal * halfWidth,
                    position + direction * maxDistance + normal * halfWidth,
                    position + direction * maxDistance - normal * halfWidth
                );
            }

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == null || !otherCollider.Active || otherCollider.Entity == null || !otherCollider.Entity.Active) {
                        continue;
                    }

                    Contact? contact = null;

                    if (otherCollider.Shape is GridShape gridShape) {
                        List<Contact> gridContacts = TestGrid(gridShape, otherCollider.Position, box.BoundingBox(),//new Rectangle(position, position + direction * maxDistance),
                            (Polygon tilePolygon) => {
                                SAT.Test(box, tilePolygon, out Contact? tileContact);
                                return tileContact;
                            }
                        );

                        if (gridContacts.Count <= 0) {
                            continue;
                        }

                        // find closest contact
                        float closestContactSqrDist = float.PositiveInfinity;
                        foreach (Contact c in gridContacts) {
                            float dist = Math.DistanceSquared(position, c.Position);
                            if (dist < closestContactSqrDist) {
                                contact = c;
                                closestContactSqrDist = dist;
                            }
                        }
                    } else {
                        if (!SAT.Test(otherCollider.Shape, otherCollider.Position, box, out contact) || contact == null) {
                            continue;
                        }
                    }

                    //float distToContact = Vector2.Dot(direction, contact.Value.Position - position);
                    float distToContact = Math.DistanceSquared(position, contact.Value.Position);
                    if (rayContact == null || distToContact < closerContactDist) {
                        rayContact = contact;
                        collidedCollider = otherCollider;
                        closerContactDist = distToContact;
                    }
                }
            }

            if (rayContact != null) {
                collisionInfo = new CollisionInfo<Body>(collidedCollider, rayContact.Value);
                return true;
            }

            collisionInfo = null;
            return false;
        }

        #endregion Boxcast [Single Output]

        #endregion Public Methods

        #region Private Methods

        private void Step(float dt) {
#if DEBUG
            _colliders.ForEach((Body body) => body.Color = Graphics.Color.White);
            CollidersBroadPhaseCount = _colliders.Count;
            Time.StartStopwatch();
#endif

            // Broad Phase
            // TODO: Make a real broad phase algorithm, probaly using a quadtree
            _narrowPhaseBodies.Clear();
            _narrowPhaseBodies.AddRange(_colliders);

#if DEBUG
            CollisionDetectionBroadPhaseExecutionTime = Time.EndStopwatch();
            CollidersNarrowPhaseCount = _narrowPhaseBodies.Count;
            Time.StartStopwatch();
#endif

            // Narrow Phase
            // solve constraints
            /*
            for (int k = 0; k < ConstraintSolverAccuracy; k++) {
                foreach (Body body in _narrowPhaseBodies) {
                    body.SolveConstraints();
                }
            }
            */

            // movement with collision detection
            for (int j = 0; j < _narrowPhaseBodies.Count; j++) {
                Body body = _narrowPhaseBodies[j];

                if (!body.Active || body.Entity == null || !body.Entity.Active) {
                    continue;
                }

                body.PhysicsUpdate(dt);

                // check if Body is static
                if (body.Movement == null && !body.AllowIndependentCollisionChecksAsStatic) {
                    // early exit, static Body, well.. should remain static
                    continue;
                }

                // swap bodies for fast collision check
                _narrowPhaseBodies[j] = _narrowPhaseBodies[0];
                _narrowPhaseBodies[0] = body;

                Vector2 nextPosition = body.Integrate(dt);

                // prepare collidable tags
                BitTag bodyCollidableTags = GetCollidableTags(body.Tags),
                       movementCollidableTags = body.Movement == null ? BitTag.None : body.Movement.Tags;

                // initial body vars
                Vector2 bodyPos = body.Position;
                int startX = PreciseConvertToInt32(bodyPos.X),
                    startY = PreciseConvertToInt32(bodyPos.Y),
                    currentX = startX,
                    currentY = startY;

                double diffX = (nextPosition.X + body.MoveBufferX) - currentX,
                       diffY = (nextPosition.Y + body.MoveBufferY) - currentY;

                // signed distance in pixels
                int distanceX = Math.Sign(diffX) * (int) Math.Truncate(Math.Abs(diffX)),
                    distanceY = Math.Sign(diffY) * (int) Math.Truncate(Math.Abs(diffY));

                double directionX = Math.Sign(distanceX),
                       directionY = Math.Sign(distanceY);

                int movementX = 0,
                    movementY = 0,
                    fixedMovementX = 0,
                    fixedMovementY = 0;

                double movementXBuffer = diffX - Math.Truncate(diffX),
                       movementYBuffer = diffY - Math.Truncate(diffY);

                bool canMoveH = true,
                     canMoveV = true;

                // try to attribute a movement value when distance will be zero
                // very small velocities will be ignored otherwise
                // and cause problems when trying to track continuous movement

                if (distanceX == 0 && !Math.EqualsEstimate(body.Velocity.X, 0f)) {
                    fixedMovementX = Math.Sign(body.Velocity.X);
                }

                if (distanceY == 0 && !Math.EqualsEstimate(body.Velocity.Y, 0f)) {
                    fixedMovementY = Math.Sign(body.Velocity.Y);
                }

                do {
                    if (canMoveH && Math.Abs(distanceX) >= 1) {
                        movementXBuffer += directionX;
                        // check movement buffer X for a valid movement
                        if (Math.Abs(movementXBuffer) >= 1.0) {
                            movementX = Math.Sign(movementXBuffer);
                            movementXBuffer -= movementX;
                        }
                    }

                    if (canMoveV && Math.Abs(distanceY) >= 1) {
                        movementYBuffer += directionY;
                        // check movement buffer Y for a valid movement
                        if (Math.Abs(movementYBuffer) >= 1.0) {
                            movementY = Math.Sign(movementYBuffer);
                            movementYBuffer -= movementY;
                        }
                    }

                    // check collision with current movement
                    Vector2 moveHorizontalPos = new Vector2(
                                                    currentX + movementX + fixedMovementX,
                                                    canMoveV ? (currentY + movementY) : currentY // NOTE  fixedMovementY should not be included here
                                                ),                                               //       it can cause undesired effects, since it'll
                                                                                                 //       affect collision direction
                                                                                                 //
                                                                                                 //       e.g  straight horizontal movement could be
                                                                                                 //       interpreted as down-side collision and
                                                                                                 //       make it stutter

                            moveVerticalPos   = new Vector2(
                                                    currentX + fixedMovementX,
                                                    currentY + movementY + fixedMovementY
                                                );

                    Vector2 movement = new Vector2(movementX + fixedMovementX, movementY + fixedMovementY);

                    if (bodyCollidableTags != BitTag.None || movementCollidableTags != BitTag.None) {
                        for (int k = 1; k < _narrowPhaseBodies.Count; k++) {
                            Body otherBody = _narrowPhaseBodies[k];

                            if (!otherBody.Active || otherBody.Entity == null || !otherBody.Entity.Active) {
                                continue;
                            }

                            bool isBodyCollidable     = otherBody.Tags.HasAny(bodyCollidableTags),
                                 isMovementCollidable = otherBody.Tags.HasAny(movementCollidableTags);

                            // checks if otherBody tags contains at least one body or movement collidable tag
                            if (!isBodyCollidable && !isMovementCollidable) {
                                continue;
                            }

                            List<Contact> horizontalContacts = new List<Contact>(),
                                          verticalContacts = new List<Contact>();

                            // vertical collision check
                            if (CheckCollision(body.Shape, moveVerticalPos, otherBody, out ContactList contactsV)) {
                                foreach (Contact c in contactsV) {
                                    verticalContacts.Add(c);
                                }

                                if (canMoveV && !Math.EqualsEstimate(movement.Y, 0)) {
                                    if (isMovementCollidable
                                     && contactsV.FindIndex(FilterValidVerticalContact) >= 0
                                     && body.Movement.CanCollideWith(new Vector2(0f, movement.Y), new CollisionInfo<Body>(otherBody, verticalContacts.ToArray()))
                                    ) {
                                        canMoveV = false;
                                        distanceY = 0;
                                        directionX = Math.Sign(directionX);
                                        moveHorizontalPos.Y = currentY;
                                    }
                                }

                                if (otherBody.Entity == null) {
                                    continue;
                                }
                            }

                            // horizontal collision check
                            if (CheckCollision(body.Shape, moveHorizontalPos, otherBody, out ContactList contactsH)) {
                                foreach (Contact c in contactsH) {
                                    horizontalContacts.Add(c);
                                }

                                if (canMoveH && !Math.EqualsEstimate(movement.X, 0f)) {
                                    if (isMovementCollidable
                                     && contactsH.FindIndex(FilterValidHorizontalContact) >= 0
                                     && body.Movement.CanCollideWith(new Vector2(movement.X, 0f), new CollisionInfo<Body>(otherBody, horizontalContacts.ToArray()))
                                    ) {
                                        canMoveH = false;
                                        distanceX = 0;
                                        directionY = Math.Sign(directionY);
                                        moveVerticalPos.X = currentX;
                                    }
                                }

                                if (otherBody.Entity == null) {
                                    continue;
                                }
                            }

                            // submiting to resolution
                            if (horizontalContacts.Count > 0 || verticalContacts.Count > 0) {
                                // body.PhysicsCollisionCheck();
                                InternalCollisionInfo previousCollisionInfo = null;

                                foreach (InternalCollisionInfo ci in _internalCollisionInfo) {
                                    if (Helper.EqualsPermutation(body, otherBody, ci.BodyA, ci.BodyB)) {
                                        previousCollisionInfo = ci;
                                        break;
                                    }
                                }

                                if (previousCollisionInfo == null) {
                                    _internalCollisionInfo.Add(new InternalCollisionInfo(body, otherBody, movement, horizontalContacts, verticalContacts));
                                } else {
                                    if (body == previousCollisionInfo.BodyB) {
                                        // we need to invert contact normals

                                        foreach (Contact c in horizontalContacts) {
                                            previousCollisionInfo.HorizontalContacts.Add(Contact.Invert(c));
                                        }

                                        foreach (Contact c in verticalContacts) {
                                            previousCollisionInfo.VerticalContacts.Add(Contact.Invert(c));
                                        }
                                    } else {
                                        previousCollisionInfo.HorizontalContacts.AddRange(horizontalContacts);
                                        previousCollisionInfo.VerticalContacts.AddRange(verticalContacts);
                                    }
                                }

                                body.PhysicsCollisionSubmit(otherBody, movement, horizontalContacts, verticalContacts);
                                //otherBody.PhysicsCollisionSubmit(body, new Vector2(- movementX, - movementY), hContacts, vContacts);

                                _collidedOnThisFrame.Add(body);
                                _collidedOnThisFrame.Add(otherBody);
                            }

                            // body position could be manually changed
                            if ((int) Math.Floor(body.Position.X) != currentX) {
                                currentX = (int) Math.Floor(body.Position.X);
                            }

                            if ((int) Math.Floor(body.Position.Y) != currentY) {
                                currentY = (int) Math.Floor(body.Position.Y);
                            }
                        }
                    }

                    // separated axis movement
                    if (canMoveV && movementY != 0) {
                        if (body.Movement.CanExecuteMove(0, movementY)) {
                            distanceY -= movementY;
                            currentY += movementY;
                            body.Position = body.Position.WithY(currentY);
                        } else {
                            canMoveV = false;
                        }
                    }

                    if (canMoveH && movementX != 0) {
                        if (body.Movement.CanExecuteMove(movementX, 0)) {
                            distanceX -= movementX;
                            currentX += movementX;
                            body.Position = body.Position.WithX(currentX);
                        } else {
                            canMoveH = false;
                        }
                    }

                    if (movementX != 0 || movementY != 0) {
                        body.PhysicsStepMove(
                            canMoveH ? movementX : 0,
                            canMoveV ? movementY : 0
                        );
                    }

                    movementX = movementY = 0;
                } while (
                     (canMoveH && Math.Abs(distanceX) >= 1)
                  || (canMoveV && Math.Abs(distanceY) >= 1)
                );

                // check if body has been removed in collision resolution and just ignore the post-processing
                if (body.Entity == null) {
                    continue;
                }

                // checks for movement buffer to return to the body
                double remainderMovementXBuffer = 0.0,
                       remainderMovementYBuffer = 0.0;

                if (canMoveH && Math.Abs(distanceX + movementXBuffer) > 0) {
                    remainderMovementXBuffer = distanceX + movementXBuffer;
                }

                if (canMoveV && Math.Abs(distanceY + movementYBuffer) > 0) {
                    remainderMovementYBuffer = distanceY + movementYBuffer;
                }

                body.MoveBufferX = remainderMovementXBuffer;
                body.MoveBufferY = remainderMovementYBuffer;
                body.Position = new Vector2(currentX, currentY);
            }

            foreach (Body body in _collidedOnThisFrame) {
                body.BeforeSolveCollisions();
            }

            // solve collisions
            foreach (InternalCollisionInfo collInfo in _internalCollisionInfo) {
                Contact[] horizontalContacts = collInfo.HorizontalContacts.ToArray(),
                          verticalContacts = collInfo.VerticalContacts.ToArray();

                collInfo.BodyA.CollidedWith(
                    collInfo.BodyB,
                    collInfo.Movement,
                    new CollisionInfo<Body>(collInfo.BodyB, horizontalContacts),
                    new CollisionInfo<Body>(collInfo.BodyB, verticalContacts)
                );

                Contact[] invertedHContacts = new Contact[horizontalContacts.Length],
                          invertedVContacts = new Contact[verticalContacts.Length];

                for (int i = 0; i < invertedHContacts.Length; i++) {
                    invertedHContacts[i] = Contact.Invert(horizontalContacts[i]);
                }

                for (int i = 0; i < invertedVContacts.Length; i++) {
                    invertedVContacts[i] = Contact.Invert(verticalContacts[i]);
                }

                collInfo.BodyB.CollidedWith(
                    collInfo.BodyA,
                    -collInfo.Movement,
                    new CollisionInfo<Body>(collInfo.BodyA, invertedHContacts),
                    new CollisionInfo<Body>(collInfo.BodyA, invertedVContacts)
                );
            }

            _internalCollisionInfo.Clear();

            foreach (Body body in _collidedOnThisFrame) {
                body.AfterSolveCollisions();
            }

            _collidedOnThisFrame.Clear();

            for (int j = 0; j < _narrowPhaseBodies.Count; j++) {
                Body body = _narrowPhaseBodies[j];
                body.PhysicsLateUpdate(dt);
            }

#if DEBUG
            CollisionDetectionNarrowPhaseExecutionTime = Time.EndStopwatch();
#endif
        }

        private int PreciseConvertToInt32(float n) {
            return (int) (Math.Sign(n) * Math.Floor(Math.Abs(n) + Math.Epsilon));
        }

        private bool FilterValidVerticalContact(Contact c) {
            return Math.Abs(Vector2.Dot(c.Normal, Vector2.Down)) >= .6f && c.PenetrationDepth > 0.5f;
        }

        private bool FilterValidHorizontalContact(Contact c) {
            return (
                Math.Abs(Vector2.Dot(c.Normal, Vector2.Right)) >= .6f
                || Math.Abs(Vector2.Dot(c.Normal, Vector2.Down)) >= .6f
            ) && c.PenetrationDepth > 0.5f;
        }

        private void AddCollider(Body collider, BitTag tags) {
            foreach (BitTag tag in tags) {
                if (!_collidersByTag.TryGetValue(tag, out List<Body> collidersByTag)) {
                    throw new System.InvalidOperationException($"Tag '{tag}' (from Tags '{tags}') isn't registered.\nAccepted values are: {string.Join(", ", _collidersByTag.Keys)}");
                }

                if (collidersByTag.Contains(collider)) {
                    continue;
                }

                collidersByTag.Add(collider);
            }
        }

        private void RemoveCollider(Body collider, BitTag tags) {
            foreach (BitTag tag in tags) {
                ValidateTag(tag);
                _collidersByTag[tag].Remove(collider);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void ValidateTag(BitTag tag, string paramName = "tag") {
            if (!_collidersByTag.ContainsKey(tag)) {
                throw new System.ArgumentException($"Tag '{tag}' ({tag.BinaryRepresentation}) not found.", paramName);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void ValidateTags(BitTag tags, string paramName = "tags") {
            foreach (BitTag tag in tags) {
                ValidateTag(tag, paramName);
            }
        }

        #endregion Private Methods

        #region Internal Methods

        internal void PrepareUpdate(int delta) {
            // TODO: update using a float delta (increases precision)
            Timesteps = (int) System.Math.Floor((delta + _leftOverDeltaTime) / (float) FixedDeltaTime);
            Timesteps = Math.Min(5, Timesteps); // prevents freezing
            _leftOverDeltaTime = Math.Max(0, delta - (Timesteps * FixedDeltaTime));
        }

        internal void Update() {
            Step(FixedDeltaTimeSeconds);
        }

        internal void CompleteUpdate() {
            Timesteps = 0;
        }

        internal bool CheckCollision(IShape A, Vector2 APos, IShape B, Vector2 BPos, out ContactList contacts) {
            bool ret = _collisionFunctions[A.GetType()][B.GetType()](A, APos, B, BPos, out Contact[] c);
            contacts = new ContactList(c);
            return ret;
        }

        internal bool CheckCollision(IShape A, Vector2 APos, Body B, out ContactList contacts) {
            return CheckCollision(A, APos, B.Shape, B.Position, out contacts);
        }

        internal bool CheckCollision(Body A, Body B, out ContactList contacts) {
            return CheckCollision(A.Shape, A.Position, B.Shape, B.Position, out contacts);
        }

#if DEBUG
        internal void ClearTimers() {
            UpdatePositionExecutionTime = SolveConstraintsExecutionTime = CollisionDetectionBroadPhaseExecutionTime
              = CollisionDetectionNarrowPhaseExecutionTime = 0;
        }
#endif

        #endregion InternalMethods

    }
}
