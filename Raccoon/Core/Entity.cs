using System;
using System.Diagnostics;
using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Components;

namespace Raccoon {
    public class Entity {
        #region Public Delegates

        public Action OnRemoved = delegate { };

        #endregion Public Delegates

        #region Private Members
        
        private List<Component> _components;
        private List<Graphic> _graphicsToAdd;
        private List<Component> _componentsToAdd, _componentsToRemove;
        private Surface _surface;
        
        #endregion Private Members

        #region Constructors

        public Entity() {
            Graphics = new List<Graphic>();
            _graphicsToAdd = new List<Graphic>();

            _components = new List<Component>();
            _componentsToAdd = new List<Component>();
            _componentsToRemove = new List<Component>();

            Name = "Entity";
            Active = Visible = true;
            Surface = Game.Instance.Core.MainSurface;

            OnRemoved += () => {
                Enabled = false;
                Scene = null;
                ClearComponents();
                ClearGraphics();
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
        public List<Graphic> Graphics { get; private set; }
        public Scene Scene { get; private set; }

        public Graphic Graphic {
            get {
                return Graphics.Count > 0 ? Graphics[0] : null;
            }

            set {
                if (Graphics.Count > 0) {
                    Graphics[0] = value;
                    return;
                }

                Add(value);
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
                Collider coll = c as Collider;
                if (coll != null) {
                    Physics.Instance.AddCollider(coll, coll.Tags);
                }
            }
        }

        public virtual void Start() { }

        public virtual void BeforeUpdate() {
            // check graphics Layer
            if (Graphics.Count > 1) {
                int previousLayer = Graphics[0].Layer;
                for (int i = 1; i < Graphics.Count; i++) {
                    if (previousLayer > Graphics[i].Layer) {
                        _graphicsToAdd.Add(Graphics[i - 1]);
                        Graphics.RemoveAt(i - 1);
                        i--;
                    }

                    previousLayer = Graphics[i].Layer;
                }
            }

            if (_graphicsToAdd.Count > 0) {
                foreach (Graphic g in _graphicsToAdd) {
                    int index = 0;
                    for (int i = Graphics.Count - 1; i >= 0; i--) {
                        if (Graphics[i].Layer <= g.Layer) {
                            index = i + 1;
                            break;
                        }
                    }

                    g.Surface = Surface;
                    Graphics.Insert(index, g);
                }

                _graphicsToAdd.Clear();
            }

            if (_componentsToAdd.Count > 0) {
                foreach (Component c in _componentsToAdd) {
                    _components.Add(c);
                }

                _componentsToAdd.Clear();
            }

            if (_componentsToRemove.Count > 0) {
                foreach (Component c in _componentsToRemove) {
                    if (_components.Contains(c)) {
                        c.OnRemoved();
                        _components.Remove(c);
                    }
                }

                _componentsToRemove.Clear();
            }
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
        }

        public virtual void LateUpdate() { }

        public virtual void Render() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Render(Position + g.Position, Rotation + g.Rotation);
            }
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
        }

        public void Add(Graphic graphic) {
            int index = 0;
            for (int i = Graphics.Count - 1; i >= 0; i--) {
                if (Graphics[i].Layer <= graphic.Layer) {
                    index = i + 1;
                    break;
                }
            }

            graphic.Surface = Surface;
            Graphics.Insert(index, graphic);
        }

        public void Add(IEnumerable<Graphic> graphics) {
            foreach (Graphic g in graphics) {
                Add(g);
            }
        }

        public void Add(Component component) {
            component.OnAdded(this);
            _componentsToAdd.Add(component);
        }

        public void Remove(Graphic graphic) {
            Graphics.Remove(graphic);
        }

        public void Remove(IEnumerable<Graphic> graphics) {
            foreach (Graphic g in graphics) {
                Graphics.Remove(g);
            }
        }

        public void Remove(Component component) {
            _componentsToRemove.Add(component);
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
                    _componentsToRemove.Add(c);
                }
            }
        }

        public void ClearGraphics() {
            Graphics.Clear();
        }

        public void ClearComponents() {
            foreach (Component c in _components) {
                c.OnRemoved();
            }

            _components.Clear();
        }

        public override string ToString() {
            return $"[Entity '{Name}' | X: {X} Y: {Y}]";
        }

        #endregion Public Methods
    }
}
