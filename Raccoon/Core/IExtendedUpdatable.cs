namespace Raccoon {
    public interface IExtendedUpdatable : IUpdatable {
        void BeforeUpdate();
        void LateUpdate();
    }
}
