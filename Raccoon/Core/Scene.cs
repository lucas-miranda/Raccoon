﻿using System.Collections.Generic;

using Raccoon.Util.Collections;
using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Private Members

        private Camera _camera;

        private Dictionary<int, ControlGroup> _controlGroups = new Dictionary<int, ControlGroup>();

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
        public bool IsRunning { get; private set; }

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

                if (_camera != null && HasStarted && !_camera.HasStarted) {
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
            if (obj == null) {
                throw new System.ArgumentNullException(nameof(obj));
            }

            if (obj is Entity entity) {
                return (T) (ISceneObject) AddEntity(entity);
            } else if (obj is Graphic graphic) {
                return (T) (IRenderable) AddGraphic(graphic);
            }

            bool added = false;

            if (obj is IUpdatable updatable) {
                AddUpdatable(updatable);
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
            if (graphic == null) {
                throw new System.ArgumentNullException(nameof(graphic));
            }

            AddUpdatable(graphic);
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
            foreach (Graphic graphic in graphics) {
                AddUpdatable(graphic);
            }

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
            if (entity == null) {
                throw new System.ArgumentNullException(nameof(entity));
            }

            if (entity.Scene == this && !entity.IsSceneFromTransformAncestor) {
                return entity;
            } else if (entity.Scene != null && entity.Scene != this) {
                throw new System.InvalidOperationException($"Before adding entity to scene '{GetType().Name}', remove it from scene '{entity.Scene.GetType().Name}'.");
            }

            _renderables.Add(entity);
            AddUpdatable(entity);
            _sceneObjects.Add(entity);

            if (entity.IsSceneFromTransformAncestor) {
                // don't need to retrigger entity.SceneAdded() for this instance
                entity.IsSceneFromTransformAncestor = false;
            } else {
                entity.SceneAdded(this);
            }

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
            foreach (Entity e in entities) {
                AddEntity(e);
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
            if (updatable is IPausable pausable
             && pausable.ControlGroup != null
            ) {
                pausable.ControlGroup.Unregister(pausable);
            }

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
        /// <param name="wipe">Allow ISceneObject to be wiped after removed.</param>
        /// <returns>True if removed, False otherwise.</returns>
        public bool Remove<T>(T obj, bool wipe = true) {
            if (obj is Entity entity) {
                return RemoveEntity(entity, wipe);
            } else if (obj is Graphic graphic) {
                return RemoveGraphic(graphic);
            }

            bool isValid = false,
                 removed = false;

            if (obj is IUpdatable updatable) {
                isValid = true;
                removed = Remove(updatable);
            }

            if (obj is IRenderable renderable) {
                isValid = true;
                removed = removed || Remove(renderable);
            }

            if (obj is ISceneObject sceneObject) {
                isValid = true;
                if (_sceneObjects.Remove(sceneObject)) {
                    sceneObject.SceneRemoved(wipe);
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
        /// Even if isn't directly add to it, ie. as a child of another Transform at this Scene, in that case it'll remove it from parent Transform.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        /// <param name="wipe">Allow Entity to be wiped after removed.</param>
        public bool RemoveEntity(Entity entity, bool wipe = true) {
            Remove((IUpdatable) entity);
            Remove((IRenderable) entity);

            if (_sceneObjects.Remove(entity)) {
                entity.SceneRemoved(wipe);
                entity.IsSceneFromTransformAncestor = false;
                return true;
            } else if (entity.Scene == this && entity.IsSceneFromTransformAncestor) {
                entity.Transform.Parent = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes multiple Entity from Scene.
        /// </summary>
        /// <param name="entities">IEnumerable containing Entity.</param>
        public void RemoveEntities(IEnumerable<Entity> entities, bool wipe = true) {
            foreach (Entity entity in entities) {
                RemoveEntity(entity, wipe);
            }
        }

        /// <summary>
        /// Removes multiple Entity from the Scene.
        /// </summary>
        /// <param name="entities">Multiple Entity as array or variable parameters.</param>
        public void RemoveEntities(bool wipe, params Entity[] entities) {
            RemoveEntities((IEnumerable<Entity>) entities, wipe);
        }

        /// <summary>
        /// Removes multiple IUpdatables and IRenderables from Scene using a filter.
        /// If it's an ISceneObject, remove it too.
        /// </summary>
        /// <param name="filter">Filter to find Entity.</param>
        /// <param name="wipe">Allow ISceneObject to be wiped after removed.</param>
        /// <returns>Removed Entity count.</returns>
        public int RemoveWhere<T>(System.Predicate<T> filter, bool wipe = true) where T : IUpdatable, IRenderable {
            List<IUpdatable> removedUpdatable = _updatables.RemoveWhere((IUpdatable u) => filter((T) u));
            List<IRenderable> removedRenderable = _renderables.RemoveWhere((IRenderable r) => filter((T) r));

            int removedCount = 0;

            foreach (IUpdatable updatable in removedUpdatable) {
                Remove(updatable);

                if (updatable is ISceneObject sceneObject && sceneObject.Scene == this) {
                    removedCount++;
                    sceneObject.SceneRemoved(wipe);
                }
            }

            foreach (IRenderable renderable in removedRenderable) {
                Remove(renderable);

                if (renderable is ISceneObject sceneObject && sceneObject.Scene == this) {
                    removedCount++;
                    sceneObject.SceneRemoved(wipe);
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Remove all IUpdatable, IRenderables and ISceneObject from Scene.
        /// </summary>
        /// <param name="wipe">Allow ISceneObject to be wiped after removed.</param>
        public void Clear(bool wipe = true) {
            foreach (IUpdatable updatable in _updatables) {
                if (updatable is IPausable pausable
                 && pausable.ControlGroup != null
                ) {
                    pausable.ControlGroup.Unregister(pausable);
                }
            }
            _updatables.Clear();

            _renderables.Clear();

            foreach (ISceneObject sceneObject in _sceneObjects) {
                sceneObject.SceneRemoved(wipe);
            }
            _sceneObjects.Clear();

            foreach (ControlGroup controlGroup in _controlGroups.Values) {
                controlGroup.Clear();
            }
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

            _sceneObjects.Lock();
            foreach (ISceneObject sceneObject in _sceneObjects) {
                if (sceneObject.HasStarted) {
                    continue;
                }

                sceneObject.Start();
            }
            _sceneObjects.Unlock();

            if (Camera == null) {
                Camera = Camera.Default;
            }
        }

        /// <summary>
        /// Called every time Game switches from another Scene.
        /// </summary>
        public virtual void Begin() {
            if (!HasStarted || !Game.Instance.IsRunning) {
                Start();
            }

            IsRunning = true;
            Camera?.SceneBegin(this);

            _sceneObjects.Lock();
            foreach (ISceneObject sceneObject in _sceneObjects) {
                sceneObject.SceneBegin();
            }
            _sceneObjects.Unlock();
        }

        /// <summary>
        /// Called every time Game switches to another Scene
        /// </summary>
        public virtual void End() {
            IsRunning = false;

            _sceneObjects.Lock();
            foreach (ISceneObject sceneObject in _sceneObjects) {
                sceneObject.SceneEnd();
            }
            _sceneObjects.Unlock();

            Camera?.SceneEnd(this);
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

            _updatables.Lock();
            foreach (IUpdatable updatable in _updatables) {
                if (!CanUpdate(updatable, out IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                extendedUpdatable.BeforeUpdate();
            }
            _updatables.Unlock();
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

            _updatables.Lock();
            foreach (IUpdatable updatable in _updatables) {
                if (!CanUpdate(updatable, out IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                extendedUpdatable.Update(delta);
            }
            _updatables.Unlock();
        }

        /// <summary>
        /// Runs after Update().
        /// Call LateUpdate() on every IExtendedUpdatable (including ISceneObject), sorted by Order.
        /// </summary>
        public virtual void LateUpdate() {
            if (!IsRunning) {
                return;
            }

            _updatables.Lock();
            foreach (IUpdatable updatable in _updatables) {
                if (!CanUpdate(updatable, out IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                extendedUpdatable.LateUpdate();
            }
            _updatables.Unlock();

            Camera?.Update(Game.Instance.UpdateDeltaTime);
        }

        public virtual void BeforePhysicsStep() {
            if (!IsRunning) {
                return;
            }

            _updatables.Lock();
            foreach (IUpdatable updatable in _updatables) {
                if (!CanUpdate(updatable, out IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                extendedUpdatable.BeforePhysicsStep();
            }
            _updatables.Unlock();
        }

        public virtual void PhysicsStep(float stepDelta) {
            if (!IsRunning) {
                return;
            }

            _updatables.Lock();
            foreach (IUpdatable updatable in _updatables) {
                if (!CanUpdate(updatable, out IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                extendedUpdatable.PhysicsStep(stepDelta);
            }
            _updatables.Unlock();
        }

        public virtual void LatePhysicsStep() {
            if (!IsRunning) {
                return;
            }

            _updatables.Lock();
            foreach (IUpdatable updatable in _updatables) {
                if (!CanUpdate(updatable, out IExtendedUpdatable extendedUpdatable)) {
                    continue;
                }

                extendedUpdatable.LatePhysicsStep();
            }
            _updatables.Unlock();
        }

        /// <summary>
        /// Render all IRenderables, sorted by Layer.
        /// </summary>
        public virtual void Render() {
            Camera?.PrepareRender();

            _renderables.Lock();
            foreach (IRenderable renderable in _renderables) {
                if (!renderable.Visible
                  || (renderable is ISceneObject sceneObject && !sceneObject.AutoRender)) {
                    continue;
                }

                renderable.Render();
            }
            _renderables.Unlock();
        }

#if DEBUG

        /// <summary>
        /// Used to render debug informations. Collision bounds, info text, for example.
        /// Call DebugRender() on every IRenderable that implements IDebugRenderable (must be Visible).
        /// Everything rendered here doesn't suffer from Game.PixelScale factor.
        /// </summary>
        public virtual void DebugRender() {
            _renderables.Lock();
            foreach (IRenderable renderable in _renderables) {
                if (!renderable.Visible
                  || !(renderable is IDebugRenderable debugRenderable)) {
                    continue;
                }

                debugRenderable.DebugRender();
            }
            _renderables.Unlock();

            Camera?.DebugRender();
        }

#endif

        public ControlGroup ControlGroup(int index) {
            return _controlGroups[index];
        }

        public ControlGroup ControlGroup(System.Enum index) {
            return ControlGroup(System.Convert.ToInt32(index));
        }

        public void RegisterControlGroup(int index, ControlGroup controlGroup) {
            if (controlGroup == null) {
                throw new System.ArgumentNullException(nameof(controlGroup));
            }

            _controlGroups.Add(index, controlGroup);
        }

        public void RegisterControlGroup(System.Enum index, ControlGroup controlGroup) {
            RegisterControlGroup(System.Convert.ToInt32(index), controlGroup);
        }

        /*
        public void PauseControlGroup(int controlGroup) {
            _pausedGroups.Add(controlGroup);
            foreach (IUpdatable updatable in _updatables) {
                if (updatable.ControlGroup != controlGroup) {
                    continue;
                }

                updatable.Active = false;
            }
        }

        public void ResumeControlGroup(int controlGroup) {
            if (!_pausedGroups.Remove(controlGroup)) {
                return;
            }

            foreach (IUpdatable updatable in _updatables) {
                if (updatable.ControlGroup != controlGroup) {
                    continue;
                }

                updatable.Active = true;
            }
        }
        */

        public void PauseAll() {
            foreach (ControlGroup controlGroup in _controlGroups.Values) {
                controlGroup.Pause();
            }
        }

        public void ResumeAll() {
            foreach (ControlGroup controlGroup in _controlGroups.Values) {
                controlGroup.Resume();
            }
        }

        public void ResumeAllExcept(int index) {
            foreach (KeyValuePair<int, ControlGroup> entry in _controlGroups) {
                if (entry.Key == index) {
                    continue;
                }

                entry.Value.Resume();
            }
        }

        public void ResumeAllExcept(System.Enum index) {
            ResumeAllExcept(System.Convert.ToInt32(index));
        }

        /*

        public bool IsControlGroupPaused(int controlGroup) {
            return _pausedGroups.Contains(controlGroup);
        }
        */

        public void Pause() {
            if (!HasStarted) {
                return;
            }

            IsRunning = false;
        }

        public void Resume() {
            if (!HasStarted) {
                return;
            }

            IsRunning = true;
        }

        #endregion

        #region Private Methods

        private void AddUpdatable(IUpdatable updatable) {
            _updatables.Add(updatable);
        }

        private bool CanUpdate(IUpdatable updatable, out IExtendedUpdatable extendedUpdatable) {
            if (!(!updatable.Active
             || (updatable.ControlGroup != null && updatable.ControlGroup.IsPaused)
             || !(updatable is IExtendedUpdatable e)
             || (updatable is ISceneObject sceneObject && !sceneObject.AutoUpdate)
            )) {
                extendedUpdatable = e;
                return true;
            }

            extendedUpdatable = null;
            return false;
        }

        #endregion Private Methods
    }
}
