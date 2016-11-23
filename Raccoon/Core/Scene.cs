using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Private Members

        private bool _started;

        #endregion Private Members

        #region Constructor

        public Scene() {
            Graphics = new List<Graphic>();
            Entities = new List<Entity>();
        }

        #endregion

        #region Public Properties

        public List<Graphic> Graphics { get; protected set; }
        public List<Entity> Entities { get; protected set; }
        public uint Timer { get; private set; }

        #endregion

        #region Public Methods

        public void Add(Graphic graphic) {
            Graphics.Add(graphic);
        }

        public void Add(IEnumerable<Graphic> graphics) {
            Graphics.AddRange(graphics);
        }

        public void Add(Entity entity) {
            Entities.Add(entity);
            entity.OnAdded(this);
            if (_started) {
                entity.Start();
            }
        }
        
        public void Add(IEnumerable<Entity> entities) {
            Entities.AddRange(entities);
            foreach (Entity e in entities) {
                e.OnAdded(this);
                if (_started) {
                    e.Start();
                }
            }
        }

        public void Remove(Graphic graphic) {
            Graphics.Remove(graphic);
        }

        public void Remove(IEnumerable<Graphic> graphics) {
            foreach (Graphic g in graphics) {
                Graphics.Remove(g);
            }
        }

        public void Remove(Entity entity) {
            if (Entities.Remove(entity)) {
                entity.OnRemoved();
            }
        }

        public void Remove(IEnumerable<Entity> entities) {
            foreach (Entity e in entities) {
                if (Entities.Remove(e)) {
                    e.OnRemoved();
                }
            }
        }

        #endregion Public Methods

        #region Public Virtual Methods

        public virtual void OnAdded() { }

        public virtual void Start() {
            foreach (Entity e in Entities) {
                e.Start();
            }
        }

        public virtual void Begin() {
            if (!_started) {
                _started = true;
                Start();
            }
        }

        public virtual void End() {

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

        public virtual void BeforeUpdate() {
            foreach (Entity e in Entities) {
                e.BeforeUpdate();
            }
        }

        public virtual void Update(int delta) {
            Timer += (uint) delta;

            foreach (Graphic g in Graphics) {
                g.Update(delta);
            }

            foreach (Entity e in Entities) {
                e.Update(delta);
            }
        }

        public virtual void LateUpdate() {
            foreach (Entity e in Entities) {
                e.LateUpdate();
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
