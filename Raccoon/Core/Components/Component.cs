using Raccoon.Graphics;

namespace Raccoon.Components {
#if DEBUG
    public abstract class Component : IExtendedUpdatable, IRenderable, IDebugRenderable {
#else
    public abstract class Component : IExtendedUpdatable, IRenderable {
#endif
        public Entity Entity { get; private set; }
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool Enabled { get { return Active || Visible; } set { Active = Visible = value; } }
        public bool IgnoreDebugRender { get; set; }
        public int Order { get; set; }
        public int Layer { get; set; }
        public Renderer Renderer { get; set; }

        public virtual void OnAdded(Entity entity) {
            Entity = entity;
        }

        public virtual void OnRemoved() {
            Entity = null;
        }

        public virtual void BeforeUpdate() {
        }

        public abstract void Update(int delta);

        public virtual void LateUpdate() {
        }

        public abstract void Render();

#if DEBUG
        public abstract void DebugRender();
#endif
    }
}
