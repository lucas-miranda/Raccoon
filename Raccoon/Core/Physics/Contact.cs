namespace Raccoon {
    public struct Contact {
        public Vector2 Position, Normal;
        public float PenetrationDepth;

        public Contact(Vector2 position, Vector2 normal, float penetrationDepth) {
            Position = position;
            Normal = normal;
            PenetrationDepth = penetrationDepth;
        }

        public Vector2 PenetrationVector {
            get {
                return PenetrationDepth * Normal;
            }
        }

        public override string ToString() {
            return $"({Position}, {Normal}, {PenetrationDepth})";
        }

        public static Contact Invert(Contact c) {
            return new Contact(c.Position, -c.Normal, c.PenetrationDepth);
        }
    }
}
