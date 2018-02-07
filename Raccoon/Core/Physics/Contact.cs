using Raccoon.Util;

namespace Raccoon {
    public struct Contact {
        public Vector2 Position, Normal;
        public float PenetrationDepth;

        public Contact(Vector2 position, Vector2 normal, float penetrationDepth) {
            Position = position;
            Normal = normal;
            PenetrationDepth = penetrationDepth;
        }

        public static Contact Sum(Contact contactA, Contact contactB) {
            Vector2 position = (contactA.Position + contactB.Position) / 2f;
            Vector2 normal = ((contactA.Normal + contactB.Normal) / 2f).Normalized();
            float penetrationDepth = Math.Max(System.Math.Abs(Vector2.Dot(contactA.PenetrationVector, normal)), System.Math.Abs(Vector2.Dot(contactB.PenetrationVector, normal)));

            return new Contact(position, normal, penetrationDepth);
        }

        public Vector2 PenetrationVector {
            get {
                return Math.PolarToCartesian(PenetrationDepth, Math.Angle(Normal));
            }
        }

        public override string ToString() {
            return $"[Pos: {Position}, Normal: {Normal}, PenDepth: {PenetrationDepth}]";
        }
    }
}
