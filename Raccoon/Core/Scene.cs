using System.Collections.Generic;

using Raccoon.Util.Collections;
using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Private Members

        private Camera _camera;

        /// <summary>
        /// All scene objects for quickly access.
        /// </summary>
        private Locker<ISceneObject> _sceneObjects = new Locker<ISceneObject>();

        /// <summary>
        /// Renderables sorted by IRenderable.Layer in Scene.
        /// </summary>
        private Locker<IRenderable> _renderables = new Locker<IRenderable>((IRenderable a, IRenderable b) => a.Layer.CompareTo(b.Layer));

        /// <summary>
        /// Updatables sorted by IUpdatable.Order in Scene.
        /// </summary>
        private Locker<IUpdatable> _updatables = new Locker<IUpdatable>((IUpdatable a, IUpdatable b) => a.Order.CompareTo(b.Order));

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
        /// Updatable count in this Scene.
        /// </summary>
        public int UpdatableCount { get { return _updatables.Count; } }

        /// <summary>
        /// Renderable count in this Scene.
        /// </summary>
        public int RenderableCount { get { return _renderables.Count; } }

        /// <summary>
        /// Scene Objects count in this Scene.
        /// </summary>
        public int SceneObjectsCount { get { return _sceneObjects.Count; } }

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
        /// Adds an IUpdatable, IRenderable or ISceneObject to Scene.
        /// Type can implements one or more of them.
        /// </summary>
        /// <typeparam name="T">Class based on IUpdatable, IRenderable and/or ISceneObject.</typeparam>
        /// <param name="obj">Object to add.</param>
        /// <returns>Reference to the object.</returns>
        public T Add<T>(T obj) {
            bool added = false;

            if (obj is IUpdatable updatable) {
                _updatables.Add(updatable);
                added = true;
            } 

            if (obj is IRenderable renderable) {
                _renderables.Add(renderable);
                added = true;
            } 

            if (obj is ISceneObject sceneObject) {
                _sceneObjects.Add(sceneObject);
                added = true;

                sceneObject.SceneAdded(this);
                if (HasStarted && !sceneObject.HasStarted) {
                    sceneObject.Start();
                }
            } 

            if (!added) {
                throw new System.ArgumentException("Object must be an ISceneObject, IUpdatable or IRenderable.");
            }

            return obj;
        }

        /// <summary>
        /// Adds a Graphic to the Scene.
        /// </summary>
        /// <param name="graphic">Graphic to add.</param>
        /// <returns>Reference to Graphic.</returns>
        public Graphic AddGraphic(Graphic graphic) {
            _updatables.Add(graphic);
            _renderables.Add(graphic);
            return graphic;
        }

        /// <summary>
        /// Adds a Graphic to the Scene.
        /// </summary>
        /// <typeparam name="T">Graphic based type.</typeparam>
        /// <param name="graphic">Graphic to add.</param>
        /// <returns>Reference to Graphic.</returns>
        public T AddGraphic<T>(T graphic) where T : Graphic {
            return AddGraphic(graphic as Graphic) as T;
        }

        /// <summary>
        /// Adds multiple Graphic to the Scene. 
        /// </summary>
        /// <param name="graphics">IEnumerable containing Graphic.</param>
        public void AddGraphics(IEnumerable<Graphic> graphics) {
            _updatables.AddRange(graphics);
            _renderables.AddRange(graphics);
        }

        /// <summary>
        /// Adds multiple Graphic to the Scene. 
        /// </summary>
        /// <param name="graphics">Multiple Graphic as array or variable parameters.</param>
        public void AddGraphics(params Graphic[] graphics) {
            AddGraphics((IEnumerable<Graphic>) graphics);
        }

        /// <summary>
        /// Adds an Entity to the Scene.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns>Reference to Entity.</returns>
        public Entity AddEntity(Entity entity) {
            if (entity.Scene == this) {
                return entity;
            }

            _updatables.Add(entity);
            _renderables.Add(entity);
            _sceneObjects.Add(entity);

            entity.SceneAdded(this);
            if (HasStarted && !entity.HasStarted) {
                entity.Start();
            }

            return entity;
        }

        /// <summary>
        /// Adds an Entity to the Scene.
        /// </summary>
        /// <typeparam name="T">Entity based type.</typeparam>
        /// <param name="entity">Entity to add.</param>
        /// <returns>Reference to the entity.</returns>
        public T AddEntity<T>(T entity) where T : Entity {
            return AddEntity(entity as Entity) as T;
        }
        
        /// <summary>
        /// Adds multiple Entity to the Scene.
        /// </summary>
        /// <param name="entities">IEnumerable containing Entity.</param>
        public void AddEntities(IEnumerable<Entity> entities) {
            _updatables.AddRange(entities);
            _renderables.AddRange(entities);

            foreach (Entity e in entities) {
                _sceneObjects.Add(e);
                e.SceneAdded(this);
                if (HasStarted && !e.HasStarted) {
                    e.Start();
                }
            }
        }

        /// <summary>
        /// Adds multiple Entity to the Scene.
        /// </summary>
        /// <param name="entities">Multiple Entity as array or variable parameters.</param>
        public void AddEntities(params Entity[] entities) {
            AddEntities((IEnumerable<Entity>) entities);
        }

        /// <summary>
        /// Remove an IUpdatable from Scene.
        /// </summary>
        /// <param name="updatable">IUpdatable to remove.</param>
        /// <returns>Reference to IUpdatable.</returns>
        public bool Remove(IUpdatable updatable) {
            return _updatables.Remove(updatable);
        }

        /// <summary>
        /// Remove an IRenderable from Scene.
        /// </summary>
        /// <param name="renderable">IRenderable to remove.</param>
        /// <returns>Reference to IRenderable.</returns>
        public bool Remove(IRenderable renderable) {
            return _renderables.Remove(renderable);
        }

        /// <summary>
        /// Removes an IUpdatable, IRenderable or ISceneObject from Scene.
        /// Type can implements one or more of them.
        /// </summary>
        /// <typeparam name="T">Class based on IUpdatable, IRenderable and/or ISceneObject.</typeparam>
        /// <param name="obj">Object to removed.</param>
        /// <returns>True if removed, False otherwise.</returns>
        public bool Remove<T>(T obj) {
            bool isValid = false, 
                 removed = false;

            if (obj is IUpdatable updatable) {
                isValid = true;
                removed = Remove(updatable);
            } 

            if (obj is IRenderable renderable) {
                isValid = true;
                removed = Remove(renderable);
            } 

            if (obj is ISceneObject sceneObject) {
                isValid = true;
                if (_sceneObjects.Remove(sceneObject)) {
                    sceneObject.SceneRemoved();
                    removed = true;
                }
            } 

            if (!isValid) {
                throw new System.ArgumentException("Object must be an ISceneObject, IUpdatable or IRenderable.");
            }

            return removed;
        }

        /// <summary>
        /// Removes a Graphic from Scene.
        /// </summary>
        /// <param name="graphic">Graphic to remove.</param>
        public bool RemoveGraphic(Graphic graphic) {
            _updatables.Remove(graphic);
            return _renderables.Remove(graphic);
        }

        /// <summary>
        /// Removes multiple Graphic from Scene.
        /// </summary>
        /// <param name="graphics">IEnumerable containing Graphic.</param>
        public void RemoveGraphics(IEnumerable<Graphic> graphics) {
            _updatables.RemoveRange(graphics);
            _renderables.RemoveRange(graphics);
        }

        /// <summary>
        /// Removes multiple Graphic from Scene.
        /// </summary>
        /// <param name="graphics">Multiple Graphic as array or variable parameters.</param>
        public void RemoveGraphics(params Graphic[] graphics) {
            RemoveGraphics((IEnumerable<Graphic>) graphics);
        }

        /// <summary>
        /// Removes an Entity from Scene.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public bool RemoveEntity(Entity entity) {
            _updatables.Remove(entity);
            _renderables.Remove(entity);
            if (_sceneObjects.Remove(entity)) {
                entity.SceneRemoved();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes multiple Entity from Scene.
        /// </summary>
        /// <param name="entities">IEnumerable containing Entity.</param>
        public void RemoveEntities(IEnumerable<Entity> entities) {
            foreach (Entity entity in entities) {
                _updatables.Remove(entity);
                _renderables.Remove(entity);
                if (_sceneObjects.Remove(entity)) {
                    entity.SceneRemoved();
                }
            }
        }

        /// <summary>
        /// Removes multiple Entity from the Scene.
        /// </summary>
        /// <param name="entities">Multiple Entity as array or variable parameters.</param>
        public void RemoveEntities(params Entity[] entities) {
            RemoveEntities((IEnumerable<Entity>) entities);
        }

        /// <summary>
        /// Removes multiple IUpdatables and IRenderables from Scene using a filter. 
        /// If it's an ISceneObject, removes too.
        /// </summary>
        /// <param name="filter">Filter to find Entity.</param>
        /// <returns>Removed Entity count.</returns>
        public int RemoveWhere<T>(System.Predicate<T> filter) where T : IUpdatable, IRenderable {
            List<IUpdatable> removedUpdatable = _updatables.RemoveWhere((IUpdatable u) => filter((T) u));
            List<IRenderable> removedRenderable = _renderables.RemoveWhere((IRenderable r) => filter((T) r));

            int removedCount = 0;

            foreach (IUpdatable updatable in removedUpdatable) {
                Remove(updatable);

                if (updatable is ISceneObject sceneObject) {
                    removedCount++;
                    sceneObject.SceneRemoved();
                }
            }

            foreach (IRenderable renderable in removedRenderable) {
                Remove(renderable);

                if (renderable is ISceneObject sceneObject && sceneObject.Scene == this) {
                    removedCount++;
                    sceneObject.SceneRemoved();
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Remove all IUpdatable, IRenderables and ISceneObject from Scene.
        /// </summary>
        public void Clear() {
            _updatables.Clear();
            _renderables.Clear();

            if (_sceneObjects.IsLocked) {
                foreach (ISceneObject sceneObject in _sceneObjects.ToAdd) {
                    sceneObject.SceneRemoved();
                }
            }

            foreach (ISceneObject sceneObject in _sceneObjects.Items) {
                sceneObject.SceneRemoved();
            }

            _sceneObjects.Clear();
        }

        #endregion Public Methods

        #region Public Virtual Methods

        /// <summary>
        /// Called when Scene is added to Game via Game.AddScene().
        /// </summary>
        public virtual void OnAdded() {
        }

        /// <summary>
        /// Called once to setup Scene.
        /// Graphics Context is already available here.
        /// </summary>
        public virtual void Start() {
            HasStarted = true;

            foreach (ISceneObject sceneObject in _sceneObjects) {
                if (sceneObject.HasStarted) {
                    continue;
                }

                sceneObject.Start();
            }

            Camera.Start();
        }

        /// <summary>
        /// Called every time Game switches from another Scene.
        /// </summary>
        public virtual void Begin() {
            if (!HasStarted || !Game.Instance.IsRunning) {
                Start();
            }

            IsRunning = true;
            Camera.Begin();

            foreach (ISceneObject sceneObject in _sceneObjects) {
                sceneObject.SceneBegin();
            }
        }

        /// <summary>
        /// Called every time Game switches to another Scene
        /// </summary>
        public virtual void End() {
            IsRunning = false;
            foreach (ISceneObject sceneObject in _sceneObjects) {
                sceneObject.SceneEnd();
            }

            Camera.End();
        }

        /// <summary>
        /// Called when Scene is disposed, to unload all resources.
        /// </summary>
        public virtual void UnloadContent() {
            /*foreach (Graphic g in _graphics) {
                g.Dispose();
            }

            foreach (Entity e in _entities) {
                foreach (Graphic g in e.Graphics) {
                    g.Dispose();
                }
            }*/
        }

        /// <summary>
        /// Runs before Update().
        /// Call BeforeUpdate() on every IExtendedUpdatable (including ISceneObject), sorted by Order.
        /// </summary>
        public virtual void BeforeUpdate() {
            if (!IsRunning) {
                return;
            }

            foreach (IUpdatable updatable in _updatables) {
                if (!updatable.Active) {
                    continue;
                }

                if (!(updatable is IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                if (updatable is ISceneObject sceneObject && !sceneObject.AutoUpdate) {
                    continue;
                }

                extendedUpdatable.BeforeUpdate();
            }
        }

        /// <summary>
        /// Main Scene Update. Normally all Game main logics stay here.
        /// Call Update() on every IExtendedUpdatable (including ISceneObject), sorted by Order.
        /// </summary>
        /// <param name="delta">Time difference (in milliseconds) from previous update.</param>
        public virtual void Update(int delta) {
            if (!IsRunning) {
                return;
            }

            Timer += (uint) delta;

            foreach (IUpdatable updatable in _updatables) {
                if (!updatable.Active) {
                    continue;
                }

                if (!(updatable is IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                if (updatable is ISceneObject sceneObject && !sceneObject.AutoUpdate) {
                    continue;
                }

                extendedUpdatable.Update(delta);
            }
        }

        /// <summary>
        /// Runs after Update().
        /// Call LateUpdate() on every IExtendedUpdatable (including ISceneObject), sorted by Order.
        /// </summary>
        public virtual void LateUpdate() {
            if (!IsRunning) {
                return;
            }

            foreach (IUpdatable updatable in _updatables) {
                if (!updatable.Active) {
                    continue;
                }

                if (!(updatable is IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                if (updatable is ISceneObject sceneObject && !sceneObject.AutoUpdate) {
                    continue;
                }

                extendedUpdatable.LateUpdate();
            }

            Camera.Update(Game.Instance.LastUpdateDeltaTime);
        }

        /// <summary>
        /// Render all IRenderables, sorted by Layer.
        /// </summary>
        public virtual void Render() {
            Camera.PrepareRender();

            foreach (IRenderable renderable in _renderables) {
                if (!renderable.Visible) {
                    continue;
                }

                if (renderable is ISceneObject sceneObject && !sceneObject.AutoRender) {
                    continue;
                }

                renderable.Render();
            }
        }

        /// <summary>
        /// Used to render debug informations. Collision bounds, info text, for example.
        /// Call DebugRender() on every IRenderable that implements IDebugRenderable (must be Visible).
        /// Everything rendered here doesn't suffer from Game.PixelScale factor.
        /// </summary>
        public virtual void DebugRender() {
            foreach (IRenderable renderable in _renderables) {
                if (!renderable.Visible) {
                    continue;
                }

                if (!(renderable is IDebugRenderable debugRenderable)) {
                    continue;
                }

                debugRenderable.DebugRender();
            }

            Camera.DebugRender();
        }

        #endregion
    }
}
