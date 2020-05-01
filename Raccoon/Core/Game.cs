using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework.Graphics;

using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public class Game : System.IDisposable {
        #region Public Members

        public delegate void GameEventDelegate();
        public event GameEventDelegate OnBeforeRender = delegate { },
                                       OnRender = delegate { },
                                       OnAfterMainCanvasRender,
                                       OnLateRender,
                                       OnDebugRender = delegate { },
                                       OnBegin = delegate { },
                                       OnBeforeUpdate = delegate { },
                                       OnLateUpdate = delegate { },
                                       OnWindowResize = delegate { },
                                       OnActivated,
                                       OnDeactivated,
                                       OnDisposed,
                                       OnExiting;

        public delegate void GameTimedEventDelegate(int delta);
        public event GameTimedEventDelegate OnUpdate = delegate { };

        public delegate void GameLogEventWriter(StreamWriter logWriter);
        public event GameLogEventWriter OnCrash;

        #endregion Public Members

        #region Private Members

#if DEBUG
        private static readonly string DiagnosticsTextFormat = @"Time: {0}

Batches:
  Total Draw Calls: {1}
  Sprites: {2}, Primitives: {3}
  Textures: {4}

Physics:
  Update Position: {5}ms
  Solve Constraints: {6}ms
  Coll Broad Phase: (C: {7}): {8}ms
  Coll Narrow Phase: (C: {9}): {10}ms

Scene:
  Updatables: {11}
  Renderables: {12}
  Objects: {13}";

        private readonly string WindowTitleDetailed = "{0} | {1} FPS  {2:0.00} MB  GC: {3:0.00} MB";
        private const int FramerateMonitorValuesCount = 25;
        private const int FramerateMonitorDataSpacing = 4;

#endif

        // window
        private string _title;

        // fps
        private int _fpsCount, _fps;
        private System.TimeSpan _lastFpsTime;

#if DEBUG
        private Size _framerateMonitorSize;
#endif

        // rendering
        private float _pixelScale = 1f;

        // resolution mode
        private Rectangle _windowedModeBounds;
        private Vector2 _gameCanvasPosition;

        // scenes
        private Dictionary<string, Scene> _scenes = new Dictionary<string, Scene>();
        private bool _isUnloadingCurrentScene;

        #endregion Private Members

        #region Constructors

        public Game(string title = "Raccoon Game", int windowWidth = 1280, int windowHeight = 720, int targetFramerate = 60, bool fullscreen = false, bool vsync = false) {
            Instance = this;

#if DEBUG
            Debug.Start();

            try {
                System.Console.Title = "Raccoon Debug";
            } catch {
            }
#endif

            System.AppDomain.CurrentDomain.UnhandledException += (object sender, System.UnhandledExceptionEventArgs args) => {
                System.Exception e = (System.Exception) args.ExceptionObject;

                using (StreamWriter logWriter = new StreamWriter($"crash-report.log", append: false)) {
                    logWriter.WriteLine($"Operating System: {System.Environment.OSVersion} ({(System.Environment.Is64BitOperatingSystem ? "x64" : "x86")})");
                    logWriter.WriteLine($"CLR Runtime Version: {System.Environment.Version}");
                    logWriter.WriteLine($"Command Line: {System.Environment.CommandLine}\n\n");
                    OnCrash?.Invoke(logWriter);
                    logWriter.WriteLine($"\n{System.DateTime.Now.ToString()}  {e.Message}\n{e.StackTrace}\n");

                    while (e.InnerException != null) {
                        e = e.InnerException;
                        logWriter.WriteLine($"{System.DateTime.Now.ToString()}  InnerException: {e.Message}\n{e.StackTrace}\n");
                    }

                    // include report.log
                    System.Diagnostics.Debug.Flush();
                    logWriter.WriteLine("\n\nreport.log\n-------------\n");
                    logWriter.WriteLine(File.ReadAllText(Debug.LogFileName));
                }

                switch (System.Environment.OSVersion.Platform) {
                    case System.PlatformID.Win32NT:
                        System.Diagnostics.Process.Start("notepad.exe", "crash-report.log");
                        break;

                    case System.PlatformID.Unix:
                        Debug.WriteLine(File.ReadAllText("crash-report.log", System.Text.Encoding.UTF8));
                        break;

                    default:
                        break;
                }
            };

            // fps
            TargetFramerate = targetFramerate;

            // wrapper
            XNAGameWrapper = new XNAGameWrapper(windowWidth, windowHeight, TargetFramerate, fullscreen, vsync, InternalLoadContent, InternalUnloadContent, InternalUpdate, InternalDraw);
            XNAGameWrapper.Content.RootDirectory = "Content/";
            Title = title;

            // background
            BackgroundColor = Color.Black;

            // events
            XNAGameWrapper.Activated += Activated;
            XNAGameWrapper.Deactivated += Deactivated;
            XNAGameWrapper.Disposed += Disposed;
            XNAGameWrapper.Exiting += Exiting;
            XNAGameWrapper.Window.ClientSizeChanged += InternalOnWindowResize;

            // window and game internal size
            WindowCenter = new Vector2(windowWidth / 2f, windowHeight/2f);
            Size = new Size(windowWidth, windowHeight) / PixelScale;
            Center = (Size / 2f).ToVector2();

            OnBeforeUpdate = () => {
                if (NextScene == Scene) {
                    return;
                }

                UpdateCurrentScene();
            };
        }

        ~Game() {
            Dispose(false);
        }

        #endregion Constructors

        #region Static Public Properties

        public static Game Instance { get; private set; }

        #endregion Static Public Properties

        #region Public Properties

        public bool IsDisposed { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsFixedTimeStep { get { return XNAGameWrapper.IsFixedTimeStep; } }
        public bool VSync { get { return XNAGameWrapper.GraphicsDeviceManager.SynchronizeWithVerticalRetrace; } set { XNAGameWrapper.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = value; } }
        public bool IsFullscreen { get { return XNAGameWrapper.GraphicsDeviceManager.IsFullScreen; } }
        public bool IsMouseVisible { get { return XNAGameWrapper.IsMouseVisible; } set { XNAGameWrapper.IsMouseVisible = value; } }
        public bool AllowResize { get { return XNAGameWrapper.Window.AllowUserResizing; } set { XNAGameWrapper.Window.AllowUserResizing = value; } }
        public bool IsActive { get { return XNAGameWrapper.IsActive; } }
        public bool IsRunningSlowly { get; private set; }
        public string ContentDirectory { get { return XNAGameWrapper.Content.RootDirectory; } set { XNAGameWrapper.Content.RootDirectory = value; } }
        public string StartSceneName { get; private set; }
        public int LastUpdateDeltaTime { get; private set; }
        public int Width { get { return (int) Size.Width; } }
        public int Height { get { return (int) Size.Height; } }
        public int WindowWidth { get { return XNAGameWrapper.Window.ClientBounds.Width; } }
        public int WindowHeight { get { return XNAGameWrapper.Window.ClientBounds.Height; } }
        public int DisplayWidth { get { return XNAGameWrapper.GraphicsDevice.DisplayMode.Width; } }
        public int DisplayHeight { get { return XNAGameWrapper.GraphicsDevice.DisplayMode.Height; } }
        public int TargetFramerate { get; private set; }
        public Size Size { get; private set; }
        public Size WindowSize { get { return new Size(XNAGameWrapper.Window.ClientBounds.Width, XNAGameWrapper.Window.ClientBounds.Height); } }
        public Size DisplaySize { get { return new Size(XNAGameWrapper.GraphicsDevice.DisplayMode.Width, XNAGameWrapper.GraphicsDevice.DisplayMode.Height); } }
        public Vector2 Center { get; private set; }
        public Vector2 WindowCenter { get; private set; }
        public Vector2 DisplayCenter { get { return (DisplaySize / 2f).ToVector2(); } }
        public Scene PreviousScene { get; private set; }
        public Scene Scene { get; private set; }
        public Scene NextScene { get; private set; }
        public Font StdFont { get; private set; }
        public Color BackgroundColor { get; set; }
        public Renderer MainRenderer { get; private set; }
        public Renderer DebugRenderer { get; private set; }
        public Renderer ScreenRenderer { get; private set; }
        public System.TimeSpan Time { get; private set; }
        public PrimitiveBatch DebugPrimitiveBatch { get; private set; }
        public BasicShader BasicShader { get; private set; }

        public string Title {
            get {
                return _title;
            }

            set {
                _title = value;
#if DEBUG
                System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                XNAGameWrapper.Window.Title = string.Format(WindowTitleDetailed, _title, _fps, currentProcess.PrivateMemorySize64 / 1048576.0, System.GC.GetTotalMemory(false) / 1048576.0);
#else
                Core.Window.Title = _title;
#endif
            }
        }

        public Vector2 WindowPosition {
            get {
                return new Vector2(XNAGameWrapper.Window.ClientBounds.X, XNAGameWrapper.Window.ClientBounds.Y);
            }

            set {
                SDL2.SDL.SDL_SetWindowPosition(XNAGameWrapper.Window.Handle, (int) value.X, (int) value.Y);
            }
        }

        public Size WindowMinimumSize {
            get {
                SDL2.SDL.SDL_GetWindowMinimumSize(XNAGameWrapper.Window.Handle, out int minWidth, out int minHeight);
                return new Size(minWidth, minHeight);
            }

            set {
                SDL2.SDL.SDL_SetWindowMinimumSize(XNAGameWrapper.Window.Handle, (int) value.Width, (int) value.Height);
            }
        }

        public Size WindowMaximumSize {
            get {
                SDL2.SDL.SDL_GetWindowMaximumSize(XNAGameWrapper.Window.Handle, out int maxWidth, out int maxHeight);
                return new Size(maxWidth, maxHeight);
            }

            set {
                SDL2.SDL.SDL_SetWindowMaximumSize(XNAGameWrapper.Window.Handle, (int) value.Width, (int) value.Height);
            }
        }

        public float PixelScale {
            get {
                return _pixelScale;
            }

            set {
                _pixelScale = value;

                if (IsRunning) {
                    Size = WindowSize / _pixelScale;
                } else {
                    // when game isn't running yet, probably GraphicsDeviceManager hasn't applied changes to window size
                    // so just use preffered back buffer size instead current window size
                    Size = new Size(XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferWidth, XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferHeight) / _pixelScale;
                }

                Center = (Size / 2f).ToVector2();

                if (!IsRunning) {
                    return;
                }

                MainCanvas.Resize(Size);

                foreach (Renderer renderer in Renderers) {
                    renderer.RecalculateProjection();
                }

                ScreenRenderer.RecalculateProjection();
            }
        }

        public float KeepProportionsScale { get; private set; } = 1f;
        public ResizeMode ResizeMode { get; set; } = ResizeMode.ExpandView;

#if DEBUG
        public bool DebugMode { get; set; }
        public List<int> FramerateValues { get; } = new List<int>();
#else
        public bool DebugMode { get { return false; } }
#endif

        #endregion

        #region Internal Properties

        internal XNAGameWrapper XNAGameWrapper { get; set; }
        internal GraphicsDevice GraphicsDevice { get { return XNAGameWrapper.GraphicsDevice; } }
        internal Canvas MainCanvas { get; private set; }
        internal List<Renderer> Renderers { get; private set; } = new List<Renderer>();
        internal Stack<RenderTarget2D> RenderTargetStack { get; private set; } = new Stack<RenderTarget2D>();

#if DEBUG
        internal Canvas DebugCanvas { get; private set; }
#endif

        #endregion Internal Properties

        #region Public Methods

        public void Start() {
            Debug.Info("| Raccoon Started |");
            IsRunning = true;
            XNAGameWrapper.Run();
        }

        public void Start(string startScene) {
            if (!_scenes.ContainsKey(startScene)) {
                throw new System.ArgumentException($"Scene '{startScene}' not found", "startScene");
            }

            StartSceneName = startScene;

            IsRunning = false;
            PreviousScene = Scene = NextScene = null;

            Start();
        }

        public void Start<T>() where T : Scene {
            Start(typeof(T).Name.Replace("Scene", ""));
        }

        public void Exit() {
            Debug.WriteLine("Exiting...");
            IsRunning = false;
            XNAGameWrapper.Exit();
        }

        public void Dispose() {
            Dispose(true);
        }

        public T GetScene<T>() where T : Scene {
            System.Type type = typeof(T);
            foreach (KeyValuePair<string, Scene> entry in _scenes) {
                if (entry.Value.GetType() == type) {
                    return entry.Value as T;
                }
            }

            throw new System.ArgumentException($"Scene '{type.Name}' is invalid or isn't registered yet.");
        }

        public Scene GetScene(string name) {
            if (!_scenes.TryGetValue(name, out Scene scene)) {
                throw new System.ArgumentException($"Scene '{name}' not found", "name");
            }

            return scene;
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
            NextScene = GetScene(name);
            return NextScene;
        }

        public T SwitchScene<T>() where T : Scene {
            T scene = GetScene<T>();
            NextScene = scene;
            return scene;
        }

        public Scene SwitchScene(Scene scene) {
            foreach (KeyValuePair<string, Scene> entry in _scenes) {
                if (entry.Value == scene) {
                    return SwitchScene(entry.Key);
                }
            }

            throw new System.ArgumentException($"Scene '{scene.GetType().Name}' is invalid or isn't registered yet.");
        }

        public bool HasScene(string name) {
            return _scenes.ContainsKey(name);
        }

        public bool HasScene<T>() where T : Scene {
            return _scenes.ContainsKey(typeof(T).Name.Replace("Scene", ""));
        }

        public Renderer AddRenderer(Renderer renderer) {
            if (renderer == null) {
                throw new System.ArgumentNullException("renderer");
            }

            if (!Renderers.Contains(renderer)) {
                Renderers.Add(renderer);
                renderer.RecalculateProjection();
            }

            return renderer;
        }

        public void RemoveRenderer(Renderer renderer) {
            Renderers.Remove(renderer);
        }

        public void ClearRenderers() {
            Renderers.Clear();
        }

        public void ResizeWindow(int width, int height) {
            Debug.Assert(width > 0 && height > 0, $"Width and Height can't be zero or smaller (width: {width}, height: {height})");

            if (width == WindowWidth && height == WindowHeight) {
                return;
            }

            DisplayMode displayMode = XNAGameWrapper.GraphicsDevice.DisplayMode;

            XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferWidth = (int) Math.Clamp(width, WindowMinimumSize.Width, displayMode.Width);
            XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferHeight = (int) Math.Clamp(height, WindowMinimumSize.Height, displayMode.Height);
            XNAGameWrapper.GraphicsDeviceManager.ApplyChanges();

            RefreshViewMode();
        }

        public void ResizeWindow(int width, int height, bool fullscreen) {
            Debug.Assert(width > 0 && height > 0, $"Width and Height can't be zero or smaller (width: {width}, height: {height})");

            DisplayMode displayMode = XNAGameWrapper.GraphicsDevice.DisplayMode;

            XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferWidth = (int) Math.Clamp(width, WindowMinimumSize.Width, displayMode.Width);
            XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferHeight = (int) Math.Clamp(height, WindowMinimumSize.Height, displayMode.Height);
            XNAGameWrapper.GraphicsDeviceManager.IsFullScreen = fullscreen;
            XNAGameWrapper.GraphicsDeviceManager.ApplyChanges();

            RefreshViewMode();
        }

        public void ResizeWindow(Size size) {
            ResizeWindow((int) size.Width, (int) size.Height);
        }

        public void ToggleFullscreen() {
            bool isSwitchingToFullscreen = !IsFullscreen;

            Size newWindowSize = Size.Empty;
            if (!isSwitchingToFullscreen) {
                XNAGameWrapper.GraphicsDeviceManager.ToggleFullScreen();

                if (_windowedModeBounds.IsEmpty) {
                    newWindowSize = WindowSize;
                    WindowPosition = new Vector2(32);
                } else {
                    newWindowSize = _windowedModeBounds.Size;
                    WindowPosition = _windowedModeBounds.Position;

#if WINDOWS
                    if (WindowPosition == Vector2.Zero) {
                        WindowPosition += new Vector2(32);
                    }
#endif
                }
            } else {
                _windowedModeBounds = new Rectangle(WindowPosition, WindowSize);
                DisplayMode displayMode = XNAGameWrapper.GraphicsDevice.DisplayMode;
                newWindowSize = new Size(displayMode.Width, displayMode.Height);
            }

            ResizeWindow(newWindowSize);

            if (isSwitchingToFullscreen) {
                XNAGameWrapper.GraphicsDeviceManager.ToggleFullScreen();
            }
        }

        public void SetFullscreen(bool fullscreen) {
            if (!(IsFullscreen ^ fullscreen)) {
                return;
            }

            ToggleFullscreen();
        }

        #endregion

        #region Protected Methods
        protected virtual void Initialize() {
            // systems initialization
            Debug.Console.Start();

            OnBegin?.Invoke();
            OnBegin = null;

            SwitchScene(StartSceneName);
            UpdateCurrentScene();

            // late systems initialization
            Util.Tween.Tweener.Instance.Start();
        }

        protected virtual void Update(int delta) {
            // every system update
            Input.Input.Instance.Update(delta);
            Util.Tween.Tweener.Instance.Update(delta);
            OnBeforeUpdate();
            Scene?.BeforeUpdate();
            OnUpdate(delta);
            Scene?.Update(delta);
            Physics.Instance.Update(delta);
            Coroutines.Instance.Update(delta);
            OnLateUpdate();
            Scene?.LateUpdate();

#if DEBUG
            Debug.Instance.Update(delta);
#endif

            // fps
            if (Time.Subtract(_lastFpsTime).Seconds >= 1) {
                _lastFpsTime = Time;
                _fps = _fpsCount;
                _fpsCount = 0;
#if DEBUG
                FramerateValues.RemoveAt(0);
                FramerateValues.Add(_fps);
                Title = Title; // force update window title info
#endif
            }
        }

        protected virtual void Render() {
            OnRender();
            Scene?.Render();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        protected virtual void DebugRender() {
#if DEBUG
            if (DebugMode) {
                OnDebugRender();
                Scene?.DebugRender();
            }

            Debug.Instance.Render();

            if (Debug.ShowPerformanceDiagnostics) {
                Debug.DrawString(
                    null,
                    new Vector2(WindowWidth - 260, 15),
                    string.Format(
                        DiagnosticsTextFormat,

                        // time
                        Time.ToString(@"hh\:mm\:ss\.fff"),

                        // batches
                        Graphics.SpriteBatch.TotalDrawCalls + PrimitiveBatch.TotalFilledDrawCalls + PrimitiveBatch.TotalHollowDrawCalls,
                        Graphics.SpriteBatch.SpriteCount,
                        PrimitiveBatch.PrimitivesCount,
                        "-",

                        // physics
                        Physics.UpdatePositionExecutionTime,
                        Physics.SolveConstraintsExecutionTime,
                        Physics.CollidersBroadPhaseCount, Physics.CollisionDetectionBroadPhaseExecutionTime,
                        Physics.CollidersNarrowPhaseCount, Physics.CollisionDetectionNarrowPhaseExecutionTime,

                        // scene
                        Scene == null ? "0" : Scene.UpdatableCount.ToString(),
                        Scene == null ? "0" : Scene.RenderableCount.ToString(),
                        Scene == null ? "0" : Scene.SceneObjectsCount.ToString()
                    )
                );

                // framerate monitor frame
                Rectangle framerateMonitorRect = new Rectangle(new Vector2(WindowWidth - 260 - _framerateMonitorSize.Width - 32, 15), _framerateMonitorSize);
                Debug.DrawRectangle(null, framerateMonitorRect, Color.White);

                // plot framerate values
                int previousFramerateValue = FramerateValues[0], currentFramerateValue;
                Vector2 monitorBottomLeft = framerateMonitorRect.BottomLeft;
                Vector2 previousPos = monitorBottomLeft + new Vector2(1, -previousFramerateValue - 1), currentPos;

                for (int i = 1; i < FramerateValues.Count; i++) {
                    currentFramerateValue = FramerateValues[i];
                    currentPos = monitorBottomLeft + new Vector2(1 + i * FramerateMonitorDataSpacing, -currentFramerateValue - 1);

                    // pick a color based on framerate
                    Color color;
                    if (currentFramerateValue >= TargetFramerate * .95f) {
                        color = Color.Cyan;
                    } else if (currentFramerateValue >= TargetFramerate * .75f) {
                        color = Color.Yellow;
                    } else if (currentFramerateValue >= TargetFramerate * .5f) {
                        color = Color.Orange;
                    } else {
                        color = Color.Red;
                    }

                    Debug.DrawLine(null, previousPos, currentPos, color);
                    previousPos = currentPos;
                    previousFramerateValue = currentFramerateValue;
                }
            }

            Physics.Instance.ClearTimers();
#endif
        }

        protected virtual void Dispose(bool disposing) {
            if (!IsDisposed) {
                if (disposing) {
                    XNAGameWrapper.Dispose();
                }

                IsRunning = false;
                IsDisposed = true;
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

            PreviousScene = Scene;
            Scene = NextScene;

            if (Scene != null && MainRenderer != null) {
                Scene.Begin();
            }
        }

        private void InternalOnWindowResize(object sender, System.EventArgs e) {
            Microsoft.Xna.Framework.Rectangle windowClientBounds = XNAGameWrapper.Window.ClientBounds;

            // checks if preffered backbuffer size is the same as current window size
            if (XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferWidth != windowClientBounds.Width || XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferHeight != windowClientBounds.Height) {
                DisplayMode displayMode = XNAGameWrapper.GraphicsDevice.DisplayMode;
                XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferWidth = (int) Math.Clamp(windowClientBounds.Width, WindowMinimumSize.Width, displayMode.Width);
                XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferHeight = (int) Math.Clamp(windowClientBounds.Height, WindowMinimumSize.Height, displayMode.Height);
                XNAGameWrapper.GraphicsDeviceManager.ApplyChanges();
            }

            RefreshViewMode();
        }

        private void RefreshViewMode() {
            WindowCenter = (WindowSize / 2f).ToVector2();

            switch (ResizeMode) {
                case ResizeMode.KeepProportions:
                    KeepProportionsScale = WindowHeight / (Height * PixelScale);

                    // width correction
                    float internalGameWidth = Width * PixelScale * KeepProportionsScale;
                    _gameCanvasPosition = new Vector2((WindowWidth - internalGameWidth) / 2f, 0f);
                    break;

                case ResizeMode.ExpandView:
                    KeepProportionsScale = 1f;
                    _gameCanvasPosition = Vector2.Zero;

                    Size = WindowSize / PixelScale;
                    Center = (Size / 2f).ToVector2();
                    break;

                default:
                    break;
            }


            // internal resize
            // renderer
            foreach (Renderer renderer in Renderers) {
                renderer.RecalculateProjection();
            }

            // canvas
            RenderTargetStack.Clear();

            // game renderers projection
            if (MainCanvas != null) {
                MainCanvas.Resize(Size);
                MainCanvas.ClippingRegion = MainCanvas.SourceRegion;
            }

#if DEBUG
            if (DebugCanvas != null) {
                DebugCanvas.Resize(WindowSize);
                DebugCanvas.ClippingRegion = DebugCanvas.SourceRegion;
            }
#endif

            ScreenRenderer.RecalculateProjection();

            // user callback
            OnWindowResize();
        }

        private void InternalLoadContent() {
            // default content
            StdFont = new Font(Resource._04b03, 0, 12f);
            BasicShader = new BasicShader(Resource.BasicShader) {
                DepthWriteEnabled = true
            };

            ScreenRenderer = new Renderer();
            DebugRenderer = new Renderer();

            MainRenderer = new Renderer(autoHandleAlphaBlendedSprites: true) {
                SpriteBatchMode = BatchMode.DepthBuffer,
                DepthStencilState = DepthStencilState.Default,
                RecalculateProjectionSize = () => Size
            };

            GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            MainCanvas = new Canvas(
                Width, 
                Height, 
                mipMap: false, 
                SurfaceFormat.Color, 
                DepthFormat.Depth24Stencil8, 
                multiSampleCount: 0, 
                RenderTargetUsage.PreserveContents
            ) {
                //InternalRenderer = MainRenderer
                InternalRenderer = null
            };

            AddRenderer(MainRenderer);

#if DEBUG
            DebugCanvas = new Canvas(
                WindowWidth, 
                WindowHeight, 
                mipMap: false, 
                SurfaceFormat.Color, 
                DepthFormat.None, 
                multiSampleCount: 0, 
                RenderTargetUsage.PreserveContents
            ) {
                InternalRenderer = DebugRenderer
            };

            float monitorFrameWidth = ((FramerateMonitorValuesCount - 1) * FramerateMonitorDataSpacing) + 1;
            _framerateMonitorSize = new Size(monitorFrameWidth, 82);

            for (int i = 0; i < FramerateMonitorValuesCount; i++) {
                FramerateValues.Add(0);
            }
#endif

            DebugPrimitiveBatch = new PrimitiveBatch();

            if (XNAGameWrapper.GraphicsDeviceManager.IsFullScreen) {
                // adjust window size to be the display size
                ResizeWindow(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
            }

            MainRenderer.RecalculateProjection();
            DebugRenderer.RecalculateProjection();
            ScreenRenderer.RecalculateProjection();

            Initialize();
        }

        private void InternalUnloadContent() {
            Scene?.UnloadContent();
            Graphics.Texture.White.Dispose();
            Graphics.Texture.Black.Dispose();
        }

        private void InternalUpdate(Microsoft.Xna.Framework.GameTime gameTime) {
            Time = gameTime.TotalGameTime;
            IsRunningSlowly = gameTime.IsRunningSlowly;
            int delta = gameTime.ElapsedGameTime.Milliseconds;
            LastUpdateDeltaTime = delta;
            Update(delta);
        }

        private void InternalDraw(Microsoft.Xna.Framework.GameTime gameTime) {
#if DEBUG
            Graphics.SpriteBatch.ResetMetrics();
            PrimitiveBatch.ResetMetrics();
#endif

            OnBeforeRender();

            MainCanvas.Begin(BackgroundColor);

            foreach (Renderer renderer in Instance.Renderers) {
                renderer.Begin();
            }

            Render();

            foreach (Renderer renderer in Instance.Renderers) {
                renderer.End();
            }

            MainCanvas.End();

#if DEBUG

            // debug render
            DebugCanvas.Begin(Color.Transparent);

            DebugPrimitiveBatch.Begin(DebugRenderer.World, DebugRenderer.View, DebugRenderer.Projection);
            DebugRender();
            DebugPrimitiveBatch.End();

            DebugCanvas.End();
#endif

            // draw main render target to screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            ScreenRenderer.Begin();

            ScreenRenderer.Draw(
                MainCanvas,
                _gameCanvasPosition,
                null,
                0f,
                new Vector2(PixelScale * KeepProportionsScale),
                ImageFlip.None,
                Color.White,
                Vector2.Zero,
                Vector2.One
            );

            OnAfterMainCanvasRender?.Invoke();

#if DEBUG
            ScreenRenderer.Draw(
                DebugCanvas,
                _gameCanvasPosition,
                null,
                0f,
                Vector2.One,
                ImageFlip.None,
                Color.White,
                Vector2.Zero,
                Vector2.One
            );
#endif

            OnLateRender?.Invoke();

            ScreenRenderer.End();

            _fpsCount++;
        }

        private void Activated(object sender, System.EventArgs args) {
            Input.Input.Instance?.OnGameActivated();
            OnActivated?.Invoke();
        }

        private void Deactivated(object sender, System.EventArgs args) {
            Input.Input.Instance?.OnGameDeactivated();
            OnDeactivated?.Invoke();
        }

        private void Disposed(object sender, System.EventArgs args) {
            OnDisposed?.Invoke();
        }

        private void Exiting(object sender, System.EventArgs args) {
            OnExiting?.Invoke();
        }

        #endregion Private Methods
    }
}
