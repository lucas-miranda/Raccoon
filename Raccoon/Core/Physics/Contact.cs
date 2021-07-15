namespace Raccoon {
    public struct Contact {
        public Vector2 Position, Normal;
        public float PenetrationDepth;
        public Location? Cell;

        public Contact(Vector2 position, Vector2 normal, float penetrationDepth, Location? cell) {
            Position = position;
            Normal = normal;
            PenetrationDepth = penetrationDepth;
            Cell = cell;
        }

        public Vector2 PenetrationVector {
            get {
                return PenetrationDepth * Normal;
            }
        }

        public override string ToString() {
            if (!Cell.HasValue) {
                return $"({Position}, {Normal}, {PenetrationDepth})";
            }

            return $"({Position}, {Normal}, {PenetrationDepth}; Cell: {Cell})";
        }

        public static Contact Invert(Contact c) {
            return new Contact(c.Position, -c.Normal, c.PenetrationDepth, c.Cell);
        }
    }
}
