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
        private Dictionary<BitTag, HashSet<BitTag>> _collisionTagTable = new Dictionary<BitTag, HashSet<BitTag>>();

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

        public bool HasTag(BitTag tag) {
            return _collidersByTag.ContainsKey(tag);
        }

        public void RegisterTags<T>() {
            System.Type tagType = typeof(T);
            if (!tagType.IsEnum) {
                throw new System.ArgumentException("Tags Type must be a Enum.");
            }

            if (!tagType.IsDefined(typeof(System.FlagsAttribute), false)) {
                throw new System.ArgumentException("Tags Type must contains System.FlagsAttribute and all values declared as power of 2.");
            }

            foreach (System.Enum enumValue in System.Enum.GetValues(tagType)) {
                BitTag tag = enumValue;
                _collidersByTag.Add(tag, new List<Body>());
                _collisionTagTable.Add(tag, new HashSet<BitTag>());
            }
        }

        public void ClearTags() {
            _collidersByTag.Clear();
            _collisionTagTable.Clear();
        }

        public BitTag GetCollidableTags(BitTag tags) {
            BitTag collidableTags = BitTag.None;

            foreach (BitTag tag in tags) {
                foreach (BitTag collidableTag in _collisionTagTable[tag]) {
                    collidableTags += collidableTag;
                }
            }

            return collidableTags;
        }

        public void RegisterCollision(BitTag tagA, BitTag tagB) {
            ValidateTag(tagA, "tagA");
            ValidateTag(tagB, "tagB");
            Debug.Assert(tagA != BitTag.None && tagB != BitTag.None, $"Can't register a collision with tag None.");

            _collisionTagTable[tagA].Add(tagB);
            _collisionTagTable[tagB].Add(tagA);
        }

        public void RemoveCollision(BitTag tagA, BitTag tagB) {
            ValidateTag(tagA, "tagA");
            ValidateTag(tagB, "tagB");

#if DEBUG
            if (tagA == BitTag.None || tagB == BitTag.None) {
                return;
            }
#endif

            _collisionTagTable[tagA].Remove(tagB);
            _collisionTagTable[tagB].Remove(tagA);
        }

        public bool IsCollidable(BitTag tagA, BitTag tagB) {
            ValidateTag(tagA, "tagA");
            ValidateTag(tagB, "tagB");

#if DEBUG
            if (tagA == BitTag.None || tagB == BitTag.None) {
                return false;
            }
#endif

            return _collisionTagTable[tagA].Contains(tagB);
        }

        public void ClearCollisions() {
            foreach (HashSet<BitTag> collisionTags in _collisionTagTable.Values) {
                collisionTags.Clear();
            }
        }

        public void SetCollisions(Dictionary<BitTag, System.Array> collisions) {
            foreach (HashSet<BitTag> collidedTags in _collisionTagTable.Values) {
                collidedTags.Clear();
            }

            foreach (KeyValuePair<BitTag, System.Array> tagCollision in collisions) {
                foreach (BitTag otherTag in tagCollision.Value) {
                    RegisterCollision(tagCollision.Key, otherTag);
                }
            }
        }

        public void AddCollider(Body collider) {
            foreach (BitTag tag in collider.Tags) {
                AddCollider(collider, tag);
            }

            _colliders.Add(collider);
        }

        public void RemoveCollider(Body collider) {
            foreach (BitTag tag in collider.Tags) {
                RemoveCollider(collider, tag);
            }

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

        public int GetCollidersCount(BitTag tag) {
            ValidateTag(tag);

            int collidersCount = 0;
            foreach (BitTag t in tag) {
                collidersCount += _collidersByTag[t].Count;
            }

            return collidersCount;
        }

        public override string ToString() {
            string info = $"Physics:\n  Colliders: {_colliders.Count}\n  Collision Tag Table:\n";
            foreach (KeyValuePair<BitTag, HashSet<BitTag>> tagCollisionTable in _collisionTagTable) {
                info += $"    {tagCollisionTable.Key} => {string.Join(", ", tagCollisionTable.Value)}\n"; 
            }

            info += "  Colliders By Tag:\n";
            foreach (KeyValuePair<BitTag, List<Body>> tagColliders in _collidersByTag) {
                info += $"    {tagColliders.Key}: {tagColliders.Value.Count}\n";
            }

            return info;
        }

        #region Queries [Single Output]

        public bool QueryCollision(IShape shape, Vector2 position, BitTag tags, out Contact[] contacts) {
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

        public bool QueryCollision(IShape shape, Vector2 position, BitTag tags, out Body collidedCollider, out Contact[] contacts) {
            if (tags == BitTag.None) {
                collidedCollider = null;
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
                        collidedCollider = otherCollider;
                        return true;
                    }
                }
            }

            collidedCollider = null;
            contacts = null;
            return false;
        }

        public bool QueryCollision<T>(IShape shape, Vector2 position, BitTag tags, out T collidedEntity, out Contact[] contacts) where T : Entity {
            if (tags == BitTag.None) {
                collidedEntity = null;
                contacts = null;
                return false;
            }

            foreach (BitTag tag in tags) {
                ValidateTag(tag);

                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape) {
                        continue;
                    }

                    if (otherCollider.Entity is T && CheckCollision(shape, position, otherCollider, out contacts)) {
                        collidedEntity = otherCollider.Entity as T;
                        return true;
                    }
                }
            }

            collidedEntity = null;
            contacts = null;
            return false;
        }

        #endregion Queries [Single Output]

        #region Queries [Multiple Output]

        public bool QueryMultipleCollision(IShape shape, Vector2 position, BitTag tags, out List<(Body collider, Contact[] contact)> collidedColliders) {
            if (tags == BitTag.None) {
                collidedColliders = null;
                return false;
            }

            collidedColliders = new List<(Body, Contact[])>();
            foreach (BitTag tag in tags) {
                ValidateTag(tag);
                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape) {
                        continue;
                    }

                    if (CheckCollision(shape, position, otherCollider, out Contact[] contact)) {
                        collidedColliders.Add((otherCollider, contact));
                    }
                }
            }

            return collidedColliders.Count > 0;
        }

        public bool QueryMultipleCollision<T>(IShape shape, Vector2 position, BitTag tags, out List<(T entity, Contact[] contact)> collidedEntities) where T : Entity {
            if (tags == BitTag.None) {
                collidedEntities = null;
                return false;
            }

            collidedEntities = new List<(T, Contact[])>();
            foreach (BitTag tag in tags) {
                ValidateTag(tag);
                foreach (Body otherCollider in _collidersByTag[tag]) {
                    if (otherCollider.Shape == shape) {
                        continue;
                    }

                    if (otherCollider.Entity is T 
                      && CheckCollision(shape, position, otherCollider, out Contact[] contacts)) {
                        collidedEntities.Add((otherCollider.Entity as T, contacts));
                    }
                }
            }

            return collidedEntities.Count > 0;
        }

        #endregion Queries [Multiple Output]

        #region Raycast [Single Output]

        public bool Raycast(Vector2 position, Vector2 direction, BitTag tags, out Body collidedCollider, out Contact[] contacts, float maxDistance = float.PositiveInfinity) {
            collidedCollider = null;

            if (tags == BitTag.None) {
                contacts = null;
                return false;
            }

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
                contacts = new Contact[] {
                    rayContact.Value
                };

                return true;
            }

            contacts = null;
            return false;
        }

        public bool Raycast(Vector2 position, Vector2 direction, BitTag tags, out Contact[] contacts, float maxDistance = float.PositiveInfinity) {
            return Raycast(position, direction, tags, out Body collidedCollider, out contacts, maxDistance);
        }

        public bool Raycast<T>(Vector2 position, Vector2 direction, BitTag tags, out T collidedEntity, out Contact[] contacts, float maxDistance = float.PositiveInfinity) where T : Entity {
            if (Raycast(position, direction, tags, out Body collidedCollider, out contacts, maxDistance)
              && collidedCollider.Entity is T entity) {
                collidedEntity = entity;
                return true;
            }

            collidedEntity = null;
            return false;
        }

        #endregion Raycast [Single Output]

        #region Raycast [Multiple Output]

        public bool RaycastMultiple(Vector2 position, Vector2 direction, BitTag tags, out List<(Body body, Contact[] contacts)> collidedBodies, float maxDistance = float.PositiveInfinity) {
            collidedBodies = new List<(Body body, Contact[] contacts)>();

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

                    Contact[] contacts = new Contact[] {
                        contact.Value
                    };

                    collidedBodies.Add((otherCollider, contacts));
                }
            }

            return collidedBodies.Count > 0;
        }


        public bool RaycastMultiple<T>(Vector2 position, Vector2 direction, BitTag tags, out List<(T entity, Contact[] contacts)> collidedEntities, float maxDistance = float.PositiveInfinity) where T : Entity {
            collidedEntities = new List<(T entity, Contact[] contacts)>();

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

                    Contact[] contacts = new Contact[] {
                        contact.Value
                    };

                    collidedEntities.Add((entity, contacts));
                }
            }

            return collidedEntities.Count > 0;
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
                if (Math.EqualsEstimate(Math.DistanceSquared(diffX, diffY, diffX, diffY), 0.0)) {
                    body.MoveBufferX = body.MoveBufferY = 0.0;
                    body.PhysicsLateUpdate();
                    continue;
                }

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

                            // test for horizontal collision (if it's moving horizontally)
                            if (movementX != 0
                              && CheckCollision(body.Shape, moveHorizontalPos, otherBody, out Contact[] contactsH)
                              && System.Array.Exists(contactsH, c => c.PenetrationDepth > 0f)) {
                                collidedH = true;

                                if (isMovementCollidable) {
                                    canMoveH = false;
                                    distanceX = 0;
                                    directionY = Math.Sign(directionY);
                                    moveVerticalPos.X = currentX;
                                }
                            }

                            // test for vertical collision (if it's moving vertically)
                            if (movementY != 0
                              && CheckCollision(body.Shape, moveVerticalPos, otherBody, out Contact[] contactsV)
                              && System.Array.Exists(contactsV, c => c.PenetrationDepth > 0f)) {
                                collidedV = true;
                                
                                if (isMovementCollidable) {
                                    canMoveV = false;
                                    distanceY = 0;
                                    directionX = Math.Sign(directionX);
                                }
                            }

                            if (collidedH || collidedV) {
                                // stop moving
                                Vector2 collisionAxes = new Vector2(
                                    collidedH ? movementX : 0,
                                    collidedV ? movementY : 0
                                );

                                body.OnCollide(otherBody, collisionAxes);
                                otherBody.OnCollide(body, -collisionAxes);

#if DEBUG
                                body.Color = otherBody.Color = Graphics.Color.Red;
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

        private void AddCollider(Body collider, BitTag tag) {
            List<Body> collidersByTag = _collidersByTag[tag];
            if (collidersByTag.Contains(collider)) {
                return;
            }

            collidersByTag.Add(collider);
        }

        private void RemoveCollider(Body collider, BitTag tag) {
            ValidateTag(tag);
            _collidersByTag[tag].Remove(collider);
        }

        private bool CheckCollision(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            return _collisionFunctions[A.GetType()][B.GetType()](A, APos, B, BPos, out contacts);
        }

        private bool CheckCollision(IShape A, Vector2 APos, Body B, out Contact[] contacts) {
            return CheckCollision(A, APos, B.Shape, B.Position, out contacts);
        }

        private bool CheckCollision(Body A, Body B, out Contact[] contacts) {
            return CheckCollision(A.Shape, A.Position, B.Shape, B.Position, out contacts);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void ValidateTag(BitTag tag, string paramName = "tag") {
            if (!HasTag(tag)) {
                throw new System.ArgumentException($"Tag '{tag}' not found.", paramName);
            }
        }

        #endregion Private Methods

        #region Internal Methods

#if DEBUG
        internal void ClearTimers() {
            UpdatePositionExecutionTime = SolveConstraintsExecutionTime = CollisionDetectionBroadPhaseExecutionTime 
              = CollisionDetectionNarrowPhaseExecutionTime = 0;
        }
#endif

        #endregion InternalMethods
    }
}
