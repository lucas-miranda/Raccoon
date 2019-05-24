namespace Raccoon {
    public interface IExtendedRenderable : IRenderable {
        void PreRender();
        void AfterRender();
    }
}
