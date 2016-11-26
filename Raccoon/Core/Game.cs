using System;
using System.Collections.Generic;

namespace Raccoon {
    public class Game : IDisposable {
        #region Public Delegates

        public delegate void TickHandler(int delta);

        #endregion Public Delegates

        #region Public Events

        public event TickHandler OnUpdate;
        public event Action OnRender;

        #endregion Public Events

        #region Private Members

        private bool _debugMode;

        #endregion Private Members

        #region Constructor

        public Game(string title = "Raccoon Game", int width = 800, int height = 600, int targetFPS = 60, bool fullscreen = false, bool vsync = false) {
            Instance = this;
            Console.Title = "Raccoon Debug";
            Scenes = new Dictionary<string, Scene>();
            ScreenWidth = width;
            ScreenHeight = height;
            IsRunning = false;
            Core = new Core(title, width, height, targetFPS, fullscreen, vsync);
            Core.OnUpdate += Update;
            Core.OnRender += Render;
            Scenes = new Dictionary<string, Scene>();
            ScreenWidth = width;
            ScreenHeight = height;
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
        public bool VSync { get { return Core.Graphics.SynchronizeWithVerticalRetrace; } set { Core.Graphics.SynchronizeWithVerticalRetrace = value; } }
        public bool IsFullscreen { get { return Core.Graphics.IsFullScreen; } set { Core.Graphics.IsFullScreen = value; } }
        public bool IsMouseVisible { get { return Core.IsMouseVisible; } set { Core.IsMouseVisible = value; } }
        public string Title { get { return Core.Title; } set { Core.Title = value; } }
        public string ContentDirectory { get { return Core.Content.RootDirectory; } set { Core.Content.RootDirectory = value; } }
        public int DeltaTime { get { return Core.DeltaTime; } }
        public int X { get { return Core.Window.Position.X; } }
        public int Y { get { return Core.Window.Position.Y; } }
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        public int WindowWidth { get { return Core.Graphics.PreferredBackBufferWidth; } }
        public int WindowHeight { get { return Core.Graphics.PreferredBackBufferHeight; } }
        public Size ScreenSize { get { return new Size(ScreenWidth, ScreenHeight); } }
        public Size WindowSize { get { return new Size(WindowWidth, WindowHeight); } }
        public Dictionary<string, Scene> Scenes { get; private set; }
        public Scene Scene { get; private set; }

        public float Scale {
            get {
                return Core.Scale;
            }

            set {
                if (IsRunning) {
                    return;
                }

                Core.Scale = value;
                ScreenWidth = (int) Math.Ceiling(WindowWidth / Scale);
                ScreenHeight = (int) Math.Ceiling(WindowHeight / Scale);
            }
        }

#if DEBUG
        public bool DebugMode { get { return _debugMode; } set { _debugMode = value; } }
#else
        public bool DebugMode { get { return false; } }
#endif

        #endregion

        #region Internal Properties

        internal Core Core { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        public void Start() {
            Debug.WriteLine("| Raccoon started |");
            IsRunning = true;
            Core.Run();
        }

        public void Start(string startScene) {
            if (!Scenes.ContainsKey(startScene)) {
                throw new KeyNotFoundException($"Scene '{startScene}' not found");
            }

            SwitchScene(startScene);
            Start();
        }

        public void Start<T>() where T : Scene {
            Start(typeof(T).Name.Replace("Scene", ""));
        }

        public void Exit() {
            Debug.WriteLine("Exiting... ");
            IsRunning = false;
            Core.Exit();
        }

        public void Dispose() {
            Dispose(true);
        }

        public void AddScene(Scene scene, string name = "") {
            name = !name.IsEmpty() ? name : scene.GetType().Name.Replace("Scene", "");
            Scenes.Add(name, scene);
            scene.OnAdded();

            if (Scene == null) {
                SwitchScene(name);
            }
        }

        public void RemoveScene(string name) {
            Scene scene;
            if (!Scenes.TryGetValue(name, out scene)) {
                return;
            }

            if (scene == Scene) {
                Scene.End();
                Scene.UnloadContent();
                Scene = null;
            }

            Scenes.Remove(name);
        }

        public void RemoveScene(Scene scene) {
            string name = "";
            foreach (KeyValuePair<string, Scene> s in Scenes) {
                if (s.Value == scene) {
                    name = s.Key;
                    break;
                }
            }

            if (!name.IsEmpty()) {
                RemoveScene(name);
            }
        }

        public void RemoveScene<T>() where T : Scene {
            RemoveScene(typeof(T).Name.Replace("Scene", ""));
        }

        public void ClearScenes() {
            if (Scene != null) {
                Scene.End();
                Scene.UnloadContent();
                Scene = null;
            }

            foreach (Scene scene in Scenes.Values) {
                scene.UnloadContent();
            }

            Scenes.Clear();
        }

        public void SwitchScene(string name) {
            if (!Scenes.ContainsKey(name)) {
                throw new KeyNotFoundException($"Scene '{name}' not found");
            }

            if (Scene != null) {
                Scene.End();
            }

            Core.ClearCallbacks();
            Core.OnUpdate += Update;
            Core.OnRender += Render;

            Scene = Scenes[name];
            if (Core.SpriteBatch != null) {
                Scene.Begin();
            } else {
                Core.OnBegin += Scene.Begin;
            }

            Core.OnUnloadContent += Scene.UnloadContent;
            Core.OnBeforeUpdate += Scene.BeforeUpdate;
            Core.OnUpdate += Scene.Update;
            Core.OnLateUpdate += Scene.LateUpdate;
            Core.OnRender += Scene.Render;

#if DEBUG
            Core.OnDebugRender += Scene.DebugRender;
#endif
        }

        public void SwitchScene<T>() where T : Scene {
            SwitchScene(typeof(T).Name.Replace("Scene", ""));
        }

        public void ToggleFullscreen() {
            Core.Graphics.ToggleFullScreen();
        }

#endregion

        #region Protected Methods

        protected void Update(int delta) {
            OnUpdate?.Invoke(delta);
        }

        protected void Render() {
            OnRender?.Invoke();
        }

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
