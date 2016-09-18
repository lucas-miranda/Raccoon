namespace Raccoon.Components {
    public abstract class Component {
        public Entity Entity { get; private set; }

        public virtual void Added(Entity entity) {
            Entity = entity;
        }

        public abstract void Update(int delta);
        public abstract void Render();
    }
}
