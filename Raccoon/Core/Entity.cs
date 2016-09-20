using Raccoon.Graphics;
using Raccoon.Components;
using System.Collections.Generic;

namespace Raccoon {
    public class Entity {
        #region Private Members

        private Vector2 _position;
        private float _rotation;
        private List<Component> _components;

        #endregion Private Members

        #region Constructors

        public Entity() {
            Name = "Entity";
            Active = Visible = true;
            Graphics = new List<Graphic>();
            _components = new List<Component>();
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public bool Active { get; set; }
        public bool Visible { get; set; }
        public List<Graphic> Graphics { get; private set; }

        public Graphic Graphic {
            get {
                return Graphics.Count > 0 ? Graphics[0] : null;
            }

            set {
                if (Graphics.Count == 0) {
                    Graphics.Add(value);
                } else {
                    Graphics[0] = value;
                }
            }
        }

        public Vector2 Position {
            get {
                return _position;
            }

            set
            {
                _position = value;
                foreach (Graphic g in Graphics) {
                    g.Position = _position;
                }
            }
        }

        public float Rotation {
            get {
                return _rotation;
            }

            set {
                float alpha = value - _rotation;
                _rotation = value;
                foreach (Graphic g in Graphics) {
                    g.Rotation = alpha;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void Initialize() {
        }

        public virtual void Update(int delta) {
            if (!Active) {
                return;
            }

            foreach (Component c in _components) {
                c.Update(delta);
            }

            foreach (Graphic g in Graphics) {
                g.Update(delta);
            }
        }

        public virtual void Render() {
            if (!Visible) {
                return;
            }

            foreach (Graphic g in Graphics) {
                g.Render();
            }
        }

        public void AddGraphic(Graphic graphic) {
            Graphics.Add(graphic);
        }

        public void AddGraphics(IEnumerable<Graphic> graphics) {
            Graphics.AddRange(graphics);
        }

        public void RemoveGraphic(Graphic graphic) {
            Graphics.Remove(graphic);
        }

        public void RemoveGraphics(IEnumerable<Graphic> graphics) {
            foreach (Graphic g in graphics) {
                RemoveGraphic(g);
            }
        }

        public void AddComponent(Component component) {
            _components.Add(component);
            component.Added(this);
        }

        public void RemoveComponent(Component component) {
            _components.Remove(component);
        }


        public override string ToString() {
            return $"[Entity '{Name}' | X: {X} Y: {Y}]";
        }

        #endregion Public Methods
    }
}
