using Raccoon.Graphics;
using System;
using System.Collections.Generic;

namespace Raccoon {
    public class Game : IDisposable {
        #region Public Delegates

        public delegate void TickHandler(int delta);

        #endregion Public Delegates

        #region Public Events

        public event TickHandler OnUpdate = delegate { };
        public event Action OnRender = delegate { }, OnBegin = delegate { };

        #endregion Public Events

        #region Private Members

        private Dictionary<string, Scene> _scenes = new Dictionary<string, Scene>();

        #endregion Private Members

        #region Constructor

        public Game(string title = "Raccoon Game", int width = 800, int height = 600, int targetFPS = 60, bool fullscreen = false, bool vsync = false) {
            Instance = this;

#if DEBUG
            try {
                System.Console.Title = "Raccoon Debug";
            } catch { }
#endif

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs args) => {
                Exception e = (Exception) args.ExceptionObject;
                Debug.Log("crash-report", $"[Unhandled Exception] {e.Message}\n{e.StackTrace}\n");
            };

            Core = new Core(title, width, height, targetFPS, fullscreen, vsync);
            ScreenSize = new Size(width, height);
            ScreenCenter = (ScreenSize / 2f).ToVector2();
            WindowSize = new Size(Core.Graphics.PreferredBackBufferWidth, Core.Graphics.PreferredBackBufferHeight);
            WindowCenter = (WindowSize / 2f).ToVector2();
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
        public bool AllowResize { get { return Core.Window.AllowUserResizing; } set { Core.Window.AllowUserResizing = value; } }
        public bool HasFocus { get { return Core.IsActive; } }
        public string Title { get { return Core.Title; } set { Core.Title = value; } }
        public string ContentDirectory { get { return Core.Content.RootDirectory; } set { Core.Content.RootDirectory = value; } }
        public int DeltaTime { get { return Core.DeltaTime; } }
        public int X { get { return Core.Window.Position.X; } }
        public int Y { get { return Core.Window.Position.Y; } }
        public int ScreenWidth { get { return (int) ScreenSize.Width; } }
        public int ScreenHeight { get { return (int) ScreenSize.Height; } }
        public int WindowWidth { get { return (int) WindowSize.Width; } }
        public int WindowHeight { get { return (int) WindowSize.Height; } }
        public Size ScreenSize { get; private set; }
        public Size WindowSize { get; private set; }
        public Vector2 ScreenCenter { get; private set; }
        public Vector2 WindowCenter { get; private set; }
        public Scene Scene { get; private set; }
        public Font StdFont { get { return Core.StdFont; } }
        public Color BackgroundColor { get { return new Color(Core.BackgroundColor.R, Core.BackgroundColor.G, Core.BackgroundColor.B, Core.BackgroundColor.A); } set { Core.BackgroundColor = new Microsoft.Xna.Framework.Color(value.R, value.G, value.B, value.A); } }
        public Surface MainSurface { get { return Core.MainSurface; } }
        public Surface DebugSurface { get { return Core.DebugSurface; } }
        public Canvas MainCanvas { get { return Core.MainCanvas; } }

        public float Scale {
            get {
                return Core.Scale;
            }

            set {
                if (IsRunning) {
                    return;
                }

                Core.Scale = value;
                ScreenSize = WindowSize / Scale;
                ScreenCenter = (ScreenSize / 2f).ToVector2();
            }
        }

#if DEBUG
        public bool DebugMode { get; set; }
#else
        public bool DebugMode { get { return false; } }
#endif

        #endregion

        #region Internal Properties

        internal Core Core { get; private set; }
        internal List<Surface> Surfaces { get; private set; } = new List<Surface>();

        #endregion Internal Properties

        #region Public Methods

        public void Start() {
            Debug.WriteLine("| Raccoon Started |");
            IsRunning = true;
            if (Scene == null) {
                Core.OnBegin += OnBegin;
                Core.OnUpdate += OnUpdate;
                Core.OnRender += OnRender;
            }

            Core.Run();
        }

        public void Start(string startScene) {
            if (!_scenes.ContainsKey(startScene)) {
                throw new ArgumentException($"Scene '{startScene}' not found", "startScene");
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
            _scenes.Add(name, scene);
            scene.OnAdded();

            if (Scene == null) {
                SwitchScene(name);
            }
        }

        public void RemoveScene(string name) {
            if (!_scenes.TryGetValue(name, out Scene scene)) {
                return;
            }

            if (scene == Scene) {
                Scene.End();
                Scene.UnloadContent();
                Scene = null;
            }

            _scenes.Remove(name);
        }

        public void RemoveScene(Scene scene) {
            string name = "";
            foreach (KeyValuePair<string, Scene> s in _scenes) {
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

            foreach (Scene scene in _scenes.Values) {
                scene.UnloadContent();
            }

            _scenes.Clear();
        }

        public void SwitchScene(string name) {
            if (!_scenes.ContainsKey(name)) throw new ArgumentException($"Scene '{name}' not found", "name");

            if (Scene != null) {
                Scene.End();
            }

            Core.ClearCallbacks();

            Scene = _scenes[name];
            if (Core.MainSurface != null) {
                Scene.Begin();
            } else {
                Core.OnBegin += OnBegin;
                Core.OnBegin += Scene.Begin;
            }

            Core.OnUpdate += OnUpdate;
            Core.OnRender += OnRender;

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

        public void AddSurface(Surface surface) {
            if (Surfaces.Contains(surface)) {
                return;
            }

            Surfaces.Add(surface);
        }

        public void RemoveSurface(Surface surface) {
            Surfaces.Remove(surface);
        }

        public void ClearSurfaces() {
            Surfaces.Clear();
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
