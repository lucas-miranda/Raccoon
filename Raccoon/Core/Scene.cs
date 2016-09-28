using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Constructor

        public Scene() {
            Graphics = new List<Graphic>();
            Entities = new List<Entity>();
        }

        #endregion

        #region Public Properties

        public List<Graphic> Graphics { get; protected set; }
        public List<Entity> Entities { get; protected set; }

        #endregion

        #region Public Methods

        public void Add(Graphic g) {
            Graphics.Add(g);
        }

        public void Add(IEnumerable<Graphic> graphics) {
            Graphics.AddRange(graphics);
        }

        public void Add(Entity e) {
            Entities.Add(e);
            if (Game.Instance.IsRunning) {
                foreach (Graphic g in e.Graphics) {
                    g.Load();
                }
            }
        }

        public void Add(IEnumerable<Entity> objects) {
            Entities.AddRange(objects);
            if (Game.Instance.IsRunning) {
                foreach (Entity e in objects) {
                    foreach (Graphic g in e.Graphics) {
                        g.Load();
                    }
                }
            }
        }

        #endregion Public Methods

        #region Public Virtual Methods

        public virtual void Added() { }

        public virtual void Initialize() {
            foreach (Entity e in Entities) {
                e.Initialize();
            }
        }

        public virtual void LoadContent() {
            foreach (Graphic g in Graphics) {
                g.Load();
            }

            foreach (Entity e in Entities) {
                foreach (Graphic g in e.Graphics) {
                    g.Load();
                }
            }
        }

        public virtual void UnloadContent() { }

        public virtual void Update(int delta) {
            foreach (Graphic g in Graphics) {
                g.Update(delta);
            }

            foreach (Entity e in Entities) {
                e.Update(delta);
            }
        }

        public virtual void Render() {
            foreach (Graphic g in Graphics) {
                g.Render();
            }

            foreach (Entity e in Entities) {
                e.Render();
            }
        }

        public virtual void DebugRender() {
            foreach (Entity e in Entities) {
                e.DebugRender();
            }
        }

        #endregion
    }
}
