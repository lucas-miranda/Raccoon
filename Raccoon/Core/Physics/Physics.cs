﻿using System.Collections.Generic;

using Raccoon.Components;
using Raccoon.Util;

namespace Raccoon {
    public sealed partial class Physics {
        #region Public Members

        public static float FixedDeltaTimeSeconds = 1f / 60f;
        public static int FixedDeltaTime = (int) (FixedDeltaTimeSeconds * 1000f);
        public static int ConstraintSolverAccuracy = 3;

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

        #region Internal Static Properties

#if DEBUG
        internal static long UpdatePositionExecutionTime { get; private set; }
        internal static long SolveConstraintsExecutionTime { get; private set; }
        internal static long CollisionDetectionBroadPhaseExecutionTime { get; private set; }
        internal static long CollisionDetectionNarrowPhaseExecutionTime { get; private set; }
        internal static int CollidersBroadPhaseCount { get; private set; }
        internal static int CollidersNarrowPhaseCount { get; private set; }
#endif

        #endregion Internal Static Properties

        #region Public Methods

        public void Update(int delta) {
            if (!IsRunning) {
                return;
            }

            // TODO: update using a float delta (increases precision)
            int timesteps = (int) System.Math.Floor((delta + _leftOverDeltaTime) / (float) FixedDeltaTime);
            timesteps = Math.Min(5, timesteps); // prevents freezing
            _leftOverDeltaTime = Math.Max(0, delta - (timesteps * FixedDeltaTime));

            for (int i = 0; i < timesteps; i++) {
                Step(FixedDeltaTimeSeconds);
            }
        }

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

        public void RegisterTags<T>() where T : System.Enum {
            System.Type tagType = typeof(T);

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

        public void SetCollisions((BitTag, BitTag)[] collisions) {
            ClearCollisions();

            foreach ((BitTag tag, BitTag collisionWith) in collisions) {
                RegisterCollision(tag, collisionWith);
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

        public void UpdateColliderTagsEntry(Body collider, BitTag oldTags = default(BitTag)) {
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

        #region Queries [Single Output]

        public bool QueryCollision(IShape shape, Vector2 position, BitTag tags, out ContactList contacts) {
            if (tags == BitTag.None) {
                contacts = null;
                return false;
            }

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape) {
                        continue;
                    }

                    if (CheckCollision(shape, position, otherCollider, out contacts)) {
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
                    if (otherCollider.Shape == shape) {
                        continue;
                    }

                    if (CheckCollision(shape, position, otherCollider, out ContactList contacts)) {
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
                    if (otherCollider.Shape == shape) {
                        continue;
                    }

                    if (otherCollider.Entity is T && CheckCollision(shape, position, otherCollider, out ContactList contacts)) {
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

            foreach (BitTag tag in tags) {
                ValidateTag(tag);
                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape) {
                        continue;
                    }

                    if (CheckCollision(shape, position, otherCollider, out ContactList contact)) {
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

            foreach (BitTag tag in tags) {
                ValidateTag(tag);
                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape) {
                        continue;
                    }

                    if (otherCollider.Entity is T 
                      && CheckCollision(shape, position, otherCollider, out ContactList contacts)) {
                        collisionList.Add(otherCollider.Entity as T, contacts);
                    }
                }
            }

            return collisionList.Count > 0;
        }

        #endregion Queries [Multiple Output]

        #region Raycast [Single Output]

        public bool Raycast(Vector2 position, Vector2 direction, BitTag tags, out CollisionInfo<Body> collisionInfo, float maxDistance = float.PositiveInfinity) {
            if (tags == BitTag.None) {
                collisionInfo = null;
                return false;
            }

            Body collidedCollider = null;

            Vector2 endPos = position + direction * maxDistance;
            Contact? rayContact = null;
            float closerContactDist = float.PositiveInfinity;

            Vector2[] raycastAxes = new Vector2[] {
                direction.Normalized(),
                direction.PerpendicularCW().Normalized()
            };

            Vector2[] axes, shapeAxes;

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == null) {
                        continue;
                    }

                    shapeAxes = otherCollider.Shape.CalculateAxes();

                    // prepare axes
                    axes = new Vector2[raycastAxes.Length + shapeAxes.Length];
                    raycastAxes.CopyTo(axes, 0);
                    shapeAxes.CopyTo(axes, raycastAxes.Length);

                    Contact? contact = null;

                    switch (otherCollider.Shape) {
                        case GridShape gridShape:
                            List<Contact> gridContacts = TestGrid(gridShape, otherCollider.Position, new Rectangle(position, position + direction * maxDistance), 
                                (Polygon tilePolygon) => {
                                    TestSAT(position, endPos, tilePolygon, axes, out Contact? tileContact);
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

                            break;

                        default:
                            if (!TestSAT(position, endPos, otherCollider.Shape, otherCollider.Position, axes, out contact) || contact == null) {
                                continue;
                            }

                            break;
                    }

                    float distToContact = Vector2.Dot(direction, contact.Value.Position - position);
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

        public bool Raycast(Vector2 position, Vector2 direction, BitTag tags, out ContactList contacts, float maxDistance = float.PositiveInfinity) {
            bool hit = Raycast(position, direction, tags, out CollisionInfo<Body> collisionInfo, maxDistance);
            contacts = collisionInfo.Contacts;
            return hit;
        }

        public bool Raycast<T>(Vector2 position, Vector2 direction, BitTag tags, out CollisionInfo<T> collisionInfo, float maxDistance = float.PositiveInfinity) where T : Entity {
            if (Raycast(position, direction, tags, out CollisionInfo<Body> collidedCollider, maxDistance)
              && collidedCollider.Subject is T) {
                collisionInfo = new CollisionInfo<T>(collidedCollider.Subject as T, collidedCollider.Contacts);
                return true;
            }

            collisionInfo = null;
            return false;
        }

        #endregion Raycast [Single Output]

        #region Raycast [Multiple Output]

        public bool RaycastMultiple(Vector2 position, Vector2 direction, BitTag tags, out CollisionList<Body> collisionList, float maxDistance = float.PositiveInfinity) {
            collisionList = new CollisionList<Body>();

            if (tags == BitTag.None) {
                return false;
            }

            Vector2 endPos = position + direction * maxDistance;

            Vector2[] raycastAxes = new Vector2[] {
                direction.Normalized(),
                direction.PerpendicularCW().Normalized()
            };

            Vector2[] axes, shapeAxes;
            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == null) {
                        continue;
                    }

                    shapeAxes = otherCollider.Shape.CalculateAxes();

                    // prepare axes
                    axes = new Vector2[raycastAxes.Length + shapeAxes.Length];
                    raycastAxes.CopyTo(axes, 0);
                    shapeAxes.CopyTo(axes, raycastAxes.Length);

                    if (!TestSAT(position, endPos, otherCollider.Shape, otherCollider.Position, axes, out Contact? contact) || contact == null) {
                        continue;
                    }

                    collisionList.Add(otherCollider, contact.Value);
                }
            }

            return collisionList.Count > 0;
        }


        public bool RaycastMultiple<T>(Vector2 position, Vector2 direction, BitTag tags, out CollisionList<T> collisionList, float maxDistance = float.PositiveInfinity) where T : Entity {
            collisionList = new CollisionList<T>();

            if (tags == BitTag.None) {
                return false;
            }

            Vector2 endPos = position + direction * maxDistance;

            Vector2[] raycastAxes = new Vector2[] {
                direction.Normalized(),
                direction.PerpendicularCW().Normalized()
            };

            Vector2[] axes, shapeAxes;
            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == null || !(otherCollider.Entity is T entity)) {
                        continue;
                    }

                    shapeAxes = otherCollider.Shape.CalculateAxes();

                    // prepare axes
                    axes = new Vector2[raycastAxes.Length + shapeAxes.Length];
                    raycastAxes.CopyTo(axes, 0);
                    shapeAxes.CopyTo(axes, raycastAxes.Length);

                    Contact? contact = null;

                    switch (otherCollider.Shape) {
                        case GridShape gridShape:
                            List<Contact> gridContacts = TestGrid(gridShape, otherCollider.Position, new Rectangle(position, position + direction * maxDistance), 
                                (Polygon tilePolygon) => {
                                    TestSAT(position, endPos, tilePolygon, axes, out Contact? tileContact);
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

                            break;

                        default:
                            if (!TestSAT(position, endPos, otherCollider.Shape, otherCollider.Position, axes, out contact) || contact == null) {
                                continue;
                            }

                            break;
                    }

                    collisionList.Add(entity, contact.Value);
                }
            }

            return collisionList.Count > 0;
        }

        #endregion Raycast [Multiple Output]

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
            for (int k = 0; k < ConstraintSolverAccuracy; k++) {
                foreach (Body body in _narrowPhaseBodies) {
                    body.SolveConstraints();
                }
            }

            // movement with collision detection
            for (int j = 0; j < _narrowPhaseBodies.Count; j++) {
                Body body = _narrowPhaseBodies[j];

                if (body.Entity == null) {
                    continue;
                }

                body.PhysicsUpdate(dt);

                // swap bodies for fast collision check
                _narrowPhaseBodies[j] = _narrowPhaseBodies[0];
                _narrowPhaseBodies[0] = body;

                Vector2 nextPosition = body.Integrate(dt);

                // prepare collidable tags
                BitTag bodyCollidableTags = GetCollidableTags(body.Tags),
                       movementCollidableTags = body.Movement == null ? BitTag.None : body.Movement.CollisionTags;

                // initial body vars
                int currentX = (int) body.Position.X,
                    currentY = (int) body.Position.Y;

                double diffX = (nextPosition.X + body.MoveBufferX) - currentX,
                       diffY = (nextPosition.Y + body.MoveBufferY) - currentY;

                // early exit if next and current positions are the same
                /*if (Math.EqualsEstimate(Math.DistanceSquared(diffX, diffY, diffX, diffY), 0.0)) {
                    body.MoveBufferX = body.MoveBufferY = 0.0;
                    body.PhysicsLateUpdate();
                    continue;
                }*/

                // signed distance in pixels
                int distanceX = Math.Sign(diffX) * (int) Math.Truncate(Math.Abs(diffX)),
                    distanceY = Math.Sign(diffY) * (int) Math.Truncate(Math.Abs(diffY));

                // I'm using the greatest distance axis to find a relation to move the body each loop by 1px at least
                double directionX = 0,
                       directionY = 0,
                       dxAbs = Math.Abs(distanceX),
                       dyAbs = Math.Abs(distanceY);

                if (Math.EqualsEstimate(dxAbs, dyAbs)) {
                    directionX = Math.Sign(distanceX);
                    directionY = Math.Sign(distanceY);
                } else if (dxAbs > dyAbs) {
                    directionX = Math.Sign(distanceX);
                    directionY = distanceY / dxAbs;
                } else if (dxAbs < dyAbs) {
                    directionX = distanceX / dyAbs;
                    directionY = Math.Sign(distanceY);
                }

                int movementX = 0,
                    movementY = 0;

                double movementXBuffer = diffX - Math.Truncate(diffX),
                       movementYBuffer = diffY - Math.Truncate(diffY);

                bool canMoveH = true, 
                     canMoveV = true,
                     singleCheck = distanceX == 0 && distanceY == 0;

                if (singleCheck) {
                    movementX = Math.Sign(movementXBuffer);
                    movementY = Math.Sign(movementYBuffer);
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
                    Vector2 moveHorizontalPos = new Vector2(currentX + movementX, currentY),
                            moveVerticalPos   = new Vector2(
                                                    canMoveH ? (currentX + movementX) : currentX, 
                                                    currentY + movementY
                                                ); // moveVerticalPos will do a diagonal move check, if canMoveH is true

                    if (bodyCollidableTags != BitTag.None || movementCollidableTags != BitTag.None) {
                        for (int k = 1; k < _narrowPhaseBodies.Count; k++) {
                            Body otherBody = _narrowPhaseBodies[k];

                            bool isBodyCollidable     = otherBody.Tags.HasAny(bodyCollidableTags),
                                 isMovementCollidable = otherBody.Tags.HasAny(movementCollidableTags);

                            // checks if otherBody tags contains at least one body or movement collidable tag
                            if (!isBodyCollidable && !isMovementCollidable) {
                                continue;
                            }

                            bool collidedH = false, 
                                 collidedV = false;

                            ContactList contactsH = new ContactList(),
                                        contactsV = new ContactList();

                            // test for horizontal collision (if it's moving horizontally)
                            if ((singleCheck || movementX != 0)
                              && CheckCollision(body.Shape, moveHorizontalPos, otherBody, out contactsH)) {
                                if (singleCheck) {
                                    collidedH = true;
                                }

                                collidedH = true;
                                if (contactsH.Contains(c => c.PenetrationDepth > 0f)) {
                                    if (isMovementCollidable && body.Movement.CanCollideWith(new Vector2(movementX, 0f), new CollisionInfo<Body>(otherBody, contactsH))) {
                                        canMoveH = false;
                                        distanceX = 0;
                                        directionY = Math.Sign(directionY);
                                        moveVerticalPos.X = currentX;
                                    }
                                }
                            }

                            // test for vertical collision (if it's moving vertically)
                            if ((singleCheck || movementY != 0)
                              && CheckCollision(body.Shape, moveVerticalPos, otherBody, out contactsV)) {
                                if (singleCheck) {
                                    collidedV = true;
                                }

                                collidedV = true;
                                if (contactsV.Contains(c => c.PenetrationDepth > 0f)) {
                                    if (isMovementCollidable && body.Movement.CanCollideWith(new Vector2(canMoveH ? movementX : 0, movementY), new CollisionInfo<Body>(otherBody, contactsV))) {
                                    //if (isMovementCollidable) {
                                        canMoveV = false;
                                        distanceY = 0;
                                        directionX = Math.Sign(directionX);
                                    }
                                }
                            }

                            if (collidedH || collidedV) {
                                // stop moving
                                Vector2 collisionAxes = Vector2.Zero;

                                if (singleCheck) {
                                    collisionAxes.X = !canMoveH ? movementX : 0;
                                    collisionAxes.Y = !canMoveV ? movementY : 0;
                                } else {
                                    collisionAxes.X = collidedH ? movementX : 0;
                                    collisionAxes.Y = collidedV ? movementY : 0;
                                }

                                if (collisionAxes.X == 0f && collisionAxes.Y == 0f) {
                                    body.CollidedWith(
                                        otherBody, 
                                        collisionAxes, 
                                        new CollisionInfo<Body>(otherBody, contactsH), 
                                        new CollisionInfo<Body>(otherBody, contactsV)
                                    );
                                } else {
                                    body.CollidedWith(
                                        otherBody, 
                                        collisionAxes, 
                                        collisionAxes.X == 0 ? null : new CollisionInfo<Body>(otherBody, contactsH), 
                                        collisionAxes.Y == 0 ? null : new CollisionInfo<Body>(otherBody, contactsV)
                                    );
                                }


                                //otherBody.CollidedWith(body, -collisionAxes);

#if DEBUG
                                body.Color = /*otherBody.Color =*/ Graphics.Color.Red;
#endif
                            }
                        }
                    }

                    // hack to force a collision verification even if not mean to move at all
                    if (singleCheck) {
                        break;
                    }

                    // separated axis movement
                    if (canMoveH && movementX != 0) {
                        distanceX -= movementX;
                        currentX += movementX;
                    }

                    if (canMoveV && movementY != 0) {
                        distanceY -= movementY;
                        currentY += movementY;
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
                body.PhysicsLateUpdate();
            }

#if DEBUG
            CollisionDetectionNarrowPhaseExecutionTime = Time.EndStopwatch();
#endif
        }

        private void AddCollider(Body collider, BitTag tags) {
            foreach (BitTag tag in tags) {
                List<Body> collidersByTag = _collidersByTag[tag];

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
                throw new System.ArgumentException($"Tag '{tag}' not found.", paramName);
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
