using Raccoon.Components;
using Raccoon.Util;

namespace Raccoon {
    public class Manifold {
        public const float PenetrationAllowance = 0.01f,
                           PenetrationPercentageToCorrect = 0.2f;

        public Manifold(Body a, Body b) {
            A = a;
            B = b;
        }

        public Body A { get; private set; }
        public Body B { get; private set; }
        public Contact[] Contacts { get; set; }

        public void ImpulseResolution() {
            Contact? contact = FindLeastPenetrationContact();
            if (contact == null) {
                return;
            }

            Vector2 relativeVelocity = B.Velocity - A.Velocity;
            float contactVelocity = Vector2.Dot(relativeVelocity, contact.Value.Normal);

            // velocities are separating
            if (contactVelocity > 0) {
                return;
            }

            float e = Math.Min(A.Material.Restitution, B.Material.Restitution);
            float impulseScalar = (-(1.0f + e) * contactVelocity) / (A.InverseMass + B.InverseMass);

            Vector2 impulse = impulseScalar * contact.Value.Normal;
            A.Velocity -= A.InverseMass * impulse;
            B.Velocity += B.InverseMass * impulse;
        }

        public void PositionCorrection() {
            Contact? contact = FindLeastPenetrationContact();
            if (contact == null) {
                return;
            }

            Vector2 correction = (Math.Max(contact.Value.PenetrationDepth - PenetrationAllowance, 0f) / (A.InverseMass + B.InverseMass)) * PenetrationPercentageToCorrect * contact.Value.Normal;
            A.Position -= A.InverseMass * correction;
            B.Position -= B.InverseMass * correction;
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
    }
}
