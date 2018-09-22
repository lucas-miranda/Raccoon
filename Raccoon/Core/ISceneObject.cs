namespace Raccoon {
#if DEBUG
    public interface ISceneObject : IExtendedUpdatable, IRenderable, IDebugRenderable {
#else
    public interface ISceneObject : IExtendedUpdatable, IRenderable {
#endif
        event System.Action OnSceneAdded, OnSceneRemoved, OnStart, OnSceneBegin, OnSceneEnd;

        bool AutoUpdate { get; set; }
        bool AutoRender { get; set; }
        bool IgnoreDebugRender { get; set; }
        bool HasStarted { get; }
        Scene Scene { get; }

        void SceneAdded(Scene scene);
        void SceneRemoved();

        void Start();

        void SceneBegin();
        void SceneEnd();
    }
}
