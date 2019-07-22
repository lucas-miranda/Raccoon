using System.Collections.Generic;

using Raccoon.Components;

namespace Raccoon {
    internal class InternalCollisionInfo {
        public InternalCollisionInfo(Body bodyA, Body bodyB, Vector2 movement, Contact[] horizontalContacts, Contact[] verticalContacts) {
            BodyA = bodyA;
            BodyB = bodyB;
            Movement = movement;
            HorizontalContacts = horizontalContacts;
            VerticalContacts = verticalContacts;
        }

        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vector2 Movement { get; }
        public Contact[] HorizontalContacts { get; }
        public Contact[] VerticalContacts { get; }
    }
}
