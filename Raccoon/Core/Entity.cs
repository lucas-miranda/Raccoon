using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Components;
using Raccoon.Util.Collections;

namespace Raccoon {
    public class Entity : ISceneObject {
        #region Public Delegates

        public event System.Action OnSceneAdded = delegate { }, 
                                   OnSceneRemoved = delegate { }, 
                                   OnStart = delegate { }, 
                                   OnSceneBegin = delegate { }, 
                                   OnSceneEnd = delegate { }, 
                                   OnBeforeUpdate = delegate { }, 
                                   OnUpdate = delegate { }, 
                                   OnLateUpdate = delegate { }, 
                                   OnRender = delegate { };

#if DEBUG
        public event System.Action OnDebugRender = delegate { };
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
        public Transform Transform { get; }
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool Enabled { get { return Active || Visible; } set { Active = Visible = value; } }
        public bool AutoUpdate { get; set; } = true;
        public bool AutoRender { get; set; } = true;
        public bool IgnoreDebugRender { get; set; }
        public bool HasStarted { get; private set; }
        public int Order { get; set; }
        public int Layer { get; set; }
        public uint Timer { get; private set; }
        public Locker<Graphic> Graphics { get; } = new Locker<Graphic>(Graphic.LayerComparer);
        public Locker<Component> Components { get; } = new Locker<Component>();
        public Scene Scene { get; private set; }

        public Graphic Graphic {
            get {
                return Graphics.Count > 0 ? Graphics[0] : null;
            }

            set {
                if (Graphics.Count == 0) {
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
                if (c.Entity == this) {
                    continue;
                }

                c.OnAdded(this);
            }

            foreach (Transform child in Transform) {
                if (child.Entity.Scene == Scene) {
                    continue;
                }

                child.Entity.SceneAdded(Scene);
            }

            OnSceneAdded();
        }

        public virtual void SceneRemoved() {
            if (Scene == null) {
                return;
            }

            Scene = null;
            foreach (Component c in Components) {
                if (c.Entity == null) {
                    continue;
                }

                c.OnRemoved();
            }

            foreach (Transform child in Transform) {
                child.Entity.SceneRemoved();
            }

            OnSceneRemoved();
        }

        public virtual void Start() {
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
                child.Entity.SceneBegin();
            }

            OnSceneBegin();
        }

        public virtual void SceneEnd() {
            foreach (Transform child in Transform) {
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
                if (!child.Entity.Active) {
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
                if (!child.Entity.Active) {
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
                if (!child.Entity.Active) {
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

                g.Render(Transform.Position, Transform.Rotation);
            }

            foreach (Component c in Components) {
                if (!c.Visible) {
                    continue;
                }

                c.Render();
            }

            foreach (Transform child in Transform) {
                if (!child.Entity.Visible) {
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
                if (!child.Entity.Visible) {
                    continue;
                }

                child.Entity.DebugRender();
            }

            OnDebugRender();
        }
#endif

        public Graphic AddGraphic(Graphic graphic) {
            if (graphic == null) {
                return default;
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
                return null;
            }

            Components.Add(component);
            component.OnAdded(this);
            return component;
        }

        public T AddComponent<T>(T component) where T : Component {
            return AddComponent(component as Component) as T;
        }

        public void RemoveGraphic(Graphic graphic) {
            if (graphic == null) {
                return;
            }

            Graphics.Remove(graphic);
        }

        public void RemoveGraphics(IEnumerable<Graphic> graphics) {
            Graphics.RemoveRange(graphics);
        }

        public void RemoveGraphics(params Graphic[] graphics) {
            RemoveGraphics((IEnumerable<Graphic>) graphics);
        }

        public void RemoveComponent(Component component) {
            if (Components.Remove(component)) {
                component.OnRemoved();
            }
        }

        public T GetComponent<T>() where T : Component {
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
                    c.OnRemoved();
                }
            }
        }

        public void ClearGraphics() {
            Graphics.Clear();
        }

        public void ClearComponents() {
            if (Components.IsLocked) {
                foreach (Component c in Components.ToAdd) {
                    c.OnRemoved();
                }

                foreach (Component c in Components.ToRemove) {
                    c.OnRemoved();
                }
            }

            foreach (Component c in Components.Items) {
                c.OnRemoved();
            }

            Components.Clear();
        }

        public void RemoveSelf() {
            if (Scene == null) {
                return;
            }

            Scene.RemoveEntity(this);
        }

        public override string ToString() {
            return $"[Entity '{Name}' | {Transform} | Graphics: {Graphics.Count} Components: {Components.Count}]";
        }

        #endregion Public Methods

        #region Private Methods

        #endregion Private Methods
    }
}
