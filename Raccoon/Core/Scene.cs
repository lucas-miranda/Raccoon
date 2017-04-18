using System.Collections.Generic;

using Raccoon.Collections;
using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Private Members

        private Camera _camera;

        #endregion Private Members

        #region Constructor

        public Scene() {
            Graphics = new Locker<Graphic>(new Graphic.LayerComparer());
            Entities = new Locker<Entity>(new Entity.LayerComparer());
            Entities.OnRemoved += (Entity e) => e.OnRemoved.Invoke();

            Camera = new Camera();
        }

        #endregion

        #region Public Properties

        public Locker<Graphic> Graphics { get; protected set; }
        public Locker<Entity> Entities { get; protected set; }
        public uint Timer { get; private set; }
        public bool HasStarted { get; private set; } 

        public Camera Camera {
            get {
                return _camera;
            }

            set {
                _camera = value;
                if (HasStarted) {
                    _camera.Start();
                }
            }
        }

        #endregion  

        #region Public Methods

        public void Add(Graphic graphic) {
            Graphics.Add(graphic);
        }

        public void Add(IEnumerable<Graphic> graphics) {
            Graphics.AddRange(graphics);
        }

        public void Add(params Graphic[] graphics) {
            Add((IEnumerable<Graphic>) graphics);
        }

        public void Add(Entity entity) {
            Entities.Add(entity);
            entity.OnAdded(this);
            if (HasStarted) {
                entity.Start();
            }
        }
        
        public void Add(IEnumerable<Entity> entities) {
            Entities.AddRange(entities);
            foreach (Entity e in entities) {
                e.OnAdded(this);
                if (HasStarted) {
                    e.Start();
                }
            }
        }

        public void Add(params Entity[] entities) {
            Add((IEnumerable<Entity>) entities);
        }

        public void Remove(Graphic graphic) {
            Graphics.Remove(graphic);
        }

        public void Remove(IEnumerable<Graphic> graphics) {
            Graphics.RemoveRange(graphics);
        }

        public void Remove(params Graphic[] graphics) {
            Remove((IEnumerable<Graphic>) graphics);
        }

        public void Remove(Entity entity) {
            Entities.Remove(entity);
        }

        public void Remove(IEnumerable<Entity> entities) {
            Entities.RemoveRange(entities);
        }

        public void Remove(params Entity[] entities) {
            Remove((IEnumerable<Entity>) entities);
        }

        public void ClearEntities() {
            Entities.Clear();
        }

        public void ClearGraphics() {
            Graphics.Clear();
        }

        #endregion Public Methods

        #region Public Virtual Methods

        public virtual void OnAdded() { }

        public virtual void Start() {
            HasStarted = true;
            foreach (Entity e in Entities) {
                e.Start();
            }

            Camera.Start();
        }

        public virtual void Begin() {
            if (!HasStarted) {
                Start();
            }

            Camera.Begin();
        }

        public virtual void End() {
            Camera.End();
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
            Entities.Upkeep();
            Graphics.Upkeep();

            foreach (Entity e in Entities) {
                if (!e.Active || !e.AutoUpdate) {
                    continue;
                }

                e.BeforeUpdate();
            }
        }

        public virtual void Update(int delta) {
            Timer += (uint) delta;

            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Update(delta);
            }

            foreach (Entity e in Entities) {
                if (!e.Active || !e.AutoUpdate) {
                    continue;
                }

                e.Update(delta);
            }

            Camera.Update(delta);
        }

        public virtual void LateUpdate() {
            foreach (Entity e in Entities) {
                if (!e.Active || !e.AutoUpdate) {
                    continue;
                }

                e.LateUpdate();
            }
        }

        public virtual void Render() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Render();
            }

            foreach (Entity e in Entities) {
                if (!e.Visible || !e.AutoRender) {
                    continue;
                }

                e.Render();
            }
        }

        public virtual void DebugRender() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.DebugRender();
            }

            foreach (Entity e in Entities) {
                if (!e.Visible || !e.AutoRender) {
                    continue;
                }

                e.DebugRender();
            }

            Camera.DebugRender();
        }

        #endregion
    }
}
