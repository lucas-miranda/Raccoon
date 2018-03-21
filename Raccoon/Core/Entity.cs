using System;
using System.Diagnostics;
using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Components;
using Raccoon.Util.Collections;

namespace Raccoon {
    public class Entity {
        #region Public Delegates

        public Action OnSceneAdded = delegate { }, OnSceneRemoved = delegate { }, OnStart = delegate { }, OnSceneBegin = delegate { }, OnSceneEnd = delegate { }, OnBeforeUpdate = delegate { }, OnUpdate = delegate { }, OnLateUpdate = delegate { }, OnRender = delegate { };

#if DEBUG
        public Action OnDebugRender = delegate { };
#endif

        #endregion Public Delegates

        #region Private Members

        private Locker<Component> _components = new Locker<Component>();
        private Surface _surface;

        #endregion Private Members

        #region Constructors

        public Entity() {
            Name = "Entity";
            Surface = Game.Instance.Core.MainSurface;
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; }
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool Enabled { get { return Active || Visible; } set { Active = Visible = value; } }
        public bool AutoUpdate { get; set; } = true;
        public bool AutoRender { get; set; } = true;
        public bool IgnoreDebugRender { get; set; }
        public bool HasStarted { get; private set; }
        public Vector2 Position { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public float Rotation { get; set; }
        public int Layer { get; set; }
        public uint Timer { get; private set; }
        public Locker<Graphic> Graphics { get; private set; } = new Locker<Graphic>(new Graphic.LayerComparer());
        public Scene Scene { get; private set; }

        public Graphic Graphic {
            get {
                return Graphics.Count > 0 ? Graphics[0] : null;
            }

            set {
                if (Graphics.Count == 0) {
                    AddGraphic(value);
                    return;
                }

                Graphics[0] = value;
            }
        }

        public Surface Surface {
            get {
                return _surface;
            }

            set {
                _surface = value;
                foreach (Graphic g in Graphics) {
                    g.Surface = _surface;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void SceneAdded(Scene scene) {
            if (Scene != null) {
                return;
            }

            Scene = scene;
            foreach (Component c in _components) {
                if (c.Entity == this) {
                    continue;
                }

                c.OnAdded(this);
            }

            OnSceneAdded();
        }

        public virtual void SceneRemoved() {
            if (Scene == null) {
                return;
            }

            Scene = null;
            foreach (Component c in _components) {
                if (c.Entity == null) {
                    continue;
                }

                c.OnRemoved();
            }

            OnSceneRemoved();
        }

        public virtual void Start() {
            if (HasStarted) {
                return; 
            }

            HasStarted = true;
            OnStart();
        }

        public virtual void SceneBegin() {
            OnSceneBegin();
        }

        public virtual void SceneEnd() {
            OnSceneEnd();
        }

        public virtual void BeforeUpdate() {
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

            foreach (Component c in _components) {
                if (!c.Active) {
                    continue;
                }

                c.Update(delta);
            }

            OnUpdate();
        }

        public virtual void LateUpdate() {
            OnLateUpdate();
        }

        public virtual void Render() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Render(Position + g.Position, Rotation + g.Rotation);
            }

            foreach (Component c in _components) {
                if (!c.Visible) {
                    continue;
                }

                c.Render();
            }

            OnRender();
        }

        [Conditional("DEBUG")]
        public virtual void DebugRender() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible || g.IgnoreDebugRender) {
                    continue;
                }

                g.DebugRender();
            }

            foreach (Component c in _components) {
                if (!c.Enabled || c.IgnoreDebugRender) {
                    continue;
                }

                c.DebugRender();
            }

#if DEBUG
            OnDebugRender();
#endif
        }

        public void AddGraphic(Graphic graphic) {
            Graphics.Add(graphic);
            graphic.Surface = Surface;
        }

        public void AddGraphics(IEnumerable<Graphic> graphics) {
            Graphics.AddRange(graphics);
            foreach (Graphic g in graphics) {
                g.Surface = Surface;
            }
        }

        public void AddGraphics(params Graphic[] graphics) {
            AddGraphics((IEnumerable<Graphic>) graphics);
        }

        public void AddComponent(Component component) {
            _components.Add(component);
            component.OnAdded(this);
        }

        public void RemoveGraphic(Graphic graphic) {
            Graphics.Remove(graphic);
        }

        public void RemoveGraphics(IEnumerable<Graphic> graphics) {
            Graphics.RemoveRange(graphics);
        }

        public void RemoveGraphics(params Graphic[] graphics) {
            RemoveGraphics((IEnumerable<Graphic>) graphics);
        }

        public void RemoveComponent(Component component) {
            if (_components.Remove(component)) {
                component.OnRemoved();
            }
        }

        public T GetComponent<T>() where T : Component {
            foreach (Component c in _components) {
                if (c is T ct) {
                    return ct;
                }
            }

            return null;
        }

        public List<T> GetComponents<T>() where T : Component {
            List<T> components = new List<T>();
            foreach (Component c in _components) {
                if (c is T ct) {
                    components.Add(ct);
                }
            }

            return components;
        }

        public void RemoveComponents<T>() {
            foreach (Component c in _components) {
                if (c is T && _components.Remove(c)) {
                    c.OnRemoved();
                }
            }
        }

        public void ClearGraphics() {
            Graphics.Clear();
        }

        public void ClearComponents() {
            if (_components.IsLocked) {
                foreach (Component c in _components.ToAdd) {
                    c.OnRemoved();
                }

                foreach (Component c in _components.ToRemove) {
                    c.OnRemoved();
                }
            }

            foreach (Component c in _components.Items) {
                c.OnRemoved();
            }

            _components.Clear();
        }

        public override string ToString() {
            return $"[Entity '{Name}' | X: {X} Y: {Y} Graphics: {Graphics.Count} Components: {_components.Count}]";
        }

        #endregion Public Methods

        #region Private Methods


        #endregion Private Methods

        #region Layer Comparer

        public class LayerComparer : IComparer<Entity> {
            public int Compare(Entity x, Entity y) {
                return Math.Sign(x.Layer - y.Layer);
            }
        }

        #endregion Layer Comparer
    }
}
