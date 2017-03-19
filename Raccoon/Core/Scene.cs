using System.Collections.Generic;

using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Private Members

        private bool _hasStarted;
        private List<Graphic> _graphicsToAdd, _graphicsToRemove;
        private List<Entity> _entitiesToAdd, _entitiesToRemove;
        private Camera _camera;

        #endregion Private Members

        #region Constructor

        public Scene() {
            Graphics = new List<Graphic>();
            _graphicsToAdd = new List<Graphic>();
            _graphicsToRemove = new List<Graphic>();

            Entities = new List<Entity>();
            _entitiesToAdd = new List<Entity>();
            _entitiesToRemove = new List<Entity>();

            Camera = new Camera();
        }

        #endregion

        #region Public Properties

        public List<Graphic> Graphics { get; protected set; }
        public List<Entity> Entities { get; protected set; }
        public uint Timer { get; private set; }

        public Camera Camera {
            get {
                return _camera;
            }

            set {
                _camera = value;
                if (_hasStarted) {
                    _camera.Start();
                }
            }
        }

        #endregion  

        #region Public Methods

        public void Add(Graphic graphic) {
            _graphicsToAdd.Add(graphic);
        }

        public void Add(IEnumerable<Graphic> graphics) {
            _graphicsToAdd.AddRange(graphics);
        }

        public void Add(params Graphic[] graphics) {
            Add((IEnumerable<Graphic>) graphics);
        }

        public void Add(Entity entity) {
            entity.OnAdded(this);
            if (_hasStarted) {
                entity.Start();
            }

            _entitiesToAdd.Add(entity);
        }
        
        public void Add(IEnumerable<Entity> entities) {
            foreach (Entity e in entities) {
                e.OnAdded(this);
                if (_hasStarted) {
                    e.Start();
                }
            }

            _entitiesToAdd.AddRange(entities);
        }

        public void Add(params Entity[] entities) {
            Add((IEnumerable<Entity>) entities);
        }

        public void Remove(Graphic graphic) {
            _graphicsToRemove.Add(graphic);
        }

        public void Remove(IEnumerable<Graphic> graphics) {
            _graphicsToRemove.AddRange(graphics);
        }

        public void Remove(params Graphic[] graphics) {
            Remove((IEnumerable<Graphic>) graphics);
        }

        public void Remove(Entity entity) {
            _entitiesToRemove.Add(entity);
        }

        public void Remove(IEnumerable<Entity> entities) {
            _entitiesToRemove.AddRange(entities);
        }

        public void Remove(params Entity[] entities) {
            Remove((IEnumerable<Entity>) entities);
        }

        public void ClearEntities() {
            _entitiesToAdd.Clear();
            _entitiesToRemove.AddRange(Entities);
        }

        public void ClearGraphics() {
            _graphicsToAdd.Clear();
            _graphicsToRemove.AddRange(Graphics);
        }

        #endregion Public Methods

        #region Public Virtual Methods

        public virtual void OnAdded() { }

        public virtual void Start() {
            _hasStarted = true;
            foreach (Entity e in Entities) {
                e.Start();
            }

            Camera.Start();
        }

        public virtual void Begin() {
            if (!_hasStarted) {
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
            // check entities Layer
            if (Entities.Count > 1) {
                int previousLayer = Entities[0].Layer;
                for (int i = 1; i < Entities.Count; i++) {
                    if (previousLayer > Entities[i].Layer) {
                        _entitiesToAdd.Add(Entities[i - 1]);
                        Entities.RemoveAt(i - 1);
                        i--;
                    }

                    previousLayer = Entities[i].Layer;
                }
            }

            if (_entitiesToAdd.Count > 0) {
                foreach (Entity e in _entitiesToAdd) {
                    int index = 0;
                    for (int i = Entities.Count - 1; i >= 0; i--) {
                        if (Entities[i].Layer <= e.Layer) {
                            index = i + 1;
                            break;
                        }
                    }

                    Entities.Insert(index, e);
                }

                _entitiesToAdd.Clear();
            }

            if (_entitiesToRemove.Count > 0) {
                foreach (Entity e in _entitiesToRemove) {
                    if (Entities.Remove(e)) {
                        e.OnRemoved.Invoke();
                    }
                }

                _entitiesToRemove.Clear();
            }

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

                    Graphics.Insert(index, g);
                }

                _graphicsToAdd.Clear();
            }

            if (_graphicsToRemove.Count > 0) {
                Graphics.RemoveAll(p => _graphicsToRemove.Contains(p));
                _graphicsToRemove.Clear();
            }

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
