using System.Collections.Generic;

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
        private System.Enum _noneTag;
        private Dictionary<System.Enum, HashSet<System.Enum>> _collisionTagTable = new Dictionary<System.Enum, HashSet<System.Enum>>();

        // colliders
        private List<Body> _colliders = new List<Body>();
        private Dictionary<System.Enum, List<Body>> _collidersByTag = new Dictionary<System.Enum, List<Body>>();

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
        public static System.Type TagType { get; private set; }

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

        public bool HasTag(System.Enum tag) {
            if (tag.GetType() != TagType) {
                throw new System.ArgumentException($"Type from '{tag}' is inconsistent with Tag Type {TagType.Name}.", "tag");
            }

            return _collidersByTag.ContainsKey(tag);
        }

        public void RegisterTags<T>() {
            System.Type type = typeof(T);
            if (!type.IsEnum) {
                throw new System.ArgumentException("Tags Type must be a Enum.");
            }

            if (!type.IsDefined(typeof(System.FlagsAttribute), false)) {
                throw new System.ArgumentException("Tags Type must contains System.FlagsAttribute and all values declared as power of 2.");
            }

            TagType = type;
            _noneTag = (System.Enum) System.Enum.ToObject(TagType, 0);
            foreach (System.Enum enumValue in System.Enum.GetValues(TagType)) {
                System.Enum tag = (System.Enum) System.Enum.ToObject(TagType, enumValue);
                _collidersByTag.Add(tag, new List<Body>());
                _collisionTagTable.Add(tag, new HashSet<System.Enum>());
            }
        }

        public void RegisterCollision(System.Enum tagA, System.Enum tagB) {
            ValidateTag(tagA, "tagA");
            ValidateTag(tagB, "tagB");

            if (tagA.Equals(_noneTag) || tagB.Equals(_noneTag)) {
                throw new System.ArgumentException($"Can't register a collision with special tag '{_noneTag}'.");
            }

            _collisionTagTable[tagA].Add(tagB);
            _collisionTagTable[tagB].Add(tagA);
        }

        public void RemoveCollision(System.Enum tagA, System.Enum tagB) {
            ValidateTag(tagA, "tagA");
            ValidateTag(tagB, "tagB");

            if (tagA.Equals(_noneTag) || tagB.Equals(_noneTag)) {
                return;
            }

            _collisionTagTable[tagA].Remove(tagB);
            _collisionTagTable[tagB].Remove(tagA);
        }

        public bool IsCollidable(System.Enum tagA, System.Enum tagB) {
            ValidateTag(tagA, "tagA");
            ValidateTag(tagB, "tagB");

            if (tagA.Equals(_noneTag) || tagB.Equals(_noneTag)) {
                return false;
            }

            return _collisionTagTable[tagA].Contains(tagB);
        }

        public void SetCollisions(Dictionary<System.Enum, System.Array> collisions) {
            foreach (HashSet<System.Enum> collidedTags in _collisionTagTable.Values) {
                collidedTags.Clear();
            }

            foreach (KeyValuePair<System.Enum, System.Array> tagCollision in collisions) {
                foreach (System.Enum otherTag in tagCollision.Value) {
                    RegisterCollision(tagCollision.Key, otherTag);
                }
            }
        }

        public void AddCollider(Body collider) {
            foreach (System.Enum tag in collider.Tags.GetFlagValues()) {
                AddCollider(collider, tag);
            }

            _colliders.Add(collider);
        }

        public void RemoveCollider(Body collider) {
            foreach (System.Enum tag in collider.Tags.GetFlagValues()) {
                RemoveCollider(collider, tag);
            }

            _colliders.Remove(collider);
        }

        public void UpdateColliderTagsEntry(Body collider, System.Enum oldTags = null) {
            if (oldTags == null) {
                foreach (KeyValuePair<System.Enum, List<Body>> tagColliders in _collidersByTag) {
                    tagColliders.Value.Remove(collider);
                }
            } else {
                List<System.Enum> oldTagsList = oldTags.GetFlagValues();
                foreach (System.Enum oldTag in oldTagsList) {
                    RemoveCollider(collider, oldTag);
                }
            }

            List<System.Enum> tags = collider.Tags.GetFlagValues();
            foreach (System.Enum tag in tags) {
                AddCollider(collider, tag);
            }
        }

        public int GetCollidersCount(System.Enum tag) {
            ValidateTag(tag);

            int collidersCount = 0;
            foreach (System.Enum t in tag.GetFlagValues()) {
                collidersCount += _collidersByTag[t].Count;
            }

            return collidersCount;
        }

        public override string ToString() {
            string info = $"Physics:\n  Colliders: {_colliders.Count}\n  Collision Tag Table:\n";
            foreach (KeyValuePair<System.Enum, HashSet<System.Enum>> tagCollisionTable in _collisionTagTable) {
                info += $"    {tagCollisionTable.Key} => {string.Join(", ", tagCollisionTable.Value)}\n"; 
            }

            info += "  Colliders By Tag:\n";
            foreach (KeyValuePair<System.Enum, List<Body>> tagColliders in _collidersByTag) {
                info += $"    {tagColliders.Key}: {tagColliders.Value.Count}\n";
            }

            return info;
        }

        #region Queries [Single Output]

        public bool QueryCollision(IShape shape, Vector2 position, System.Enum tags, out Contact[] contacts) {
            if (tags.Equals(_noneTag)) {
                contacts = null;
                return false;
            }

            foreach (System.Enum tag in tags.GetFlagValues()) {
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

        public bool QueryCollision(IShape shape, Vector2 position, System.Enum tags, out Body collidedCollider, out Contact[] contacts) {
            if (tags.Equals(_noneTag)) {
                collidedCollider = null;
                contacts = null;
                return false;
            }

            foreach (System.Enum tag in tags.GetFlagValues()) {
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

        public bool QueryCollision<T>(IShape shape, Vector2 position, System.Enum tags, out T collidedEntity, out Contact[] contacts) where T : Entity {
            if (tags.Equals(_noneTag)) {
                collidedEntity = null;
                contacts = null;
                return false;
            }

            foreach (System.Enum tag in tags.GetFlagValues()) {
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

        public bool QueryMultipleCollision(IShape shape, Vector2 position, System.Enum tags, out List<(Body collider, Contact[] contact)> collidedColliders) {
            if (tags.Equals(_noneTag)) {
                collidedColliders = null;
                return false;
            }

            collidedColliders = new List<(Body, Contact[])>();
            foreach (System.Enum tag in tags.GetFlagValues()) {
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

        public bool QueryMultipleCollision<T>(IShape shape, Vector2 position, System.Enum tags, out List<(T entity, Contact[] contact)> collidedEntities) where T : Entity {
            if (tags.Equals(_noneTag)) {
                collidedEntities = null;
                return false;
            }

            collidedEntities = new List<(T, Contact[])>();
            foreach (System.Enum tag in tags.GetFlagValues()) {
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

                if (body.Movement == null || !body.Enabled) {
                    continue;
                }

                body.Movement.FixedUpdate(dt);

                // swap bodies for fast collision check
                _narrowPhaseBodies[j] = _narrowPhaseBodies[0];
                _narrowPhaseBodies[0] = body;

                Vector2 nextPosition = body.PrepareMovement(dt);

                // prepare collidable tags
                long bodyCollidableTags = GetCollidableTagsAsNumber(body.Tags),
                     movementCollidableTags = System.Convert.ToInt64(body.Movement.CollisionTags);

                // initial body vars
                Vector2 currentPosition = body.Position;
                Vector2 distance = nextPosition - currentPosition;

                if (Math.EqualsEstimate(distance.LengthSquared(), 0f)) {
                    body.AfterMovement(dt, currentPosition, Vector2.Zero);
                    continue;
                }

                // I'm using the greatest distance axis to find a relation to move the body each loop by 1px at least
                float greatestAxis = System.Math.Max(System.Math.Abs(distance.X), System.Math.Abs(distance.Y));
                Vector2 direction = new Vector2(distance.X / greatestAxis, distance.Y / greatestAxis); 

                Vector2 movement = Vector2.Zero, 
                        movementBuffer = Vector2.Zero;

                bool canMoveH = true, canMoveV = true;
                bool isFirstCheck = true;

                do {
                    if (canMoveH && System.Math.Abs(distance.X) >= 1f) {
                        movementBuffer.X += direction.X;
                        // check movement buffer X for a valid movement
                        if (System.Math.Abs(movementBuffer.X) >= 1f) {
                            movement.X = System.Math.Sign(movementBuffer.X);
                            movementBuffer.X -= movement.X;
                        }
                    }

                    if (canMoveV && System.Math.Abs(distance.Y) >= 1f) {
                        movementBuffer.Y += direction.Y;
                        // check movement buffer Y for a valid movement
                        if (System.Math.Abs(movementBuffer.Y) >= 1f) {
                            movement.Y = System.Math.Sign(movementBuffer.Y);
                            movementBuffer.Y -= movement.Y;
                        }
                    }

                    // hack to force a collision verification without moving
                    if (isFirstCheck && distance.LengthSquared() < 1f) {
                        movement = new Vector2(System.Math.Sign(direction.X), System.Math.Sign(direction.Y));
                    }

                    // check collision with current movement
                    Vector2 moveHorizontalPos = new Vector2(currentPosition.X + movement.X, currentPosition.Y),
                            moveVerticalPos   = new Vector2(
                                                    canMoveH ? currentPosition.X + movement.X : currentPosition.X, 
                                                    currentPosition.Y + movement.Y
                                                ); // moveVerticalPos will do a diagonal move check, if canMoveH is true

                    if (!bodyCollidableTags.Equals(_noneTag) || !movementCollidableTags.Equals(_noneTag)) {
                        for (int k = 1; k < _narrowPhaseBodies.Count; k++) {
                            Body otherBody = _narrowPhaseBodies[k];
                            long otherBodyTags = System.Convert.ToInt64(otherBody.Tags);

                            bool isBodyCollidable     = (otherBodyTags & bodyCollidableTags)     != 0L,
                                 isMovementCollidable = (otherBodyTags & movementCollidableTags) != 0L;

                            // checks if otherBody tags contains at least one body or movement collidable tag
                            if (!isBodyCollidable && !isMovementCollidable) {
                                continue;
                            }

                            bool collidedH = false, collidedV = false;

                            // test for horizontal collision (if it's moving horizontally)
                            if (movement.X != 0f
                              && CheckCollision(body.Shape, moveHorizontalPos, otherBody, out Contact[] contactsH)
                              && contactsH[0].PenetrationDepth > 0f) {
                                collidedH = true;

                                if (isMovementCollidable) {
                                    canMoveH = false;
                                    distance.X = 0f;
                                    direction.Y = System.Math.Sign(direction.Y);
                                    moveVerticalPos = new Vector2(currentPosition.X, currentPosition.Y + movement.Y);
                                }
                            }

                            // test for vertical collision (if it's moving vertically)
                            if (movement.Y != 0f
                              && CheckCollision(body.Shape, moveVerticalPos, otherBody, out Contact[] contactsV)
                              && contactsV[0].PenetrationDepth > 0f) {
                                collidedV = true;

                                if (isMovementCollidable) {
                                    canMoveV = false;
                                    distance.Y = 0f;
                                    direction.X = System.Math.Sign(direction.X);
                                }
                            }

                            if (collidedH || collidedV) {
                                // stop moving
                                Vector2 collisionAxes = new Vector2(
                                    collidedH ? movement.X : 0f,
                                    collidedV ? movement.Y : 0f
                                );

                                body.OnCollide(otherBody, collisionAxes);
                                otherBody.OnCollide(body, -collisionAxes);

#if DEBUG
                                body.Color = otherBody.Color = Graphics.Color.Red;
#endif
                            }
                        }
                    }

                    // hack to force a collision verification without moving
                    if (isFirstCheck && distance.LengthSquared() < 1f) {
                        break;
                    }

                    // separated axis movement
                    if (canMoveH && movement.X != 0f) {
                        distance.X -= movement.X;
                        currentPosition.X += movement.X;
                    }

                    if (canMoveV && movement.Y != 0f) {
                        distance.Y -= movement.Y;
                        currentPosition.Y += movement.Y;
                    }

                    movement = Vector2.Zero;
                    isFirstCheck = false;
                } while (
                     (canMoveH && System.Math.Abs(distance.X) >= 1f) 
                  || (canMoveV && System.Math.Abs(distance.Y) >= 1f)
                );

                // checks for movement buffer to return to the body
                Vector2 finalMovementBuffer = Vector2.Zero;
                if (canMoveH && System.Math.Abs(distance.X) > 0f) {
                    finalMovementBuffer.X = distance.X + movementBuffer.X;
                }

                if (canMoveV && System.Math.Abs(distance.Y) > 0f) {
                    finalMovementBuffer.Y = distance.Y + movementBuffer.Y;
                }

                body.AfterMovement(dt, currentPosition, finalMovementBuffer);
            }

#if DEBUG
            CollisionDetectionNarrowPhaseExecutionTime = Time.EndStopwatch();
#endif
        }

        private void AddCollider(Body collider, System.Enum tag) {
            List<Body> collidersByTag = _collidersByTag[tag];
            if (collidersByTag.Contains(collider)) {
                return;
            }

            collidersByTag.Add(collider);
        }

        private void RemoveCollider(Body collider, System.Enum tag) {
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

        private void ValidateTag(System.Enum tag, string paramName = "tag") {
            if (!HasTag(tag)) {
                throw new System.ArgumentException($"Tag '{tag}' not found on Tag Type {TagType.Name}.", paramName);
            }
        }

        private bool IsSingleTag(System.Enum tags) {
            return Math.IsPowerOfTwo(System.Convert.ToInt64(tags));
        }

        #endregion Private Methods

        #region Internal Methods

        internal long GetCollidableTagsAsNumber(System.Enum tags) {
            long tagsValue = 0;
            foreach (System.Enum tag in tags.GetFlagValues()) {
                foreach (System.Enum collidableTag in _collisionTagTable[tag]) {
                    tagsValue |= System.Convert.ToInt64(collidableTag);
                }
            }

            return tagsValue;
        }

        internal System.Enum GetCollidableTags(System.Enum tags) {
            return (System.Enum) System.Enum.ToObject(TagType, GetCollidableTagsAsNumber(tags));
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
