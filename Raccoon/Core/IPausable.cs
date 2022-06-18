
namespace Raccoon {
    public interface IPausable {
        ControlGroup ControlGroup { get; }

        void Paused();
        void Resumed();
        void ControlGroupRegistered(ControlGroup controlGroup);
        void ControlGroupUnregistered();
    }
}
