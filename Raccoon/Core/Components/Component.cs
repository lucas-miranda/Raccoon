using Raccoon.Graphics;

namespace Raccoon.Components {
#if DEBUG
    public abstract class Component : IExtendedUpdatable, IRenderable, IDebugRenderable {
#else
    public abstract class Component : IExtendedUpdatable, IRenderable {
#endif
        #region Private Members

        private bool _active = true;

        #endregion Private Members

        #region Public Properties

        public Entity Entity { get; private set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get { return Active || Visible; } set { Active = Visible = value; } }
        public bool IgnoreDebugRender { get; set; }
        public int Order { get; set; }
        public int Layer { get; set; }
        public Renderer Renderer { get; set; }

        public bool Active {
            get {
                return _active;
            }

            set {
                if (_active == value) {
                    return;
                }

                bool oldValue = _active;
                _active = value;

                if (oldValue) {
                    OnDeactivate();
                } else {
                    OnActivate();
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void OnAdded(Entity entity) {
            Debug.Assert(entity != null);
            Entity = entity;
            OnActivate();
        }

        public virtual void OnRemoved() {
            Entity = null;
            OnDeactivate();
        }

        public virtual void OnSceneAdded() {
        }

        public virtual void OnSceneRemoved() {
        }

        public virtual void BeforeUpdate() {
            Debug.Assert(Entity != null);
        }

        public abstract void Update(int delta);

        public virtual void LateUpdate() {
            Debug.Assert(Entity != null);
        }

        public abstract void Render();

#if DEBUG
        public abstract void DebugRender();
#endif

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnActivate() {
        }

        protected virtual void OnDeactivate() {
        }

        #endregion Protected Methods
    }
}
