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

        private delegate bool CollisionCheckDelegate(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold);

        private static readonly System.Lazy<Physics> _lazy = new System.Lazy<Physics>(() => new Physics());

        private Dictionary<System.Type, Dictionary<System.Type, CollisionCheckDelegate>> _collisionFunctions = new Dictionary<System.Type, Dictionary<System.Type, CollisionCheckDelegate>>();

        // colliders register
        private Dictionary<string, HashSet<string>> _collisionTagTable = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, List<Body>> _collidersByTag = new Dictionary<string, List<Body>>();
        private List<Body> _bodies = new List<Body>();
        private List<Movement> _movements = new List<Movement>(); // REMOVE

        // collision checking
        private int _leftOverDeltaTime;
        private List<Manifold> _contactManifolds = new List<Manifold>();

        private List<Body> _collisionCandidates = new List<Body>();
        private Dictionary<string, List<Body>> _candidatesByTag = new Dictionary<string, List<Body>>();
        private Dictionary<Body, List<Body>> _collisionQueries = new Dictionary<Body, List<Body>>();

        #endregion Private Members

        #region Constructors

        private Physics() {
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
            // TODO: update using a float delta (increases precision)
            int timesteps = (int) System.Math.Floor((delta + _leftOverDeltaTime) / (float) FixedDeltaTime);
            timesteps = Math.Min(5, timesteps); // prevents freezing
            _leftOverDeltaTime = Math.Max(0, delta - (timesteps * FixedDeltaTime));

            for (int i = 0; i < timesteps; i++) {
                Step(FixedDeltaTimeSeconds);
            }
        }

        public void RegisterTag(string tagName) {
            if (string.IsNullOrWhiteSpace(tagName)) {
                throw new System.ArgumentException("Invalid tag name.", "tagName");
            }

            if (HasTag(tagName)) {
                return;
            }

            _collidersByTag.Add(tagName, new List<Body>());
            _collisionTagTable.Add(tagName, new HashSet<string>());
            _candidatesByTag.Add(tagName, new List<Body>());
        }

        public void RegisterTag(System.Enum tag) {
            RegisterTag(tag.ToString());
        }

        public bool HasTag(string tagName) {
            return !string.IsNullOrWhiteSpace(tagName) && _collidersByTag.ContainsKey(tagName);
        }

        public bool HasTag(System.Enum tag) {
            return HasTag(tag.ToString());
        }

        public void RegisterTags(params string[] tags) {
            foreach (string tagName in tags) {
                RegisterTag(tagName);
            }
        }

        public void RegisterTags(params System.Enum[] tags) {
            foreach (System.Enum tag in tags) {
                RegisterTag(tag);
            }
        }

        public void RegisterTags<T>() {
            System.Type type = typeof(T);
            if (!type.IsEnum) {
                throw new System.ArgumentException("Type must be a Enum.");
            }

            RegisterTags(System.Enum.GetNames(type));
        }

        public void RegisterCollision(string tagNameA, string tagNameB) {
            if (!HasTag(tagNameA)) {
                throw new System.ArgumentException($"Tag '{tagNameA}' not found or it's invalid.", "tagA");
            }

            if (!HasTag(tagNameB)) {
                throw new System.ArgumentException($"Tag '{tagNameB}' not found or it's invalid.", "tagB");
            }

            _collisionTagTable[tagNameA].Add(tagNameB);
            _collisionTagTable[tagNameB].Add(tagNameA);
        }

        public void RegisterCollision(System.Enum tagA, System.Enum tagB) {
            RegisterCollision(tagA.ToString(), tagB.ToString());
        }

        public void RemoveCollision(string tagNameA, string tagNameB) {
            if (!HasTag(tagNameA)) {
                throw new System.ArgumentException($"Tag '{tagNameA}' not found or it's invalid.", "tagNameA");
            }

            if (!HasTag(tagNameB)) {
                throw new System.ArgumentException($"Tag '{tagNameB}' not found or it's invalid.", "tagNameB");
            }

            _collisionTagTable[tagNameA].Remove(tagNameB);
            _collisionTagTable[tagNameB].Remove(tagNameA);
        }

        public void RemoveCollision(System.Enum tagA, System.Enum tagB) {
            RemoveCollision(tagA.ToString(), tagB.ToString());
        }

        public bool IsCollidable(string tagNameA, string tagNameB) {
            if (!HasTag(tagNameA)) {
                throw new System.ArgumentException($"Tag '{tagNameA}' not found or it's invalid.", "tagNameA");
            }

            if (!HasTag(tagNameB)) {
                throw new System.ArgumentException($"Tag '{tagNameB}' not found or it's invalid.", "tagNameB");
            }

            return _collisionTagTable[tagNameA].Contains(tagNameB) && _collisionTagTable[tagNameB].Contains(tagNameA);
        }

        public bool IsCollidable(System.Enum tagA, System.Enum tagB) {
            return IsCollidable(tagA.ToString(), tagB.ToString());
        }

        public void AddCollider(Body collider) {
            /*foreach (string tag in collider.Tags) {
                AddCollider(collider, tag);
            }*/

            _bodies.Add(collider);
        }

        public void AddMovement(Movement movement) {
            _movements.Add(movement);
        }

        public void RemoveCollider(Body collider) {
            /*foreach (string tag in collider.Tags) {
                RemoveCollider(collider, tag);
            }*/

            _bodies.Remove(collider);
        }

        public void RemoveMovement(Movement movement) {
            _movements.Remove(movement);
        }

        public int GetCollidersCount(string tagName) {
            if (!HasTag(tagName)) {
                return 0;
            }

            return _collidersByTag[tagName].Count;
        }

        public int GetCollidersCount(System.Enum tag) {
            return GetCollidersCount(tag.ToString());
        }

        /*

        #region Collides [Single Tag] [Single Output]

        public bool Collides(Vector2 position, Collider collider, string tag) {
            if (!HasTag(tag)) {
                return false;
            }

            foreach (Collider c in _collidersByTag[tag]) {
                if (c != collider && CheckCollision(collider, position - collider.Origin, c)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(Vector2 position, Collider collider, string tag, out Collider collidedCollider) {
            collidedCollider = null;
            if (!HasTag(tag)) {
                return false;
            }

            foreach (Collider c in _collidersByTag[tag]) {
                if (c != collider && CheckCollision(collider, position - collider.Origin, c)) {
                    collidedCollider = c;
                    return true;
                }
            }

            return false;
        }

        public bool Collides<T>(Vector2 position, Collider collider, string tag, out T collidedEntity) where T : Entity {
            collidedEntity = null;
            if (!HasTag(tag)) {
                return false;
            }

            foreach (Collider c in _collidersByTag[tag]) {
                if (c != collider && c.Entity is T && CheckCollision(collider, position - collider.Origin, c)) {
                    collidedEntity = c.Entity as T;
                    return true;
                }
            }

            return false;
        }

        public bool Collides(Collider collider, string tag) {
            return Collides(collider.Entity.Position, collider, tag);
        }

        public bool Collides(Collider collider, string tag, out Collider collidedCollider) {
            return Collides(collider.Entity.Position, collider, tag, out collidedCollider);
        }

        public bool Collides<T>(Collider collider, string tag, out T collidedEntity) where T : Entity {
            return Collides(collider.Entity.Position, collider, tag, out collidedEntity);
        }

        #endregion Collides [Single Tag] [Single Output]

        #region Collides [Single Tag] [Multiple Output]

        public bool Collides(Vector2 position, Collider collider, string tag, out List<Collider> collidedColliders) {
            collidedColliders = new List<Collider>();
            if (!HasTag(tag)) {
                return false;
            }

            foreach (Collider c in _collidersByTag[tag]) {
                if (c != collider && CheckCollision(collider, position - collider.Origin, c)) {
                    collidedColliders.Add(c);
                }
            }

            return collidedColliders.Count > 0;
        }

        public bool Collides<T>(Vector2 position, Collider collider, string tag, out List<T> collidedEntities) where T : Entity {
            collidedEntities = new List<T>();
            if (!HasTag(tag)) {
                return false;
            }

            foreach (Collider c in _collidersByTag[tag]) {
                if (c != collider && c.Entity is T && CheckCollision(collider, position - collider.Origin, c)) {
                    collidedEntities.Add(c.Entity as T);
                }
            }

            return collidedEntities.Count > 0;
        }

        public bool Collides(Collider collider, string tag, out List<Collider> collidedColliders) {
            return Collides(collider.Entity.Position, collider, tag, out collidedColliders);
        }

        public bool Collides<T>(Collider collider, string tag, out List<T> collidedEntities) where T : Entity {
            return Collides(collider.Entity.Position, collider, tag, out collidedEntities);
        }

        #endregion Collides [Single Tag] [Multiple Output]

        #region Collides [Multiple Tag] [Single Output]

        public bool Collides(Vector2 position, Collider collider, IEnumerable<string> tags) {
            foreach (string tag in tags) {
                if (Collides(position, collider, tag)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(Vector2 position, Collider collider, IEnumerable<string> tags, out Collider collidedCollider) {
            foreach (string tag in tags) {
                if (Collides(position, collider, tag, out collidedCollider)) {
                    return true;
                }
            }

            collidedCollider = null;
            return false;
        }

        public bool Collides<T>(Vector2 position, Collider collider, IEnumerable<string> tags, out T collidedEntity) where T : Entity {
            foreach (string tag in tags) {
                if (Collides(position, collider, tag, out collidedEntity)) {
                    return true;
                }
            }

            collidedEntity = null;
            return false;
        }

        public bool Collides(Collider collider, IEnumerable<string> tags) {
            return Collides(collider.Position, collider, tags);
        }

        public bool Collides(Collider collider, IEnumerable<string> tags, out Collider collidedCollider) {
            return Collides(collider.Position, collider, tags, out collidedCollider);
        }

        public bool Collides<T>(Collider collider, IEnumerable<string> tags, out T collidedEntity) where T : Entity {
            return Collides(collider.Position, collider, tags, out collidedEntity);
        }

        #endregion Collides [Multiple Tag] [Single Output]

        #region Collides [Multiple Tag] [Multiple Output]

        public bool Collides(Vector2 position, Collider collider, IEnumerable<string> tags, out List<Collider> collidedColliders) {
            collidedColliders = new List<Collider>();
            foreach (string tag in tags) {
                List<Collider> collidedTagColliders = new List<Collider>();
                if (Collides(position, collider, tag, out collidedTagColliders)) {
                    collidedColliders.AddRange(collidedTagColliders);
                }
            }

            return collidedColliders.Count > 0;
        }

        public bool Collides<T>(Vector2 position, Collider collider, IEnumerable<string> tags, out List<T> collidedEntities) where T : Entity {
            collidedEntities = new List<T>();
            foreach (string tag in tags) {
                List<T> collidedTagEntities = new List<T>();
                if (Collides(position, collider, tag, out collidedTagEntities)) {
                    collidedEntities.AddRange(collidedTagEntities);
                }
            }

            return collidedEntities.Count > 0;
        }

        public bool Collides(Collider collider, IEnumerable<string> tags, out List<Collider> collidedColliders) {
            return Collides(collider.Position, collider, tags, out collidedColliders);
        }

        public bool Collides<T>(Collider collider, IEnumerable<string> tags, out List<T> collidedEntities) where T : Entity {
            return Collides(collider.Position, collider, tags, out collidedEntities);
        }

        #endregion Collides [Multiple Tag] [Multiple Output]

        */

        #endregion Public Methods

        #region Private Methods

        private void Step(float dt) {
            // solve constraints
            for (int i = 0; i < ConstraintSolverAccuracy; i++) {
                foreach (Body body in _bodies) {
                    body.SolveConstraints();
                }
            }

            // generate contacts
            _contactManifolds.Clear();
            for (int i = 0; i < _bodies.Count; i++) {
                Body A = _bodies[i];
                for (int j = i + 1; j < _bodies.Count; j++) {
                    Body B = _bodies[j];
                    
                    // if both mass are infinite, doesn't need to solve collision
                    if (A.InverseMass == 0 && B.InverseMass == 0) {
                        continue;
                    }

                    if (CheckCollision(A, B, out Manifold manifold)) {
                        _contactManifolds.Add(manifold);
                        A.OnCollide(B, manifold);
                        B.OnCollide(A, manifold);
#if DEBUG
                        A.Color = B.Color = Graphics.Color.Red;
                        continue;
#endif
                    }

#if DEBUG
                    A.Color = B.Color = Graphics.Color.White;
#endif
                }
            }

            // integrate position
            foreach (Body body in _bodies) {
                body.Integrate(dt);
            }

            for (int i = 0; i < ConstraintSolverAccuracy; i++) {
                foreach (Manifold manifold in _contactManifolds) {
                    manifold.ImpulseResolution();
                }
            }

            foreach (Manifold manifold in _contactManifolds) {
                manifold.PositionCorrection();
            }
        }

        private void AddCollider(Body collider, string tagName) {
            RegisterTag(tagName);
            if (_collidersByTag[tagName].Contains(collider)) {
                return;
            }

            _collidersByTag[tagName].Add(collider);
        }

        private void RemoveCollider(Body collider, string tagName) {
            if (string.IsNullOrWhiteSpace(tagName)) {
                throw new System.ArgumentException("Tag can't be empty.", "tagName");
            }

            if (!HasTag(tagName)) {
                throw new System.ArgumentException($"Tag '{tagName}' not found. Register it first.", "tagName");
            }

            _collidersByTag[tagName].Remove(collider);
        }

        private bool CheckCollision(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            return _collisionFunctions[A.Shape.GetType()][B.Shape.GetType()](A, APos, B, BPos, out manifold);
        }

        private bool CheckCollision(Body A, Vector2 APos, Body B, out Manifold manifold) {
            return CheckCollision(A, APos, B, B.Position, out manifold);
        }

        private bool CheckCollision(Body A, Body B, out Manifold manifold) {
            return CheckCollision(A, A.Position, B, B.Position, out manifold);
        }

        #endregion Private Methods

        #region Internal Methods

#if DEBUG
        internal void ClearTimers() {
            UpdatePositionExecutionTime = SolveConstraintsExecutionTime = CollisionDetectionBroadPhaseExecutionTime = CollisionDetectionNarrowPhaseExecutionTime = 0;
        }
#endif

        #endregion InternalMethods
    }
}
