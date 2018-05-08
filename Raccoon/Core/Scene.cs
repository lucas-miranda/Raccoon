using System.Collections.Generic;

using Raccoon.Util.Collections;
using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Private Members

        private Camera _camera;


        /// <summary>
        /// Graphics sorted by Graphic.Layer in Scene.
        /// </summary>
        private Locker<Graphic> _graphics = new Locker<Graphic>(Graphic.LayerComparer);

        /// <summary>
        /// Entities sorted by Entity.Layer in Scene.
        /// </summary>
        private Locker<Entity> _entities = new Locker<Entity>(Entity.LayerComparer);

        #endregion Private Members

        #region Constructor

        /// <summary>
        /// Create a Scene.
        /// Must be added via Game.AddScene()
        /// </summary>
        public Scene() {
            Camera = new Camera();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Entity count in this Scene.
        /// </summary>
        public int EntitiesCount { get { return _entities.Count; } }

        /// <summary>
        /// Graphic count in this Scene.
        /// </summary>
        public int GraphicsCount { get { return _graphics.Count; } }

        /// <summary>
        /// Current Scene Time (in milliseconds), increments in every Update() when Scene is running.
        /// </summary>
        public uint Timer { get; private set; }

        /// <summary>
        /// If Scene has already started. 
        /// A way to know if Start() has already been called, useful to safely use Graphics Context when it's properly ready.
        /// </summary>
        public bool HasStarted { get; private set; } 

        /// <summary>
        /// If Scene is updating. 
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Camera instance. 
        /// Can be changed for a custom Camera.
        /// </summary>
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

        /// <summary>
        /// Add a Graphic to the Scene.
        /// </summary>
        /// <param name="graphic">The graphic to be added.</param>
        /// <returns>The added Graphic.</returns>
        public Graphic AddGraphic(Graphic graphic) {
            _graphics.Add(graphic);
            return graphic;
        }

        public T AddGraphic<T>(T graphic) where T : Graphic {
            return AddGraphic(graphic as Graphic) as T;
        }

        /// <summary>
        /// Add multiple graphics to the Scene. 
        /// </summary>
        /// <param name="graphics">The IEnumerable containing graphics.</param>
        public void AddGraphics(IEnumerable<Graphic> graphics) {
            _graphics.AddRange(graphics);
        }

        /// <summary>
        /// Add multiple graphics to the Scene. 
        /// </summary>
        /// <param name="graphics">Graphics as variable number of arguments.</param>
        public void AddGraphics(params Graphic[] graphics) {
            AddGraphics((IEnumerable<Graphic>) graphics);
        }

        /// <summary>
        /// Add an Entity to the Scene.
        /// </summary>
        /// <param name="entity">The Entity to be added.</param>
        /// <returns>The added Entity.</returns>
        public Entity AddEntity(Entity entity) {
            _entities.Add(entity);
            entity.SceneAdded(this);
            if (HasStarted && !entity.HasStarted) {
                entity.Start();
            }

            return entity;
        }

        public T AddEntity<T>(T entity) where T : Entity {
            return AddEntity(entity as Entity) as T;
        }
        
        /// <summary>
        /// Add multiple entities to the Scene.
        /// </summary>
        /// <param name="entities">The IEnumerable containing entities.</param>
        public void AddEntities(IEnumerable<Entity> entities) {
            _entities.AddRange(entities);
            foreach (Entity e in entities) {
                e.SceneAdded(this);
                if (HasStarted && !e.HasStarted) {
                    e.Start();
                }
            }
        }

        /// <summary>
        /// Add multiple entities to the Scene.
        /// </summary>
        /// <param name="entities">Entities as variable number of arguments.</param>
        public void AddEntities(params Entity[] entities) {
            AddEntities((IEnumerable<Entity>) entities);
        }

        /// <summary>
        /// Remove a Graphic from Scene.
        /// </summary>
        /// <param name="graphic">The Graphic to be removed.</param>
        public void RemoveGraphic(Graphic graphic) {
            _graphics.Remove(graphic);
        }

        /// <summary>
        /// Remove multiple graphics from the Scene.
        /// </summary>
        /// <param name="graphics">The IEnumerable containing graphics.</param>
        public void RemoveGraphics(IEnumerable<Graphic> graphics) {
            _graphics.RemoveRange(graphics);
        }

        /// <summary>
        /// Remove multiple graphics from the Scene.
        /// </summary>
        /// <param name="graphics">Graphics as variable number of arguments.</param>
        public void RemoveGraphics(params Graphic[] graphics) {
            RemoveGraphics((IEnumerable<Graphic>) graphics);
        }

        /// <summary>
        /// Remove an Entity from the Scene.
        /// </summary>
        /// <param name="entity">The Entity to be removed.</param>
        public void RemoveEntity(Entity entity) {
            if (_entities.Remove(entity)) {
                entity.SceneRemoved();
            }
        }

        /// <summary>
        /// Remove multiple entities from the Scene.
        /// </summary>
        /// <param name="entities">The IEnumerable containing entities.</param>
        public void RemoveEntities(IEnumerable<Entity> entities) {
            foreach (Entity entity in entities) {
                if (_entities.Remove(entity)) {
                    entity.SceneRemoved();
                }
            }
        }

        /// <summary>
        /// Remove multiple entities from the Scene.
        /// </summary>
        /// <param name="entities">Entities as variable number of arguments.</param>
        public void RemoveEntities(params Entity[] entities) {
            RemoveEntities((IEnumerable<Entity>) entities);
        }

        /// <summary>
        /// Remove multiple Entity using a System.Predicate.
        /// </summary>
        /// <param name="match">A filter to find entities.</param>
        /// <returns>How many entities were removed.</returns>
        public int RemoveEntitiesWhere(System.Predicate<Entity> match) {
            List<Entity> removed = _entities.RemoveWhere(match);
            foreach (Entity entity in removed) {
                entity.OnSceneRemoved();
            }

            return removed.Count;
        }

        /// <summary>
        /// Remove all entities from the Scene.
        /// </summary>
        public void ClearEntities() {
            if (_entities.IsLocked) {
                foreach (Entity entity in _entities.ToAdd) {
                    entity.SceneRemoved();
                }

                foreach (Entity entity in _entities.ToRemove) {
                    entity.SceneRemoved();
                }
            }

            foreach (Entity entity in _entities.Items) {
                entity.SceneRemoved();
            }

            _entities.Clear();
        }

        /// <summary>
        /// Remove all graphics from the Scene.
        /// </summary>
        public void ClearGraphics() {
            _graphics.Clear();
        }

        #endregion Public Methods

        #region Public Virtual Methods

        /// <summary>
        /// Called when Scene is added to the Game via Game.AddScene().
        /// </summary>
        public virtual void OnAdded() { }

        /// <summary>
        /// Called once to setup Scene.
        /// Graphics Context is already available from here.
        /// </summary>
        public virtual void Start() {
            HasStarted = true;
            foreach (Entity e in _entities) {
                if (e.HasStarted) {
                    continue;
                }

                e.Start();
            }

            Camera.Start();
        }

        /// <summary>
        /// Called every time Game switches to Scene.
        /// </summary>
        public virtual void Begin() {
            if (!HasStarted) {
                Start();
            }

            IsRunning = true;
            Camera.Begin();

            foreach (Entity e in _entities) {
                e.SceneBegin();
            }
        }

        /// <summary>
        /// Called every time Game switches to another Scene
        /// </summary>
        public virtual void End() {
            IsRunning = false;
            foreach (Entity e in _entities) {
                e.SceneEnd();
            }

            Camera.End();
        }

        /// <summary>
        /// Called when Scene is disposed to unload all resources.
        /// </summary>
        public virtual void UnloadContent() {
            foreach (Graphic g in _graphics) {
                g.Dispose();
            }

            foreach (Entity e in _entities) {
                foreach (Graphic g in e.Graphics) {
                    g.Dispose();
                }
            }
        }

        /// <summary>
        /// Runs before Update().
        /// </summary>
        public virtual void BeforeUpdate() {
            if (!IsRunning) {
                return;
            }

            foreach (Entity e in _entities) {
                if (!e.Active || !e.AutoUpdate) {
                    continue;
                }

                e.BeforeUpdate();
            }
        }

        /// <summary>
        /// Main Scene Update. Normally all Game main logics stay here.
        /// </summary>
        /// <param name="delta">Time difference (in milliseconds) from previous update.</param>
        public virtual void Update(int delta) {
            if (!IsRunning) {
                return;
            }

            Timer += (uint) delta;

            foreach (Graphic g in _graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Update(delta);
            }

            foreach (Entity e in _entities) {
                if (!e.Active || !e.AutoUpdate) {
                    continue;
                }

                e.Update(delta);
            }
        }

        /// <summary>
        /// Runs after Update().
        /// </summary>
        public virtual void LateUpdate() {
            if (!IsRunning) {
                return;
            }

            foreach (Entity e in _entities) {
                if (!e.Active || !e.AutoUpdate) {
                    continue;
                }

                e.LateUpdate();
            }

            Camera.Update(Game.Instance.DeltaTime);
        }

        /// <summary>
        /// Render Graphics and Entities. (In this specific order)
        /// </summary>
        public virtual void Render() {
            Camera.PrepareRender();

            foreach (Graphic g in _graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Render();
            }

            foreach (Entity e in _entities) {
                if (!e.Visible || !e.AutoRender) {
                    continue;
                }

                e.Render();
            }
        }

        /// <summary>
        /// Render Debug informations. Collision bounds, for example.
        /// Everything rendered here doesn't suffer from Game.Scale factor.
        /// </summary>
        public virtual void DebugRender() {
            foreach (Graphic g in _graphics) {
                if (!g.Visible || g.IgnoreDebugRender) {
                    continue;
                }

                g.DebugRender();
            }

            foreach (Entity e in _entities) {
                if (!e.Visible || !e.AutoRender || e.IgnoreDebugRender) {
                    continue;
                }

                e.DebugRender();
            }

            Camera.DebugRender();
        }

        #endregion
    }
}
