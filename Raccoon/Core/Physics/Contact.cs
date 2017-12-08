namespace Raccoon {
    public struct Contact {
        public Vector2 Position, Normal;
        public float PenetrationDepth;

        public Contact(Vector2 position, Vector2 normal, float penetrationDepth) {
            Position = position;
            Normal = normal;
            PenetrationDepth = penetrationDepth;
        }
    }
}
