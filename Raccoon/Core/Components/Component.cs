﻿namespace Raccoon.Components {
    public abstract class Component {
        public Entity Entity { get; private set; }
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool Enabled { get { return Active || Visible; } set { Active = Visible = value; } }
        public bool IgnoreDebugRender { get; set; }

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

        [System.Diagnostics.Conditional("DEBUG")]
        public abstract void DebugRender();
    }
}
