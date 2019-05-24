namespace Raccoon {
    public struct Contact {
        public Vector2 Position, Normal;
        public float PenetrationDepth;

        public Contact(Vector2 position, Vector2 normal, float penetrationDepth) {
            Position = position;
            Normal = normal;
            PenetrationDepth = penetrationDepth;
        }

        /*public static Contact Sum(Contact contactA, Contact contactB) {
            Vector2 position = (contactA.Position + contactB.Position) / 2f;

            Vector2 penetrationVectorA = contactA.PenetrationVector,
                    penetrationVectorB = contactB.PenetrationVector;

            float contactAAngle = Math.WrapAngle(Math.Angle(contactA.Normal)),
                  contactBAngle = Math.WrapAngle(Math.Angle(contactB.Normal));

            float contactsArcAngle = Math.AngleArc(contactA.Normal, contactB.Normal);
            float arcStartAngle, arcEndAngle, weigthedAverage;

            if ((contactAAngle < contactBAngle && contactBAngle - contactAAngle <= 180f)
              || (contactAAngle > contactBAngle && contactAAngle + contactsArcAngle >= 360)) {
                arcStartAngle = contactAAngle;
                arcEndAngle = contactBAngle;
                weigthedAverage = contactA.PenetrationDepth == 0f && contactB.PenetrationDepth == 0f ? 0f : contactB.PenetrationDepth / (contactA.PenetrationDepth + contactB.PenetrationDepth);
            } else {
                arcStartAngle = contactBAngle;
                arcEndAngle = contactAAngle;
                weigthedAverage = contactA.PenetrationDepth == 0f && contactB.PenetrationDepth == 0f ? 0f : contactA.PenetrationDepth / (contactA.PenetrationDepth + contactB.PenetrationDepth);
            }

            float bisectorAngle = arcStartAngle + contactsArcAngle * weigthedAverage;

            Vector2 normal = Math.PolarToCartesian(1f, bisectorAngle);
            float penetrationDepth = Math.Max(Vector2.Dot(penetrationVectorA, normal), Vector2.Dot(penetrationVectorB, normal));

            Contact totalContact = new Contact(position, normal, Math.Max(0f, penetrationDepth));
            Debug.Info($"A: {contactA}\nB: {contactB}\n{totalContact}");
            return totalContact;
        }*/

        public Vector2 PenetrationVector {
            get {
                return PenetrationDepth * Normal;
            }
        }

        public override string ToString() {
            return $"[Pos: {Position}, Normal: {Normal}, PenDepth: {PenetrationDepth}]";
        }
    }
}
