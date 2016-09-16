using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Constructor

        public Scene() {
            Graphics = new List<Graphic>();
            Objects = new List<Entity>();
        }

        #endregion

        #region Public Properties

        public List<Graphic> Graphics { get; protected set; }
        public List<Entity> Objects { get; protected set; }

        #endregion

        #region Public Methods

        public void Add(Graphic g) {
            Graphics.Add(g);
        }

        public void Add(IEnumerable<Graphic> graphics) {
            Graphics.AddRange(graphics);
        }

        public void Add(Entity o) {
            Objects.Add(o);
            if (Game.Instance.IsRunning) {
                foreach (Graphic g in o.Graphics) {
                    g.Load();
                }
            }
        }

        public void Add(IEnumerable<Entity> objects) {
            Objects.AddRange(objects);
            if (Game.Instance.IsRunning) {
                foreach (Entity o in objects) {
                    foreach (Graphic g in o.Graphics) {
                        g.Load();
                    }
                }
            }
        }

        #endregion Public Methods

        #region Public Virtual Methods

        public virtual void Initialize() { }
        public virtual void Added() { }

        public virtual void LoadContent() {
            foreach (Graphic g in Graphics) {
                g.Load();
            }

            foreach (Entity o in Objects) {
                foreach (Graphic g in o.Graphics) {
                    g.Load();
                }
            }
        }

        public virtual void UnloadContent() { }

        public virtual void Update(int delta) {
            foreach (Graphic g in Graphics) {
                g.Update(delta);
            }

            foreach (Entity o in Objects) {
                o.Update(delta);
            }
        }

        public virtual void Render() {
            foreach (Graphic g in Graphics) {
                g.Render();
            }

            foreach (Entity o in Objects) {
                o.Render();
            }
        }

        #endregion
    }
}
