using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public class Game : System.IDisposable {
        #region Public Events

        public event System.Action<int> OnUpdate = delegate { };
        public event System.Action OnRender = delegate { }, 
                                   OnBegin = delegate { }, 
                                   OnBeforeUpdate, 
                                   OnLateUpdate = delegate { },
                                   OnWindowResize = delegate { };

        #endregion Public Events

        #region Private Members

        // scenes
        private Dictionary<string, Scene> _scenes = new Dictionary<string, Scene>();
        private bool _isUnloadingCurrentScene;

        #endregion Private Members

        #region Constructor

        public Game(string title = "Raccoon Game", int width = 800, int height = 600, int targetFramerate = 60, bool fullscreen = false, bool vsync = false) {
            Instance = this;

#if DEBUG
            try {
                System.Console.Title = "Raccoon Debug";
            } catch { }
#endif

            System.AppDomain.CurrentDomain.UnhandledException += (object sender, System.UnhandledExceptionEventArgs args) => {
                System.Exception e = (System.Exception) args.ExceptionObject;
                Debug.Log("crash-report", $"[Unhandled Exception] {e.Message}\n{e.StackTrace}\n");
            };

            TargetFramerate = targetFramerate;
            Core = new Core(title, width, height, TargetFramerate, fullscreen, vsync);
            WindowSize = new Size(width, height);
            WindowCenter = (WindowSize / 2f).ToVector2();
            Size = WindowSize / Scale;
            Center = (Size / 2f).ToVector2();

            Core.Window.ClientSizeChanged += InternalOnWindowResize;

            // events
            OnBeforeUpdate = () => {
                if (NextScene == Scene) {
                    return;
                }

                UpdateCurrentScene();
            };

            Debug.Instance.GetType(); // HACK: Force early Debug lazy initialization
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
        public bool IsFullscreen { get { return Core.Graphics.IsFullScreen; } }
        public bool IsMouseVisible { get { return Core.IsMouseVisible; } set { Core.IsMouseVisible = value; } }
        public bool AllowResize { get { return Core.Window.AllowUserResizing; } set { Core.Window.AllowUserResizing = value; } }
        public bool HasFocus { get { return Core.IsActive; } }
        public bool HardwareModeSwitch { get { return Core.Graphics.HardwareModeSwitch; } set { Core.Graphics.HardwareModeSwitch = value; } }
        public string Title { get { return Core.Title; } set { Core.Title = value; } }
        public string ContentDirectory { get { return Core.Content.RootDirectory; } set { Core.Content.RootDirectory = value; } }
        public int DeltaTime { get { return Core.DeltaTime; } }
        public int X { get { return Core.Window.Position.X; } }
        public int Y { get { return Core.Window.Position.Y; } }
        public int Width { get { return (int) Size.Width; } }
        public int Height { get { return (int) Size.Height; } }
        public int WindowWidth { get { return (int) WindowSize.Width; } }
        public int WindowHeight { get { return (int) WindowSize.Height; } }
        public int ScreenWidth { get { return Core.GraphicsDevice.DisplayMode.Width; } }
        public int ScreenHeight { get { return Core.GraphicsDevice.DisplayMode.Height; } }
        public int TargetFramerate { get; private set; }
        public Size Size { get; private set; }
        public Size WindowSize { get; private set; }
        public Size WindowMinimunSize { get; set; }
        public Size ScreenSize { get { return new Size(ScreenWidth, ScreenHeight); } }
        public Vector2 Center { get; private set; }
        public Vector2 WindowCenter { get; private set; }
        public Vector2 WindowPosition { get { return new Vector2(X, Y); } set { Core.Window.Position = new Microsoft.Xna.Framework.Point((int) value.X, (int) value.Y); } }
        public Vector2 ScreenCenter { get { return (ScreenSize / 2f).ToVector2(); } }
        public Scene Scene { get; private set; }
        public Scene NextScene { get; private set; }
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
                Size = WindowSize / Scale;
                Center = (Size / 2f).ToVector2();
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
            Debug.Info("| Raccoon Started |");
            IsRunning = true;
            if (Scene == null) {
                Core.OnBegin += OnBegin;
                UpdateCurrentScene();
            }

            Core.Run();
        }

        public void Start(string startScene) {
            if (!_scenes.ContainsKey(startScene)) {
                throw new System.ArgumentException($"Scene '{startScene}' not found", "startScene");
            }

            SwitchScene(startScene);
            UpdateCurrentScene();
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
            name = !string.IsNullOrWhiteSpace(name) ? name : scene.GetType().Name.Replace("Scene", "");
            _scenes.Add(name, scene);
            scene.OnAdded();
        }

        public void RemoveScene(string name) {
            if (!_scenes.TryGetValue(name, out Scene scene)) {
                return;
            }

            if (scene == Scene) {
                NextScene = null;
            }

            _scenes.Remove(name);
            _isUnloadingCurrentScene = true;
        }

        public void RemoveScene(Scene scene) {
            string name = "";
            foreach (KeyValuePair<string, Scene> s in _scenes) {
                if (s.Value == scene) {
                    name = s.Key;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(name)) {
                return;
            }

            RemoveScene(name);
        }

        public void RemoveScene<T>() where T : Scene {
            RemoveScene(typeof(T).Name.Replace("Scene", ""));
        }

        public void ClearScenes() {
            if (Scene != null) {
                NextScene = null;
            }

            foreach (Scene scene in _scenes.Values) {
                if (scene == Scene) {
                    continue;
                }

                scene.UnloadContent();
            }

            _scenes.Clear();
            _isUnloadingCurrentScene = true;
        }

        public Scene SwitchScene(string name) {
            if (!_scenes.TryGetValue(name, out Scene scene)) throw new System.ArgumentException($"Scene '{name}' not found", "name");
            NextScene = scene;
            return NextScene;
        }

        public T SwitchScene<T>() where T : Scene {
            return SwitchScene(typeof(T).Name.Replace("Scene", "")) as T;
        }

        public void AddSurface(Surface surface) {
            if (Surfaces.Contains(surface)) {
                return;
            }

            surface.Projection = MainSurface.Projection;
            Surfaces.Add(surface);
        }

        public void RemoveSurface(Surface surface) {
            Surfaces.Remove(surface);
        }

        public void ClearSurfaces() {
            Surfaces.Clear();
        }

        public void ResizeWindow(int width, int height) {
            Debug.Assert(width > 0 && height > 0, $"Width and Height can't be zero or smaller (width: {width}, height: {height})");

            var displayMode = Core.GraphicsDevice.DisplayMode;

            Core.Graphics.PreferredBackBufferWidth = (int) Math.Clamp(width, WindowMinimunSize.Width, displayMode.Width);
            Core.Graphics.PreferredBackBufferHeight = (int) Math.Clamp(height, WindowMinimunSize.Height, displayMode.Height);
            Core.Graphics.ApplyChanges();
        }

        public void ResizeWindow(Size size) {
            ResizeWindow((int) size.Width, (int) size.Height);
        }

        public void ToggleFullscreen() {
            Core.Graphics.ToggleFullScreen();
        }

        public void SetFullscreen(bool fullscreen) {
            if (!(IsFullscreen ^ fullscreen)) {
                return;
            }

            ToggleFullscreen();
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

        #region Private Methods

        private void UpdateCurrentScene() {
            if (Scene != null) {
                Scene.End();

                if (_isUnloadingCurrentScene) {
                    Scene.UnloadContent();
                    _isUnloadingCurrentScene = false;
                }
            }

            Core.ClearCallbacks();

            Scene = NextScene;

            Core.OnBeforeUpdate += OnBeforeUpdate;
            Core.OnUpdate += OnUpdate;
            Core.OnLateUpdate += OnLateUpdate;
            Core.OnRender += OnRender;

            if (Scene == null) {
                return;
            }

            if (Core.MainSurface != null) {
                Scene.Begin();
            } else {
                Core.OnBegin += OnBegin;
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

        private void InternalOnWindowResize(System.Object sender, System.EventArgs e) {
            var windowClientBounds = Core.Window.ClientBounds;

            // checks if preffered backbuffer size is the same as current window size
            if (Core.Graphics.PreferredBackBufferWidth != windowClientBounds.Width || Core.Graphics.PreferredBackBufferHeight != windowClientBounds.Height) {
                var displayMode = Core.GraphicsDevice.DisplayMode;
                Core.Graphics.PreferredBackBufferWidth = (int) Math.Clamp(windowClientBounds.Width, WindowMinimunSize.Width, displayMode.Width);
                Core.Graphics.PreferredBackBufferHeight = (int) Math.Clamp(windowClientBounds.Height, WindowMinimunSize.Height, displayMode.Height);
                Core.Graphics.ApplyChanges();
                return;
            }

            WindowSize = new Size(Core.Graphics.PreferredBackBufferWidth, Core.Graphics.PreferredBackBufferHeight);
            WindowCenter = (WindowSize / 2f).ToVector2();
            Size = WindowSize / Scale;
            Center = (Size / 2f).ToVector2();

            // internal resize
            // surface
            var projection = Microsoft.Xna.Framework.Matrix.CreateOrthographicOffCenter(0f, WindowWidth, WindowHeight, 0f, 1f, 0f);
            foreach (Surface surface in Surfaces) {
                surface.Projection = projection;
            }

            // canvas
            Core.RenderTargetStack.Clear();

#if DEBUG
            if (Core.DebugSurface != null) {
                Core.DebugSurface.Projection = projection;
            }

            if (Core.DebugCanvas != null) {
                Core.DebugCanvas.Resize(WindowSize);
                Core.DebugCanvas.ClippingRegion = Core.DebugCanvas.SourceRegion;
                Core.RenderTargetStack.Push(Core.DebugCanvas.XNARenderTarget);
            }
#endif

            if (Core.MainSurface != null) {
                Core.MainSurface.Projection = projection;
            }

            if (MainCanvas != null) {
                MainCanvas.Resize(WindowSize);
                MainCanvas.ClippingRegion = MainCanvas.SourceRegion;
                Core.RenderTargetStack.Push(MainCanvas.XNARenderTarget);
            }

            // user callback
            OnWindowResize();
        }

        #endregion Private Methods
    }
}
