namespace Raccoon.Components {
    public abstract class Component {
        public Entity Entity { get; private set; }
        public bool Enabled { get; set; } = true;
        public bool IgnoreDebugRender { get; set; }

        public virtual void OnAdded(Entity entity) {
            Entity = entity;
        }

        public virtual void OnRemoved() { }

        public abstract void Update(int delta);
        public abstract void Render();

        [System.Diagnostics.Conditional("DEBUG")]
        public abstract void DebugRender();
    }
}
