using Raccoon.Components;
using Raccoon.Util;

namespace Raccoon {
    public class Manifold {
        public const float PenetrationAllowance = 0.01f,
                           PenetrationPercentageToCorrect = 0.2f;

        public Manifold(Body a, Body b) {
            A = a;
            B = b;
            AverageRestitution = Math.Min(A.Material.Restitution, B.Material.Restitution);
        }

        public Body A { get; private set; }
        public Body B { get; private set; }
        public Contact[] Contacts { get; set; }
        public float AverageRestitution { get; private set; }

        public void ImpulseResolution() {
            if (A.InverseMass == 0f && B.InverseMass == 0f) {
                A.Force = B.Force = Vector2.Zero;
                A.LastPosition = A.Position;
                B.LastPosition = B.Position;
                return;
            }

            float totalInvMass = A.InverseMass + B.InverseMass;
            foreach (Contact contact in Contacts) {


                /*Vector2 relativeVelocity = B.Velocity - A.Velocity;
                float contactVelocity = Vector2.Dot(relativeVelocity, contact.Normal);

                // bodies are separating
                if (contactVelocity > 0f) {
                    return;
                }

                float impulseScalar = (-(1.0f + AverageRestitution) * contactVelocity) / (A.InverseMass + B.InverseMass);

                Vector2 impulse = impulseScalar * contact.Normal;
                A.Position -= A.InverseMass * impulse;
                B.Position += B.InverseMass * impulse;*/
            }
        }

        public void PositionCorrection() {
            Contact? contact = FindLeastPenetrationContact();
            if (contact == null) {
                return;
            }

            Vector2 correction = (Math.Max(contact.Value.PenetrationDepth - PenetrationAllowance, 0f) / (A.InverseMass + B.InverseMass)) * PenetrationPercentageToCorrect * contact.Value.Normal;

            Vector2 aCorrection = A.InverseMass * correction,
                    bCorrection = B.InverseMass * correction;

            A.Position -= aCorrection;
            A.LastPosition -= aCorrection;
            B.Position -= bCorrection;
            B.LastPosition -= bCorrection;
        }

        public Contact? FindLeastPenetrationContact() {
            if (Contacts.Length == 0) {
                return null;
            }

            Contact contact = Contacts[0];

            // find the least penetration contact
            for (int i = 1; i < Contacts.Length; i++) {
                Contact c = Contacts[i];
                if (c.PenetrationDepth < contact.PenetrationDepth) {
                    contact = Contacts[i];
                }
            }

            return contact;
        }

        public override string ToString() {
            return $"[Contacts: {string.Join(", ", Contacts)}, AverageRestitution: {AverageRestitution}]";
        }
    }
}
