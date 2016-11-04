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
        }

        public void Add(IEnumerable<Entity> objects) {
            Entities.AddRange(objects);
        }

        #endregion Public Methods

        #region Public Virtual Methods

        public virtual void OnAdded() { }

        public virtual void Start() {
            foreach (Entity e in Entities) {
                e.Start();
            }
        }

        public virtual void UnloadContent() {
            foreach (Graphic g in Graphics) {
                g.Dispose();
            }

            foreach (Entity e in Entities) {
                foreach (Graphic g in e.Graphics) {
                    g.Dispose();
                }
            }
        }

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
