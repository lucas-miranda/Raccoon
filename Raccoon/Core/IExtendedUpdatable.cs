﻿namespace Raccoon {
    public interface IExtendedUpdatable : IUpdatable {
        void BeforeUpdate();
        void LateUpdate();
        void BeforePhysicsStep();
        void PhysicsStep(float stepDelta);
        void LatePhysicsStep();
    }
}
