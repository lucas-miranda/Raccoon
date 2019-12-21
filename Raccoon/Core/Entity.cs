using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Components;
using Raccoon.Util.Collections;

namespace Raccoon {
    public class Entity : ISceneObject {
        #region Public Delegates

        public event SceneObjectEvent OnSceneAdded      = delegate { },
                                      OnSceneRemoved    = delegate { },
                                      OnStart           = delegate { },
                                      OnSceneBegin      = delegate { },
                                      OnSceneEnd        = delegate { },
                                      OnBeforeUpdate    = delegate { },
                                      OnUpdate          = delegate { },
                                      OnLateUpdate      = delegate { },
                                      OnRender          = delegate { };

#if DEBUG
        public event SceneObjectEvent OnDebugRender     = delegate { };
#endif

        #endregion Public Delegates

        #region Private Members

        private Renderer _renderer;

        #endregion Private Members

        #region Constructors

        public Entity() {
            Name = "Entity";
            Renderer = Game.Instance.MainRenderer;
            Transform = new Transform(this);
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; }
        public Transform Transform { get; private set; }
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool Enabled { get { return Active || Visible; } set { Active = Visible = value; } }
        public bool AutoUpdate { get; set; } = true;
        public bool AutoRender { get; set; } = true;
        public bool IgnoreDebugRender { get; set; }
        public bool HasStarted { get; private set; }
        public bool WipeOnRemoved { get; set; } = true;
        public bool IsWiped { get; private set; }
        public int Order { get; set; }
        public int Layer { get; set; }
        public uint Timer { get; private set; }
        public Locker<Graphic> Graphics { get; private set; } = new Locker<Graphic>(Graphic.LayerComparer);
        public Locker<Component> Components { get; private set; } = new Locker<Component>();
        public Scene Scene { get; private set; }
        public bool IsSceneFromTransformAncestor { get; internal set; }

        public Graphic Graphic {
            get {
                if (Graphics == null || Graphics.Count == 0) {
                    return null;
                }

                return Graphics[0];
            }

            set {
                if (Graphics.Count == 0) {
                    if (value == null) {
                        return;
                    }

                    AddGraphic(value);
                    return;
                } else if (value == null) {
                    RemoveGraphic(Graphics[0]);
                    return;
                }

                Graphics[0] = value;
            }
        }

        public Renderer Renderer {
            get {
                return _renderer;
            }

            set {
                _renderer = value;
                foreach (Graphic g in Graphics) {
                    g.Renderer = _renderer;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static int LayerComparer(Entity a, Entity b) {
            return a.Layer.CompareTo(b.Layer);
        }

        public virtual void SceneAdded(Scene scene) {
            if (Scene != null) {
                return;
            }

            Scene = scene;
            foreach (Component c in Components) {
                c.OnSceneAdded();
            }

            Transform.EntitySceneAdded(Scene);
            OnSceneAdded();
        }

        public virtual void SceneRemoved(bool allowWipe = true) {
            if (IsWiped) {
                // avoid more than one wipe call
                return;
            }

            Scene = null;

            if (WipeOnRemoved && allowWipe) {
                IsWiped = true;
                Enabled = false;
                OnSceneAdded = null;
                OnStart = null;
                OnSceneBegin = null;
                OnSceneEnd = null;
                OnBeforeUpdate = null;
                OnUpdate = null;
                OnLateUpdate = null;
                OnRender = null;
#if DEBUG
                OnDebugRender = null;
#endif

                _renderer = null;
            }

            foreach (Component c in Components) {
                if (c.Entity == null) {
                    continue;
                }

                c.OnSceneRemoved(WipeOnRemoved && allowWipe);
            }

            Transform.EntitySceneRemoved(WipeOnRemoved && allowWipe);
            OnSceneRemoved?.Invoke();

            if (WipeOnRemoved && allowWipe) {
                OnSceneRemoved = null;
                Transform.Detach();

                foreach (Graphic g in Graphics) {
                    GraphicRemoved(g);
                    g.Dispose();
                }

                Graphics.Clear();

                foreach (Component c in Components) {
                    ComponentRemoved(c);
                    c.OnRemoved();
                    c.Dispose();
                }

                Components.Clear();
            }
        }

        public virtual void Start() {
            if (HasStarted) {
                return;
            }

            HasStarted = true;

            foreach (Transform child in Transform) {
                if (child.Entity.HasStarted) {
                    continue;
                }

                child.Entity.Start();
            }

            OnStart();
        }

        public virtual void SceneBegin() {
            foreach (Transform child in Transform) {
                if (child.IsDetached || !child.Entity.IsSceneFromTransformAncestor) {
                    continue;
                }

                child.Entity.SceneBegin();
            }

            OnSceneBegin();
        }

        public virtual void SceneEnd() {
            foreach (Transform child in Transform) {
                if (child.IsDetached || !child.Entity.IsSceneFromTransformAncestor) {
                    continue;
                }

                child.Entity.SceneEnd();
            }

            OnSceneEnd();
        }

        public virtual void BeforeUpdate() {
            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.BeforeUpdate();
            }

            foreach (Transform child in Transform) {
                if (child.IsDetached || !child.Entity.IsSceneFromTransformAncestor || !child.Entity.Active) {
                    continue;
                }

                child.Entity.BeforeUpdate();
            }

            OnBeforeUpdate();
        }

        public virtual void Update(int delta) {
            Timer += (uint) delta;

            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Update(delta);
            }

            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.Update(delta);
            }

            foreach (Transform child in Transform) {
                if (child.IsDetached || !child.Entity.IsSceneFromTransformAncestor || !child.Entity.Active) {
                    continue;
                }

                child.Entity.Update(delta);
            }

            OnUpdate();
        }

        public virtual void LateUpdate() {
            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.LateUpdate();
            }

            foreach (Transform child in Transform) {
                if (child.IsDetached || !child.Entity.IsSceneFromTransformAncestor || !child.Entity.Active) {
                    continue;
                }

                child.Entity.LateUpdate();
            }

            OnLateUpdate();
        }

        public virtual void Render() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Render(Transform.Position, Transform.Rotation, Vector2.One, ImageFlip.None, Color.White, Vector2.One, null, Layer);
            }

            foreach (Component c in Components) {
                if (!c.Visible) {
                    continue;
                }

                c.Render();
            }

            foreach (Transform child in Transform) {
                if (child.IsDetached || !child.Entity.IsSceneFromTransformAncestor || !child.Entity.Visible) {
                    continue;
                }

                child.Entity.Render();
            }

            OnRender();
        }

#if DEBUG
        public virtual void DebugRender() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible || g.IgnoreDebugRender) {
                    continue;
                }

                g.DebugRender();
            }

            foreach (Component c in Components) {
                if (!c.Enabled || c.IgnoreDebugRender) {
                    continue;
                }

                c.DebugRender();
            }

            foreach (Transform child in Transform) {
                if (child.IsDetached || !child.Entity.IsSceneFromTransformAncestor || !child.Entity.Visible) {
                    continue;
                }

                child.Entity.DebugRender();
            }

            OnDebugRender();
        }
#endif

        public Graphic AddGraphic(Graphic graphic) {
            if (graphic == null) {
                throw new System.ArgumentNullException(nameof(graphic));
            }

            if (!GraphicAdded(graphic)) {
                return null;
            }

            Graphics.Add(graphic);
            graphic.Renderer = Renderer;
            return graphic;
        }

        public T AddGraphic<T>(T graphic) where T : Graphic {
            return AddGraphic(graphic as Graphic) as T;
        }

        public void AddGraphics(IEnumerable<Graphic> graphics) {
            foreach (Graphic g in graphics) {
                AddGraphic(g);
            }
        }

        public void AddGraphics(params Graphic[] graphics) {
            AddGraphics((IEnumerable<Graphic>) graphics);
        }

        public Component AddComponent(Component component) {
            if (component == null) {
                throw new System.ArgumentNullException(nameof(component));
            }

            if (!ComponentAdded(component)) {
                return null;
            }

            Components.Add(component);
            component.OnAdded(this);
            return component;
        }

        public T AddComponent<T>(T component) where T : Component {
            return AddComponent(component as Component) as T;
        }

        public bool RemoveGraphic(Graphic graphic) {
            if (graphic == null) {
                return false;
            }

            if (Graphics.Remove(graphic)) {
                GraphicRemoved(graphic);
                return true;
            }

            return false;
        }

        public void RemoveGraphics(IEnumerable<Graphic> graphics) {
            foreach (Graphic g in graphics) {
                RemoveGraphic(g);
            }
        }

        public void RemoveGraphics(params Graphic[] graphics) {
            RemoveGraphics((IEnumerable<Graphic>) graphics);
        }

        public void RemoveComponent(Component component) {
            if (Components == null || !Components.Remove(component)) {
                return;
            }

            ComponentRemoved(component);
            component.OnRemoved();
        }

        public T GetComponent<T>() where T : Component {
            if (Components == null) {
                return null;
            }

            foreach (Component c in Components) {
                if (c is T ct) {
                    return ct;
                }
            }

            return null;
        }

        public List<T> GetComponents<T>() where T : Component {
            List<T> components = new List<T>();
            foreach (Component c in Components) {
                if (c is T ct) {
                    components.Add(ct);
                }
            }

            return components;
        }

        public void RemoveComponents<T>() {
            foreach (Component c in Components) {
                if (c is T && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.OnRemoved();
                }
            }
        }

        public void ClearGraphics() {
            if (Graphics == null) {
                return;
            }

            foreach (Graphic g in Graphics) {
                GraphicRemoved(g);
            }

            Graphics.Clear();
        }

        public void ClearComponents() {
            if (Components == null) {
                return;
            }

            if (Components.IsLocked) {
                foreach (Component c in Components.ToAdd) {
                    ComponentRemoved(c);
                    c.OnRemoved();
                }

                foreach (Component c in Components.ToRemove) {
                    ComponentRemoved(c);
                    c.OnRemoved();
                }
            }

            foreach (Component c in Components.Items) {
                ComponentRemoved(c);
                c.OnRemoved();
            }

            Components.Clear();
        }

        public void RemoveSelf() {
            if (Scene == null) {
                throw new System.NullReferenceException("Can't remove from a null Scene.");
            }

            Scene.RemoveEntity(this);
        }

        public override string ToString() {
            return $"[Entity '{Name}' | {Transform} | Graphics: {Graphics.Count} Components: {Components.Count}]";
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual bool GraphicAdded(Graphic graphic) {
            return true;
        }

        protected virtual void GraphicRemoved(Graphic graphic) {
        }

        protected virtual bool ComponentAdded(Component component) {
            return true;
        }

        protected virtual void ComponentRemoved(Component component) {
        }

        #endregion Protected Methods
    }
}
