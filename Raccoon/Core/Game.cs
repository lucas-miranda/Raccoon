using Raccoon.Graphics;
using System;
using System.Collections.Generic;

namespace Raccoon {
    public class Game : IDisposable {
        #region Constructor

        public Game(string title = "Raccoon2D Game", int width = 800, int height = 600, int targetFPS = 60, bool fullscreen = false) {
            Core = new Core(title, width, height, targetFPS, fullscreen);
            Scenes = new Dictionary<string, Scene>();
            Width = width;
            Height = height;
            IsRunning = false;
            Instance = this;
        }

        ~Game() {
            Dispose(false);
        }

        #endregion

        #region Static Public Properties

        public static Game Instance { get; private set; }

        #endregion Static Public Properties

        #region Public Properties

        public bool Disposed { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsFixedTimeStep { get { return Core.IsFixedTimeStep; } }
        public string ContentDirectory { get { return Core.Content.RootDirectory; } set { Core.Content.RootDirectory = value; } }
        public int DeltaTime { get { return Core.DeltaTime; } }
        public int X { get { return Core.Window.Position.X; } }
        public int Y { get { return Core.Window.Position.Y; } }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int ScreenWidth { get { return Core.Graphics.PreferredBackBufferWidth; } }
        public int ScreenHeight { get { return Core.Graphics.PreferredBackBufferHeight; } }
        public string Title { get { return Core.Title; } set { Core.Title = value; } }
        public Dictionary<string, Scene> Scenes { get; private set; }
        public Scene Scene { get; set; }

        public float Scale {
            get {
                return Core.Scale;
            }

            set {
                Core.Scale = value;
                Width = (int) Math.Ceil(ScreenWidth / Scale);
                Height = (int) Math.Ceil(ScreenHeight / Scale);
            }
        }

        #endregion

        #region Internal Properties

        internal Core Core { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        public void Start(string startScene) {
            Debug.WriteLine("Raccoon started!");
            IsRunning = true;
            Scene = Scenes[startScene];
            Core.Run();
        }

        public void Exit() {
            Debug.WriteLine("Exiting... ");
            Core.Exit();
            IsRunning = false;
        }

        public void Dispose() {
            Dispose(true);
        }

        public void AddScene(Scene scene, string customName = "") {
            Scenes.Add((customName.Length > 0 ? customName : scene.GetType().Name), scene);
            scene.Added();
            Core.OnInitialize += new Core.GeneralHandler(scene.Initialize);
            Core.OnLoadContent += new Core.GeneralHandler(scene.LoadContent);
            Core.OnUnloadContent += new Core.GeneralHandler(scene.UnloadContent);
            Core.OnRender += new Core.GeneralHandler(scene.Render);
            Core.OnDebugRender += new Core.GeneralHandler(scene.DebugRender);
            Core.OnUpdate += new Core.TickHandler(scene.Update);
        }
        
        public void SwitchToScene(string sceneName) {
            Scene = Scenes[sceneName];
        }

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                if (disposing) {
                    Core.Dispose();
                }

                IsRunning = false;
                Disposed = true;
            }
        }

        #endregion
    }
}
