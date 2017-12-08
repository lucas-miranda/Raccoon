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
        private List<Body> _colliders = new List<Body>();
        private List<Movement> _movements = new List<Movement>();

        // collision checking
        private int _leftOverDeltaTime;
        private List<Body> _collisionCandidates = new List<Body>();
        private Dictionary<string, List<Body>> _candidatesByTag = new Dictionary<string, List<Body>>();
        private Dictionary<Body, List<Body>> _collisionQueries = new Dictionary<Body, List<Body>>();

        #endregion Private Members

        #region Constructors

        private Physics() {
            // collision functions dictionary
            System.Type circle = typeof(CircleShape);

            System.Type[] colliderTypes = {
                /*typeof(BoxCollider),
                typeof(GridCollider),
                typeof(CircleCollider),
                typeof(LineCollider),
                typeof(PolygonCollider),
                typeof(RichGridCollider)*/
                circle
            };

            foreach (System.Type type in colliderTypes) {
                _collisionFunctions.Add(type, new Dictionary<System.Type, CollisionCheckDelegate>());
            }

            // box vs others
            /*_collisionFunctions[typeof(BoxCollider)].Add(typeof(BoxCollider), CheckBoxBox);
            _collisionFunctions[typeof(BoxCollider)].Add(typeof(GridCollider), CheckBoxGrid);
            _collisionFunctions[typeof(BoxCollider)].Add(typeof(CircleCollider), CheckBoxCircle);
            _collisionFunctions[typeof(BoxCollider)].Add(typeof(LineCollider), CheckBoxLine);
            _collisionFunctions[typeof(BoxCollider)].Add(typeof(PolygonCollider), CheckBoxPolygon);
            _collisionFunctions[typeof(BoxCollider)].Add(typeof(RichGridCollider), CheckBoxRichGrid);

            // grid vs others
            _collisionFunctions[typeof(GridCollider)].Add(typeof(GridCollider), CheckGridGrid);
            _collisionFunctions[typeof(GridCollider)].Add(typeof(BoxCollider), CheckGridBox);
            _collisionFunctions[typeof(GridCollider)].Add(typeof(CircleCollider), CheckGridCircle);
            _collisionFunctions[typeof(GridCollider)].Add(typeof(LineCollider), CheckGridLine);
            _collisionFunctions[typeof(GridCollider)].Add(typeof(PolygonCollider), CheckGridPolygon);
            _collisionFunctions[typeof(GridCollider)].Add(typeof(RichGridCollider), CheckGridRichGrid);*/

            // circle vs others
            _collisionFunctions[circle].Add(circle, CheckCircleCircle);
            /*_collisionFunctions[typeof(CircleCollider)].Add(typeof(CircleCollider), CheckCircleCircle);
            _collisionFunctions[typeof(CircleCollider)].Add(typeof(BoxCollider), CheckCircleBox);
            _collisionFunctions[typeof(CircleCollider)].Add(typeof(GridCollider), CheckCircleGrid);
            _collisionFunctions[typeof(CircleCollider)].Add(typeof(LineCollider), CheckCircleLine);
            _collisionFunctions[typeof(CircleCollider)].Add(typeof(PolygonCollider), CheckCirclePolygon);
            _collisionFunctions[typeof(CircleCollider)].Add(typeof(RichGridCollider), CheckCircleRichGrid);

            // line vs others
            _collisionFunctions[typeof(LineCollider)].Add(typeof(LineCollider), CheckLineLine);
            _collisionFunctions[typeof(LineCollider)].Add(typeof(BoxCollider), CheckLineBox);
            _collisionFunctions[typeof(LineCollider)].Add(typeof(GridCollider), CheckLineGrid);
            _collisionFunctions[typeof(LineCollider)].Add(typeof(CircleCollider), CheckLineCircle);
            _collisionFunctions[typeof(LineCollider)].Add(typeof(PolygonCollider), CheckLinePolygon);
            _collisionFunctions[typeof(LineCollider)].Add(typeof(RichGridCollider), CheckLineRichGrid);

            // polygon vs others
            _collisionFunctions[typeof(PolygonCollider)].Add(typeof(PolygonCollider), CheckPolygonPolygon);
            _collisionFunctions[typeof(PolygonCollider)].Add(typeof(BoxCollider), CheckPolygonBox);
            _collisionFunctions[typeof(PolygonCollider)].Add(typeof(GridCollider), CheckPolygonGrid);
            _collisionFunctions[typeof(PolygonCollider)].Add(typeof(CircleCollider), CheckPolygonCircle);
            _collisionFunctions[typeof(PolygonCollider)].Add(typeof(LineCollider), CheckPolygonLine);
            _collisionFunctions[typeof(PolygonCollider)].Add(typeof(RichGridCollider), CheckPolygonRichGrid);

            // rich grid vs others
            _collisionFunctions[typeof(RichGridCollider)].Add(typeof(RichGridCollider), CheckRichGridRichGrid);
            _collisionFunctions[typeof(RichGridCollider)].Add(typeof(PolygonCollider), CheckRichGridPolygon);
            _collisionFunctions[typeof(RichGridCollider)].Add(typeof(BoxCollider), CheckRichGridBox);
            _collisionFunctions[typeof(RichGridCollider)].Add(typeof(GridCollider), CheckRichGridGrid);
            _collisionFunctions[typeof(RichGridCollider)].Add(typeof(CircleCollider), CheckRichGridCircle);
            _collisionFunctions[typeof(RichGridCollider)].Add(typeof(LineCollider), CheckRichGridLine);*/
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
#if DEBUG
                Time.StartStopwatch();
#endif

                // update position
                foreach (Movement movement in _movements) {
                    movement.OnMoveUpdate(FixedDeltaTimeSeconds);
                }

#if DEBUG
                UpdatePositionExecutionTime += Time.EndStopwatch();
                Time.StartStopwatch();
#endif

                // solve constraints
                /*for (int j = 0; j < ConstraintSolverAccuracy; j++) {
                    foreach (Body collider in _colliders) {
                        collider.SolveConstraints();
                    }
                }*/

#if DEBUG
                SolveConstraintsExecutionTime += Time.EndStopwatch();
                CollidersBroadPhaseCount = _colliders.Count;
                Time.StartStopwatch();
#endif

                // collision detection
                _collisionCandidates.Clear();
                _collisionQueries.Clear();
                foreach (List<Body> candidatesList in _candidatesByTag.Values) {
                    candidatesList.Clear();
                }

                // broad phase
                // choose candidate colliders
                foreach (Body collider in _colliders) {
                    _collisionCandidates.Add(collider);

                    /*foreach (string tag in collider.Tags) {
                        _candidatesByTag[tag].Add(collider);
                    }*/
                }

                // prepare queries
                // TODO: optimize this section
                foreach (Body collider in _collisionCandidates) {
                    _collisionQueries.Add(collider, new List<Body>());

                    foreach (Body colliderB in _collisionCandidates) {
                        if (colliderB == collider || _collisionQueries.ContainsKey(colliderB)) {
                            continue;
                        }

                        _collisionQueries[collider].Add(colliderB);
                    }

                    /*foreach (string tag in collider.Tags) {
                        foreach (string collisionTag in _collisionTagTable[tag]) {
                            foreach (Collider candidateCollider in _candidatesByTag[collisionTag]) {
                                if (_collisionQueries.ContainsKey(candidateCollider)) {
                                    continue;
                                }

                                _collisionQueries[collider].Add(candidateCollider);
                            }
                        }
                    }*/
                }

#if DEBUG
                CollisionDetectionBroadPhaseExecutionTime += Time.EndStopwatch();
                CollidersNarrowPhaseCount = _collisionCandidates.Count;
                Time.StartStopwatch();
#endif

                // narrow phase
                foreach (KeyValuePair<Body, List<Body>> query in _collisionQueries) {
                    foreach (Body colliderCandidate in query.Value) {
                        if (CheckCollision(query.Key, colliderCandidate, out Manifold manifold)) {
                            //Debug.WriteLine($"Collision with {query.Key.Entity.Name} and {colliderCandidate.Entity.Name}");
                            query.Key.OnCollide(colliderCandidate, manifold);
                            colliderCandidate.OnCollide(query.Key, manifold);
                        }
                    }
                }

#if DEBUG
                CollisionDetectionNarrowPhaseExecutionTime += Time.EndStopwatch();
#endif
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

            _colliders.Add(collider);
        }

        public void AddMovement(Movement movement) {
            _movements.Add(movement);
        }

        public void RemoveCollider(Body collider) {
            /*foreach (string tag in collider.Tags) {
                RemoveCollider(collider, tag);
            }*/

            _colliders.Remove(collider);
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
