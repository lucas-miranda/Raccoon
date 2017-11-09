using System.Collections.Generic;

using Raccoon.Util.Collections;
using Raccoon.Graphics;

namespace Raccoon {
    public class Scene {
        #region Private Members

        private Camera _camera;

        #endregion Private Members

        #region Constructor

        /// <summary>
        /// Create a Scene.
        /// Must be added via Game.AddScene()
        /// </summary>
        public Scene() {
            Graphics = new Locker<Graphic>(new Graphic.LayerComparer());
            Entities = new Locker<Entity>(new Entity.LayerComparer());
            Entities.OnAdded += (Entity e) => e.SceneAdded(this);
            Entities.OnRemoved += (Entity e) => e.SceneRemoved();

            Camera = new Camera();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Graphics sorted by Graphic.Layer in Scene.
        /// </summary>
        public Locker<Graphic> Graphics { get; protected set; }

        /// <summary>
        /// Entities sorted by Entity.Layer in Scene.
        /// </summary>
        public Locker<Entity> Entities { get; protected set; }

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
        public void Add(Graphic graphic) {
            Graphics.Add(graphic);
        }

        /// <summary>
        /// Add multiple graphics to the Scene. 
        /// </summary>
        /// <param name="graphics">The IEnumerable containing graphics.</param>
        public void Add(IEnumerable<Graphic> graphics) {
            Graphics.AddRange(graphics);
        }

        /// <summary>
        /// Add multiple graphics to the Scene. 
        /// </summary>
        /// <param name="graphics">Graphics as variable number of arguments.</param>
        public void Add(params Graphic[] graphics) {
            Add((IEnumerable<Graphic>) graphics);
        }

        /// <summary>
        /// Add an Entity to the Scene.
        /// </summary>
        /// <param name="entity">The Entity to be added.</param>
        public void Add(Entity entity) {
            Entities.Add(entity);
            if (HasStarted) {
                entity.Start();
            }
        }
        
        /// <summary>
        /// Add multiple entities to the Scene.
        /// </summary>
        /// <param name="entities">The IEnumerable containing entities.</param>
        public void Add(IEnumerable<Entity> entities) {
            Entities.AddRange(entities);
            foreach (Entity e in entities) {
                if (HasStarted) {
                    e.Start();
                }
            }
        }

        /// <summary>
        /// Add multiple entities to the Scene.
        /// </summary>
        /// <param name="entities">Entities as variable number of arguments.</param>
        public void Add(params Entity[] entities) {
            Add((IEnumerable<Entity>) entities);
        }

        /// <summary>
        /// Remove a Graphic from Scene.
        /// </summary>
        /// <param name="graphic">The Graphic to be removed.</param>
        public void Remove(Graphic graphic) {
            Graphics.Remove(graphic);
        }

        /// <summary>
        /// Remove multiple graphics from the Scene.
        /// </summary>
        /// <param name="graphics">The IEnumerable containing graphics.</param>
        public void Remove(IEnumerable<Graphic> graphics) {
            Graphics.RemoveRange(graphics);
        }

        /// <summary>
        /// Remove multiple graphics from the Scene.
        /// </summary>
        /// <param name="graphics">Graphics as variable number of arguments.</param>
        public void Remove(params Graphic[] graphics) {
            Remove((IEnumerable<Graphic>) graphics);
        }

        /// <summary>
        /// Remove an Entity from the Scene.
        /// </summary>
        /// <param name="entity">The Entity to be removed.</param>
        public void Remove(Entity entity) {
            Entities.Remove(entity);
        }

        /// <summary>
        /// Remove multiple entities from the Scene.
        /// </summary>
        /// <param name="entities">The IEnumerable containing entities.</param>
        public void Remove(IEnumerable<Entity> entities) {
            Entities.RemoveRange(entities);
        }

        /// <summary>
        /// Remove multiple entities from the Scene.
        /// </summary>
        /// <param name="entities">Entities as variable number of arguments.</param>
        public void Remove(params Entity[] entities) {
            Remove((IEnumerable<Entity>) entities);
        }

        /// <summary>
        /// Remove all entities from the Scene.
        /// </summary>
        public void ClearEntities() {
            Entities.Clear();
        }

        /// <summary>
        /// Remove all graphics from the Scene.
        /// </summary>
        public void ClearGraphics() {
            Graphics.Clear();
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
            foreach (Entity e in Entities) {
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

            Camera.Begin();

            foreach (Entity e in Entities) {
                e.SceneBegin();
            }
        }

        /// <summary>
        /// Called every time Game switches to another Scene
        /// </summary>
        public virtual void End() {
            foreach (Entity e in Entities) {
                e.SceneEnd();
            }

            Camera.End();
        }

        /// <summary>
        /// Called when Scene is disposed to unload all resources.
        /// </summary>
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

        /// <summary>
        /// Runs before Update().
        /// </summary>
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

        /// <summary>
        /// Main Scene Update. Normally all Game main logics stay here.
        /// </summary>
        /// <param name="delta"></param>
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


        /// <summary>
        /// Runs after Update().
        /// </summary>
        public virtual void LateUpdate() {
            foreach (Entity e in Entities) {
                if (!e.Active || !e.AutoUpdate) {
                    continue;
                }

                e.LateUpdate();
            }
        }

        /// <summary>
        /// Render Graphics and Entities. (In this specific order)
        /// </summary>
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

        /// <summary>
        /// Render Debug informations. Collision bounds, for example.
        /// Everything rendered here doesn't suffer from Game.Scale factor.
        /// </summary>
        public virtual void DebugRender() {
            foreach (Graphic g in Graphics) {
                if (!g.Visible || g.IgnoreDebugRender) {
                    continue;
                }

                g.DebugRender();
            }

            foreach (Entity e in Entities) {
                if (!e.Visible || !e.AutoRender || e.IgnoreDebugRender) {
                    continue;
                }

                e.DebugRender();
            }

            Camera.DebugRender();

            Debug.DrawString(Camera, new Vector2(Game.Instance.WindowWidth - 250, 100), $"Entities: {Entities}\nGraphics: {Graphics}");
        }

        #endregion
    }
}
