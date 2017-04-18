using System;
using System.Diagnostics;
using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Components;
using Raccoon.Collections;

namespace Raccoon {
    public class Entity {
        #region Public Delegates

        public Action OnUpdate = delegate { }, OnRender = delegate { }, OnDebugRender = delegate { }, OnRemoved = delegate { };

        #endregion Public Delegates

        #region Private Members
        
        private Locker<Component> _components = new Locker<Component>();
        private Surface _surface;
        
        #endregion Private Members

        #region Constructors

        public Entity() {
            _components.OnAdded += (Component c) => c.OnAdded(this);
            _components.OnRemoved += (Component c) => c.OnRemoved();

            Graphics = new Locker<Graphic>(new Graphic.LayerComparer());
            Graphics.OnAdded += (Graphic g) => g.Surface = Surface;

            Name = "Entity";
            Active = Visible = true;
            Surface = Game.Instance.Core.MainSurface;

            OnRemoved += () => {
                Scene = null;
            };
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Visible { get; set; }
        public bool Enabled { get { return Active || Visible; } set { Active = Visible = value; } }
        public bool AutoUpdate { get; set; } = true;
        public bool AutoRender { get; set; } = true;
        public Vector2 Position { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public float Rotation { get; set; }
        public int Layer { get; set; }
        public uint Timer { get; private set; }
        public Locker<Graphic> Graphics { get; private set; }
        public Scene Scene { get; private set; }

        public Graphic Graphic {
            get {
                return Graphics.Count > 0 ? Graphics[0] : null;
            }

            set {
                if (Graphics.Count == 0) {
                    Add(value);
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

        public virtual void OnAdded(Scene scene) {
            Scene = scene;
            foreach (Component c in _components) {
                if (c is Collider) {
                    Collider collider = c as Collider;
                    Physics.Instance.AddCollider(collider, collider.Tags);
                }
            }
        }

        public virtual void Start() { }

        public virtual void BeforeUpdate() {
            Graphics.Upkeep();
            _components.Upkeep();
        }

        public virtual void Update(int delta) {
            Timer += (uint) delta;
            
            foreach (Component c in _components) {
                if (!c.Enabled) {
                    continue;
                }

                c.Update(delta);
            }

            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Update(delta);
            }

            OnUpdate.Invoke();
        }

        public virtual void LateUpdate() { }

        public virtual void Render() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Render(Position + g.Position, Rotation + g.Rotation);
            }

            OnRender.Invoke();
        }

        [Conditional("DEBUG")]
        public virtual void DebugRender() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.DebugRender();
            }

            foreach (Component c in _components) {
                if (!c.Enabled) {
                    continue;
                }

                c.DebugRender();
            }

            OnDebugRender.Invoke();
        }

        public void Add(Graphic graphic) {
            Graphics.Add(graphic);
        }

        public void Add(IEnumerable<Graphic> graphics) {
            Graphics.AddRange(graphics);
        }

        public void Add(Component component) {
            _components.Add(component);
        }

        public void Remove(Graphic graphic) {
            Graphics.Remove(graphic);
        }

        public void Remove(IEnumerable<Graphic> graphics) {
            Graphics.RemoveRange(graphics);
        }

        public void Remove(Component component) {
            _components.Remove(component);
        }

        public T GetComponent<T>() where T : Component {
            foreach (Component c in _components) {
                if (c is T) {
                    return c as T;
                }
            }

            return null;
        }

        public List<T> GetComponents<T>() where T : Component {
            List<T> components = new List<T>();
            foreach (Component c in _components) {
                if (c is T) {
                    components.Add(c as T);
                }
            }

            return components;
        }

        public void RemoveComponents<T>() {
            foreach (Component c in _components) {
                if (c is T) {
                    _components.Remove(c);
                }
            }
        }

        public void ClearGraphics() {
            Graphics.Clear();
        }

        public void ClearComponents() {
            _components.Clear();
        }

        public override string ToString() {
            return $"[Entity '{Name}' | X: {X} Y: {Y} Graphics: {Graphics.Count} Components: {_components.Count}]";
        }

        #endregion Public Methods

        #region Layer Comparer

        public class LayerComparer : IComparer<Entity> {
            public int Compare(Entity x, Entity y) {
                return Math.Sign(x.Layer - y.Layer);
            }
        }

        #endregion Layer Comparer
    }
}
