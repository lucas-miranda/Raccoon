using System;
using System.Collections.Generic;

using Raccoon.Components;
using Raccoon.Util;

namespace Raccoon {
    public class Physics {
        #region Private Static Readonly Members

        private static readonly Lazy<Physics> _lazy = new Lazy<Physics>(() => new Physics());

        #endregion Private Static Readonly Members

        #region Private Members

        private Dictionary<string, List<Collider>> _colliders = new Dictionary<string, List<Collider>>();
        private Dictionary<Type, Dictionary<Type, Func<Collider, Vector2, Collider, Vector2, bool>>> _collisionFunctions = new Dictionary<Type, Dictionary<Type, Func<Collider, Vector2, Collider, Vector2, bool>>>();

        #endregion Private Members

        #region Constructors

        private Physics() {
            // collision functions dictionary
            Type[] colliderTypes = {
                typeof(BoxCollider),
                typeof(GridCollider),
                typeof(CircleCollider),
                typeof(LineCollider),
                typeof(PolygonCollider),
                typeof(RichGridCollider)
            };

            foreach (Type type in colliderTypes) {
                _collisionFunctions.Add(type, new Dictionary<Type, Func<Collider, Vector2, Collider, Vector2, bool>>());
            }

            // box vs others
            _collisionFunctions[typeof(BoxCollider)].Add(typeof(BoxCollider), CheckBoxBox);
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
            _collisionFunctions[typeof(GridCollider)].Add(typeof(RichGridCollider), CheckGridRichGrid);

            // circle vs others
            _collisionFunctions[typeof(CircleCollider)].Add(typeof(CircleCollider), CheckCircleCircle);
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
            _collisionFunctions[typeof(RichGridCollider)].Add(typeof(LineCollider), CheckRichGridLine);
        }

        #endregion Constructors

        #region Public Static Properties

        public static Physics Instance { get { return _lazy.Value; } }
        public static int MinUpdateInterval { get; set; } = (int) (1 / 60f * 1000);

        #endregion Public Static Properties

        #region Public Methods

        public void RegisterTag(string tag) {
            if (HasTag(tag)) {
                return;
            }

            _colliders.Add(tag, new List<Collider>());
        }

        public void RegisterTag(Enum tag) {
            RegisterTag(tag.ToString());
        }

        public bool HasTag(string tag) {
            return _colliders.ContainsKey(tag);
        }

        public void HasTag(Enum tag) {
            HasTag(tag.ToString());
        }

        public void AddCollider(Collider collider, string tag) {
            RegisterTag(tag);
            if (_colliders[tag].Contains(collider)) {
                return;
            }

            _colliders[tag].Add(collider);
        }

        public void AddCollider(Collider collider, Enum tag) {
            AddCollider(collider, tag.ToString());
        }

        public void AddCollider(Collider collider, IEnumerable<string> tags) {
            foreach (string tag in tags) {
                AddCollider(collider, tag);
            }
        }

        public void AddCollider(Collider collider, IEnumerable<Enum> tags) {
            foreach (Enum tag in tags) {
                AddCollider(collider, tag);
            }
        }

        public void RemoveCollider(Collider collider, string tag = "") {
            if (tag == "") {
                foreach (List<Collider> colls in _colliders.Values) {
                    colls.Remove(collider);
                }
            } else if (HasTag(tag)) {
                _colliders[tag].Remove(collider);
            }
        }

        public void RemoveCollider(Collider collider, Enum tag) {
            RemoveCollider(collider, tag.ToString());
        }

        public void RemoveCollider(Collider collider, ICollection<string> tags) {
            if (tags.Count == 0) {
                RemoveCollider(collider);
                return;
            }

            foreach (string tag in tags) {
                if (!HasTag(tag)) {
                    continue;
                }

                _colliders[tag].Remove(collider);
            }
        }

        public void RemoveCollider(Collider collider, ICollection<Enum> tags) {
            if (tags.Count == 0) {
                RemoveCollider(collider);
                return;
            }

            foreach (Enum tag in tags) {
                if (!HasTag(tag.ToString())) {
                    continue;
                }

                _colliders[tag.ToString()].Remove(collider);
            }
        }

        public int GetCollidersCount(string tag) {
            if (!HasTag(tag)) {
                return 0;
            }

            return _colliders[tag].Count;
        }

        public int GetCollidersCount(Enum tag) {
            return GetCollidersCount(tag.ToString());
        }

        #region Collides [Single Tag] [Single Output]

        public bool Collides(Vector2 position, Collider collider, string tag) {
            if (!HasTag(tag)) {
                return false;
            }

            foreach (Collider c in _colliders[tag]) {
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

            foreach (Collider c in _colliders[tag]) {
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

            foreach (Collider c in _colliders[tag]) {
                if (c != collider && CheckCollision(collider, position - collider.Origin, c)) {
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

            foreach (Collider c in _colliders[tag]) {
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

            foreach (Collider c in _colliders[tag]) {
                if (c != collider && CheckCollision(collider, position - collider.Origin, c)) {
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

        #endregion Public Methods

        #region Private Methods

        private bool CheckCollision(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return _collisionFunctions[colliderA.GetType()][colliderB.GetType()](colliderA, colliderAPos, colliderB, colliderBPos);
        }

        private bool CheckCollision(Collider colliderA, Vector2 colliderAPos, Collider colliderB) {
            return CheckCollision(colliderA, colliderAPos, colliderB, colliderB.Position);
        }

        private bool CheckCollision(Collider colliderA, Collider colliderB) {
            return CheckCollision(colliderA, colliderA.Position, colliderB, colliderB.Position);
        }

        #region Box

        #region Box vs Box

        private bool CheckBoxBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxAColl = colliderA as BoxCollider, boxBColl = colliderB as BoxCollider;
            if (boxAColl.Rotation != 0 || boxBColl.Rotation != 0) { // SAT
                // box collider polygon
                Polygon boxACollPolygon = boxAColl.Polygon;
                boxACollPolygon.Translate(colliderAPos);

                // other collider polygon
                Polygon boxBCollPolygon = boxBColl.Polygon;
                boxBCollPolygon.Translate(colliderBPos);

                Vector2[] axes = new Vector2[] {
                                (boxACollPolygon[0] - boxACollPolygon[1]).Perpendicular(),
                                (boxACollPolygon[1] - boxACollPolygon[2]).Perpendicular(),
                                (boxBCollPolygon[0] - boxBCollPolygon[1]).Perpendicular(),
                                (boxBCollPolygon[1] - boxBCollPolygon[2]).Perpendicular()
                            };

                return CheckPolygonsIntersection(boxACollPolygon, boxBCollPolygon, axes);
            }

            return new Rectangle(colliderAPos, boxAColl.Size) & new Rectangle(colliderBPos, boxBColl.Size); // regular AABB
        }

        #endregion Box vs Box

        #region Box vs Grid

        private bool CheckBoxGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            GridCollider gridColl = colliderB as GridCollider;

            int startColumn, startRow, endColumn, endRow, row, column;
            if (boxColl.Rotation != 0) {
                // box collider polygon
                Polygon boxCollPolygon = boxColl.Polygon;
                boxCollPolygon.Translate(colliderAPos);

                Vector2[] axes = new Vector2[] {
                            new Vector2(1, 0),
                            new Vector2(0, 1),
                            (boxCollPolygon[0] - boxCollPolygon[1]).Perpendicular(),
                            (boxCollPolygon[1] - boxCollPolygon[2]).Perpendicular()
                        };

                float top = boxCollPolygon[0].Y, right = boxCollPolygon[0].X, bottom = boxCollPolygon[0].Y, left = boxCollPolygon[0].X;
                foreach (Vector2 vertex in boxCollPolygon) {
                    if (vertex.Y < top)
                        top = vertex.Y;
                    if (vertex.X > right)
                        right = vertex.X;
                    if (vertex.Y > bottom)
                        bottom = vertex.Y;
                    if (vertex.X < left)
                        left = vertex.X;
                }

                startColumn = (int) (left - colliderBPos.X) / (int) gridColl.TileSize.Width;
                startRow = (int) (top - colliderBPos.Y) / (int) gridColl.TileSize.Height;
                endColumn = (int) (right - colliderBPos.X) / (int) gridColl.TileSize.Width;
                endRow = (int) (bottom - colliderBPos.Y) / (int) gridColl.TileSize.Height;

                for (row = startRow; row <= endRow; row++) {
                    for (column = startColumn; column <= endColumn; column++) {
                        if (!gridColl.IsCollidable(column, row)) {
                            continue;
                        }

                        if (CheckPolygonsIntersection(boxCollPolygon,
                            new Polygon(new Vector2(colliderBPos.X + column * gridColl.TileSize.Width, colliderBPos.Y + row * gridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + (column + 1) * gridColl.TileSize.Width, colliderBPos.Y + row * gridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + (column + 1) * gridColl.TileSize.Width, colliderBPos.Y + (row + 1) * gridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + column * gridColl.TileSize.Width, colliderBPos.Y + (row + 1) * gridColl.TileSize.Height)
                            ), axes)) {
                            return true;
                        }
                    }
                }

                return false;
            }

            Rectangle boxRect = new Rectangle(colliderAPos, boxColl.Size);
            if (!(boxRect & new Rectangle(colliderBPos, gridColl.Size))) { // out of grid
                return false;
            }

            startColumn = (int) (boxRect.Left - colliderBPos.X) / (int) gridColl.TileSize.Width;
            startRow = (int) (boxRect.Top - colliderBPos.Y) / (int) gridColl.TileSize.Height;
            endColumn = (int) (boxRect.Right - colliderBPos.X - 1) / (int) gridColl.TileSize.Width;
            endRow = (int) (boxRect.Bottom - colliderBPos.Y - 1) / (int) gridColl.TileSize.Height;
            for (row = startRow; row <= endRow; row++) {
                for (column = startColumn; column <= endColumn; column++) {
                    if (gridColl.IsCollidable(column, row)) {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Box vs Grid

        #region Box vs Circle

        private bool CheckBoxCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            CircleCollider circleColl = colliderB as CircleCollider;

            if (boxColl.Rect & circleColl.Center) {
                return true;
            }

            float radiusSquared = circleColl.Radius * circleColl.Radius;
            return Util.Math.DistanceSquared(new Line(new Vector2(colliderAPos.X - 1, colliderAPos.Y - 1), new Vector2(colliderAPos.X + boxColl.Width, colliderAPos.Y - 1)), circleColl.Center) < radiusSquared
                || Util.Math.DistanceSquared(new Line(new Vector2(colliderAPos.X + boxColl.Width, colliderAPos.Y - 1), new Vector2(colliderAPos.X + boxColl.Width, colliderAPos.Y + boxColl.Height)), circleColl.Center) < radiusSquared
                || Util.Math.DistanceSquared(new Line(new Vector2(colliderAPos.X + boxColl.Width, colliderAPos.Y + boxColl.Height), new Vector2(colliderAPos.X - 1, colliderAPos.Y + boxColl.Height)), circleColl.Center) < radiusSquared
                || Util.Math.DistanceSquared(new Line(new Vector2(colliderAPos.X - 1, colliderAPos.Y + boxColl.Height), new Vector2(colliderAPos.X, colliderAPos.Y - 1)), circleColl.Center) < radiusSquared;
        }

        #endregion Box vs Circle

        #region Box vs Line

        private bool CheckBoxLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            LineCollider lineColl = colliderB as LineCollider;

            // box collider polygon
            Polygon boxPolygon = boxColl.Polygon;
            boxPolygon.Translate(colliderAPos);

            // line collider polygon
            Polygon linePolygon = new Polygon(lineColl.From, lineColl.To);
            linePolygon.Translate(colliderBPos);

            Vector2[] axes = new Vector2[] { 
                // box relevant axes
                (boxPolygon[0] - boxPolygon[1]).Perpendicular(),
                (boxPolygon[1] - boxPolygon[2]).Perpendicular(),

                // line axis
                (linePolygon[0] - linePolygon[1]).Perpendicular()
            };

            return CheckPolygonsIntersection(boxPolygon, linePolygon, axes);
        }

        #endregion Box vs Line

        #region Box vs Polygon

        private bool CheckBoxPolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            PolygonCollider polygonColl = colliderB as PolygonCollider;

            // box collider polygon
            Polygon boxPolygon = boxColl.Polygon;
            boxPolygon.Translate(colliderAPos);

            // polygon collider polygon
            Polygon polygon = polygonColl.Polygon.Clone();
            polygon.Translate(colliderBPos);

            List<Vector2> axes = new List<Vector2>();

            // box relevant axes
            axes.Add((boxPolygon[0] - boxPolygon[1]).Perpendicular());
            axes.Add((boxPolygon[1] - boxPolygon[2]).Perpendicular());

            // polygon axes
            for (int i = 0; i < polygon.VertexCount; i++) {
                axes.Add((polygon[i] - polygon[(i + 1) % polygon.VertexCount]).Perpendicular());
            }

            return CheckPolygonsIntersection(boxPolygon, polygon, axes);
        }

        #endregion Box vs Polygon

        #region Box vs RichGrid

        private bool CheckBoxRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            RichGridCollider richGridColl = colliderB as RichGridCollider;

            int startColumn, startRow, endColumn, endRow, row, column;
            /*if (boxColl.Rotation != 0) {
                // box collider polygon
                Polygon boxCollPolygon = boxColl.Polygon;
                boxCollPolygon.Translate(colliderAPos);

                Vector2[] axes = new Vector2[] {
                            new Vector2(1, 0),
                            new Vector2(0, 1),
                            (boxCollPolygon[0] - boxCollPolygon[1]).Perpendicular(),
                            (boxCollPolygon[1] - boxCollPolygon[2]).Perpendicular()
                        };

                float top = boxCollPolygon[0].Y, right = boxCollPolygon[0].X, bottom = boxCollPolygon[0].Y, left = boxCollPolygon[0].X;
                foreach (Vector2 vertex in boxCollPolygon) {
                    if (vertex.Y < top)
                        top = vertex.Y;
                    if (vertex.X > right)
                        right = vertex.X;
                    if (vertex.Y > bottom)
                        bottom = vertex.Y;
                    if (vertex.X < left)
                        left = vertex.X;
                }

                startColumn = (int) (left - colliderBPos.X) / (int) richGridColl.TileSize.Width;
                startRow = (int) (top - colliderBPos.Y) / (int) richGridColl.TileSize.Height;
                endColumn = (int) (right - colliderBPos.X) / (int) richGridColl.TileSize.Width;
                endRow = (int) (bottom - colliderBPos.Y) / (int) richGridColl.TileSize.Height;

                for (row = startRow; row <= endRow; row++) {
                    for (column = startColumn; column <= endColumn; column++) {
                        if (!richGridColl.IsCollidable(column, row)) {
                            continue;
                        }

                        if (CheckPolygonsIntersection(boxCollPolygon,
                            new Polygon(new Vector2(colliderBPos.X + column * richGridColl.TileSize.Width, colliderBPos.Y + row * richGridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + (column + 1) * richGridColl.TileSize.Width, colliderBPos.Y + row * richGridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + (column + 1) * richGridColl.TileSize.Width, colliderBPos.Y + (row + 1) * richGridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + column * richGridColl.TileSize.Width, colliderBPos.Y + (row + 1) * richGridColl.TileSize.Height)
                            ), axes)) {
                            return true;
                        }
                    }
                }

                return false;
            }*/

            Rectangle boxRect = new Rectangle(colliderAPos, boxColl.Size);
            if (!(boxRect & new Rectangle(colliderBPos, richGridColl.Size))) { // out of grid
                return false;
            }

            // box collider polygon
            Polygon boxPolygon = boxColl.Polygon;
            boxPolygon.Translate(colliderAPos);

            startColumn = (int) (boxRect.Left - colliderBPos.X) / (int) richGridColl.TileSize.Width;
            startRow = (int) (boxRect.Top - colliderBPos.Y) / (int) richGridColl.TileSize.Height;
            endColumn = (int) (boxRect.Right - colliderBPos.X - 1) / (int) richGridColl.TileSize.Width;
            endRow = (int) (boxRect.Bottom - colliderBPos.Y - 1) / (int) richGridColl.TileSize.Height;
            for (row = startRow; row <= endRow; row++) {
                for (column = startColumn; column <= endColumn; column++) {
                    if (!richGridColl.IsCollidable(column, row)) {
                        continue;
                    }

                    RichGridCollider.Tile tile = richGridColl.GetTileInfo(column, row);
                    if (tile is RichGridCollider.BoxTile) {
                        return true;
                    }

                    // rich grid collider tile (column, row) polygon
                    Polygon tilePolygon = (tile as RichGridCollider.PolygonTile).Polygon.Clone();

                    List<Vector2> axes = new List<Vector2>();
                    Vector2 boxAxis0 = (boxPolygon[0] - boxPolygon[1]).Perpendicular(), 
                            boxAxis1 = (boxPolygon[1] - boxPolygon[2]).Perpendicular();

                    if (tilePolygon.IsConvex) {
                        tilePolygon.Translate(colliderBPos + new Vector2(column, row) * richGridColl.TileSize);

                        // box relevant axes
                        axes.Add(boxAxis0);
                        axes.Add(boxAxis1);

                        // tile axes
                        for (int i = 0; i < tilePolygon.VertexCount; i++) {
                            axes.Add((tilePolygon[i] - tilePolygon[(i + 1) % tilePolygon.VertexCount]).Perpendicular());
                        }

                        if (CheckPolygonsIntersection(boxPolygon, tilePolygon, axes)) {
                            return true;
                        }
                    } else {
                        foreach (Polygon convexComponent in tilePolygon.GetConvexComponents()) {
                            convexComponent.Translate(colliderBPos + new Vector2(column, row) * richGridColl.TileSize);

                            axes.Clear();

                            // box relevant axes
                            axes.Add(boxAxis0);
                            axes.Add(boxAxis1);

                            // tile axes
                            for (int i = 0; i < convexComponent.VertexCount; i++) {
                                axes.Add((convexComponent[i] - convexComponent[(i + 1) % convexComponent.VertexCount]).Perpendicular());
                            }

                            if (CheckPolygonsIntersection(boxPolygon, convexComponent, axes)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion Box vs RichGrid

        #endregion Box

        #region Grid

        #region Grid vs Grid

        private bool CheckGridGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Grid vs Grid

        #region Grid vs Box

        private bool CheckGridBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Grid vs Box

        #region Grid vs Circle

        private bool CheckGridCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckCircleGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Grid vs Circle

        #region Grid vs Line

        private bool CheckGridLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckLineGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Grid vs Line

        #region Grid vs Polygon

        private bool CheckGridPolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckPolygonGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Grid vs Polygon

        #region Grid vs RichGrid

        private bool CheckGridRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return true;
        }

        #endregion Grid vs RichGrid

        #endregion Grid

        #region Circle

        #region Circle vs Circle

        private bool CheckCircleCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            CircleCollider circleA = colliderA as CircleCollider, circleB = colliderB as CircleCollider;
            Vector2 centerDiff = (colliderBPos + circleB.Radius) - (colliderAPos + circleA.Radius);
            return centerDiff.X * centerDiff.X + centerDiff.Y * centerDiff.Y <= (circleA.Radius + circleB.Radius) * (circleA.Radius + circleB.Radius);
        }

        #endregion Circle vs Circle

        #region Circle vs Box

        private bool CheckCircleBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxCircle(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Circle vs Box

        #region Circle vs Grid

        private bool CheckCircleGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Circle vs Grid

        #region Circle vs Line

        private bool CheckCircleLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            CircleCollider circleColl = colliderA as CircleCollider;
            LineCollider lineColl = colliderB as LineCollider;

            if (Util.Math.DistanceSquared(new Line(colliderBPos + lineColl.From, colliderBPos + lineColl.To), circleColl.Center) < circleColl.Radius * circleColl.Radius) {
                return true;
            }

            return false;
        }

        #endregion Circle vs Line

        #region Circle vs Polygon

        private bool CheckCirclePolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            CircleCollider circleColl = colliderA as CircleCollider;
            PolygonCollider polygonColl = colliderB as PolygonCollider;

            float radiusSquared = circleColl.Radius * circleColl.Radius;
            Polygon polygon = polygonColl.Polygon.Clone();
            polygon.Translate(colliderBPos);
            for (int i = 0; i < polygon.VertexCount; i++) {
                if (Util.Math.DistanceSquared(new Line(polygon[i], polygon[(i + 1) % polygon.VertexCount]), circleColl.Center) < radiusSquared) {
                    return true;
                }
            }

            return false;
        }

        #endregion Circle vs Polygon

        #region Circle vs RichGrid

        private bool CheckCircleRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Circle vs RichGrid

        #endregion Circle

        #region Line

        #region Line vs Line

        private bool CheckLineLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            LineCollider lineA = colliderA as LineCollider, lineB = colliderB as LineCollider;
            Vector2 lineAFrom = lineA.From + colliderAPos, lineATo = lineA.To + colliderAPos,
                    lineBFrom = lineB.From + colliderBPos, lineBTo = lineB.To + colliderBPos;

            Vector2 lineALength = lineA.To - lineA.From, lineBLength = lineB.To - lineB.From;

            float lengthsCross = Vector2.Cross(lineALength, lineBLength);
            float fromDiffCrossLengthA = Vector2.Cross(lineBFrom - lineAFrom, lineALength);

            if (lengthsCross == 0) {
                if (fromDiffCrossLengthA == 0) { // collinear
                    float t0 = (lineBFrom - lineAFrom).Dot(lineALength / lineALength.Dot(lineALength));
                    float t1 = t0 + lineBLength.Dot(lineALength / lineALength.Dot(lineALength));
                    return Vector2.Dot(lineBLength, lineALength) < 0 ? !(t1 > 1 || t0 < 0) : !(t0 > 1 || t1 < 0);
                }

                return false;
            }

            float t = Vector2.Cross(lineBFrom - lineAFrom, lineBLength) / lengthsCross, u = fromDiffCrossLengthA / lengthsCross;
            return !((t < 0 || t > 1) || (u < 0 || u > 1));
        }

        #endregion Line vs Line

        #region Line vs Box

        private bool CheckLineBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxLine(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Line vs Box

        private bool CheckLineGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #region Line vs Circle

        private bool CheckLineCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckCircleLine(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Line vs Circle

        #region Line vs Polygon

        private bool CheckLinePolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            LineCollider lineColl = colliderA as LineCollider;
            PolygonCollider polygonColl = colliderB as PolygonCollider;

            // line collider polygon
            Polygon linePolygon = new Polygon(new Vector2(lineColl.From), new Vector2(lineColl.To));
            linePolygon.Translate(colliderAPos);

            // polygon collider polygon
            Polygon polygon = polygonColl.Polygon.Clone();
            polygon.Translate(colliderBPos);

            List<Vector2> axes = new List<Vector2>();

            // line axis
            axes.Add((linePolygon[0] - linePolygon[1]).Perpendicular());

            // polygon axes
            for (int i = 0; i < polygon.VertexCount; i++) {
                axes.Add((polygon[i] - polygon[(i + 1) % polygon.VertexCount]).Perpendicular());
            }

            return CheckPolygonsIntersection(linePolygon, polygon, axes);
        }

        #endregion Line vs Polygon

        #region Line vs RichGrid

        private bool CheckLineRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Line vs RichGrid

        #endregion Line

        #region Polygon

        #region Polygon vs Polygon

        private bool CheckPolygonPolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            PolygonCollider polygonAColl = colliderA as PolygonCollider, polygonBColl = colliderB as PolygonCollider;

            // polygon A collider polygon
            Polygon polygonA = polygonAColl.Polygon.Clone();
            polygonA.Translate(colliderAPos);

            // polygon B collider polygon
            Polygon polygonB = polygonBColl.Polygon.Clone();
            polygonB.Translate(colliderBPos);

            List<Vector2> axes = new List<Vector2>();

            // polygon A axes
            for (int i = 0; i < polygonA.VertexCount; i++) {
                axes.Add((polygonA[i] - polygonA[(i + 1) % polygonA.VertexCount]).Perpendicular());
            }

            // polygon B axes
            for (int i = 0; i < polygonB.VertexCount; i++) {
                axes.Add((polygonB[i] - polygonB[(i + 1) % polygonB.VertexCount]).Perpendicular());
            }

            return CheckPolygonsIntersection(polygonA, polygonB, axes);
        }

        #endregion Polygon vs Polygon

        #region Polygon vs Box

        private bool CheckPolygonBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxPolygon(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Polygon vs Box

        private bool CheckPolygonGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #region Polygon vs Circle

        private bool CheckPolygonCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckCirclePolygon(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Polygon vs Circle

        #region Polygon vs Line

        private bool CheckPolygonLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckLinePolygon(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Polygon vs Line

        #region Polygon vs RichGrid

        private bool CheckPolygonRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Polygon vs RichGrid

        #endregion Polygon

        #region RichGrid

        #region RichGrid vs RichGrid

        private bool CheckRichGridRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion RichGrid vs RichGrid

        #region RichGrid vs Polygon

        private bool CheckRichGridPolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckPolygonRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Polygon

        #region RichGrid vs Box

        private bool CheckRichGridBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Box

        #region RichGrid vs Grid

        private bool CheckRichGridGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckGridRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Grid

        #region RichGrid vs Circle

        private bool CheckRichGridCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckCircleRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Circle

        #region RichGrid vs Line

        private bool CheckRichGridLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckLineRichGrid(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion RichGrid vs Line

        #endregion RichGrid

        #region Helper Functions

        private bool CheckPolygonsIntersection(Polygon polygonA, Polygon polygonB, IEnumerable<Vector2> axes) {
            foreach (Vector2 axis in axes) {
                Range projectionA = polygonA.Projection(axis), projectionB = polygonB.Projection(axis);
                if (projectionA.Min >= projectionB.Max || projectionB.Min >= projectionA.Max) {
                    return false;
                }
            }

            return true;
        }

        private bool CheckPolygonsIntersection(Polygon polygonA, Polygon polygonB) {
            List<Vector2> axes = new List<Vector2>();

            // polygon A axes
            Vector2 previousVertex = polygonA[0];
            for (int i = 1; i < polygonA.VertexCount; i++) {
                axes.Add((polygonA[i] - previousVertex).Perpendicular());
                previousVertex = polygonA[i];
            }

            // polygon B axes
            previousVertex = polygonB[0];
            for (int i = 1; i < polygonB.VertexCount; i++) {
                axes.Add((polygonB[i] - previousVertex).Perpendicular());
                previousVertex = polygonB[i];
            }

            return CheckPolygonsIntersection(polygonA, polygonB, axes);
        }

        #endregion Helper Functions

        #endregion Private Methods
    }
}
