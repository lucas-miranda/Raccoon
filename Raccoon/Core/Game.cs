using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public class Game : System.IDisposable {
        #region Public Events

        public event System.Action OnRender = delegate { }, 
                                   OnDebugRender = delegate { },
                                   OnBegin = delegate { }, 
                                   OnBeforeUpdate = delegate { },
                                   OnLateUpdate = delegate { },
                                   OnWindowResize = delegate { };

        public event System.Action<int> OnUpdate = delegate { };

        #endregion Public Events

        #region Private Members

#if DEBUG
        private static readonly string DiagnosticsTextFormat = @"Time: {0}

Draw Calls: {1}, Sprites: {2}
Texture: {3}

Physics:
  Update Position: {4}ms
  Solve Constraints: {5}ms
  Coll Broad Phase: (C: {6}): {7}ms
  Coll Narrow Phase: (C: {8}): {9}ms

Scene:
  Updatables: {10}
  Renderables: {11}
  Objects: {12}";

        private readonly string WindowTitleDetailed = "{0} | {1} FPS {2:0.00} MB";
        private const int FramerateMonitorValuesCount = 25;
        private const int FramerateMonitorDataSpacing = 4;

#endif

        // window
        private string _title;

        // fps
        private int _fpsCount, _fps;
        private System.TimeSpan _lastFpsTime;

#if DEBUG
        private Rectangle _framerateMonitorFrame;
#endif

        // rendering
        private float _pixelScale = 1f;

        // resolution mode
        private Rectangle _windowedModeBounds;

        // scenes
        private Dictionary<string, Scene> _scenes = new Dictionary<string, Scene>();
        private bool _isUnloadingCurrentScene;

        #endregion Private Members

        #region Constructor

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
                Debug.Log("crash-report", $"[Unhandled Exception] {e.Message}\n{e.StackTrace}\n");
            };

            // fps
            TargetFramerate = targetFramerate;

            // wrapper
            XNAGameWrapper = new XNAGameWrapper(windowWidth, windowHeight, TargetFramerate, fullscreen, vsync, InternalLoadContent, InternalUnloadContent, InternalUpdate, InternalDraw);
            XNAGameWrapper.Content.RootDirectory = "Content/";
            Title = title;

            // background
            BackgroundColor = Color.Black;

            // window and resolution
            WindowSize = new Size(windowWidth, windowHeight);
            WindowCenter = (WindowSize / 2f).ToVector2();
            Size = WindowSize / PixelScale;
            Center = (Size / 2f).ToVector2();

            // events
            XNAGameWrapper.Window.ClientSizeChanged += InternalOnWindowResize;

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

        #endregion

        #region Static Public Properties

        public static Game Instance { get; private set; }

        #endregion Static Public Properties

        #region Public Properties

        public bool Disposed { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsFixedTimeStep { get { return XNAGameWrapper.IsFixedTimeStep; } }
        public bool VSync { get { return XNAGameWrapper.GraphicsDeviceManager.SynchronizeWithVerticalRetrace; } set { XNAGameWrapper.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = value; } }
        public bool IsFullscreen { get { return XNAGameWrapper.GraphicsDeviceManager.IsFullScreen; } }
        public bool IsMouseVisible { get { return XNAGameWrapper.IsMouseVisible; } set { XNAGameWrapper.IsMouseVisible = value; } }
        public bool AllowResize { get { return XNAGameWrapper.Window.AllowUserResizing; } set { XNAGameWrapper.Window.AllowUserResizing = value; } }
        public bool HasFocus { get { return XNAGameWrapper.IsActive; } }
        public bool HardwareModeSwitch { get { return XNAGameWrapper.GraphicsDeviceManager.HardwareModeSwitch; } set { XNAGameWrapper.GraphicsDeviceManager.HardwareModeSwitch = value; } }
        public bool IsRunningSlowly { get; private set; }
        public string ContentDirectory { get { return XNAGameWrapper.Content.RootDirectory; } set { XNAGameWrapper.Content.RootDirectory = value; } }
        public int LastUpdateDeltaTime { get; private set; }
        public int X { get { return XNAGameWrapper.Window.Position.X; } }
        public int Y { get { return XNAGameWrapper.Window.Position.Y; } }
        public int Width { get { return (int) Size.Width; } }
        public int Height { get { return (int) Size.Height; } }
        public int WindowWidth { get { return (int) WindowSize.Width; } }
        public int WindowHeight { get { return (int) WindowSize.Height; } }
        public int ScreenWidth { get { return XNAGameWrapper.GraphicsDevice.DisplayMode.Width; } }
        public int ScreenHeight { get { return XNAGameWrapper.GraphicsDevice.DisplayMode.Height; } }
        public int TargetFramerate { get; private set; }
        public Size Size { get; private set; }
        public Size WindowSize { get; private set; }
        public Size WindowMinimunSize { get; set; }
        public Size ScreenSize { get { return new Size(ScreenWidth, ScreenHeight); } }
        public Vector2 Center { get; private set; }
        public Vector2 WindowCenter { get; private set; }
        public Vector2 WindowPosition { get { return new Vector2(X, Y); } set { XNAGameWrapper.Window.Position = new Microsoft.Xna.Framework.Point((int) value.X, (int) value.Y); } }
        public Vector2 ScreenCenter { get { return (ScreenSize / 2f).ToVector2(); } }
        public Scene Scene { get; private set; }
        public Scene NextScene { get; private set; }
        public Font StdFont { get; private set; }
        public Color BackgroundColor { get; set; }
        public Renderer MainRenderer { get; private set; }
        public Renderer DebugRenderer { get; private set; }
        public System.TimeSpan Time { get; private set; }

        public string Title {
            get {
                return _title;
            }

            set {
                _title = value;
#if DEBUG
                XNAGameWrapper.Window.Title = string.Format(WindowTitleDetailed, _title, _fps, System.GC.GetTotalMemory(false) / 1048576f);
#else
                Core.Window.Title = _title;
#endif
            }
        }

        public float PixelScale {
            get {
                return _pixelScale;
            }

            set {
                _pixelScale = value;

                Size = WindowSize / _pixelScale;
                Center = (Size / 2f).ToVector2();

                if (!IsRunning) {
                    return;
                }

                MainCanvas.Resize(Size);
            }
        }

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
        internal SpriteBatch MainSpriteBatch { get; private set; }
        internal BasicShader BasicShader { get; private set; }
        internal Canvas MainCanvas { get; private set; }
        internal List<Renderer> Renderers { get; private set; } = new List<Renderer>();
        internal Stack<RenderTarget2D> RenderTargetStack { get; private set; } = new Stack<RenderTarget2D>();
        internal ResourceContentManager DefaultResourceContentManager { get; private set; }

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

            SwitchScene(startScene);
            UpdateCurrentScene();
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
            if (!_scenes.TryGetValue(name, out Scene scene)) {
                throw new System.ArgumentException($"Scene '{name}' not found", "name");
            }

            NextScene = scene;
            return NextScene;
        }

        public T SwitchScene<T>() where T : Scene {
            return SwitchScene(typeof(T).Name.Replace("Scene", "")) as T;
        }

        public Renderer AddRenderer(Renderer renderer) {
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

            var displayMode = XNAGameWrapper.GraphicsDevice.DisplayMode;

            XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferWidth = (int) Math.Clamp(width, WindowMinimunSize.Width, displayMode.Width);
            XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferHeight = (int) Math.Clamp(height, WindowMinimunSize.Height, displayMode.Height);
            XNAGameWrapper.GraphicsDeviceManager.ApplyChanges();
        }

        public void ResizeWindow(Size size) {
            ResizeWindow((int) size.Width, (int) size.Height);
        }

        public void ToggleFullscreen() {
            bool isSwitchingToFullscreen = !IsFullscreen;

            Size newWindowSize = Size.Empty;
            if (!isSwitchingToFullscreen) {
                XNAGameWrapper.GraphicsDeviceManager.ToggleFullScreen();
                newWindowSize = _windowedModeBounds.Size;
                WindowPosition = _windowedModeBounds.Position;
            } else {
                _windowedModeBounds = new Rectangle(WindowPosition, WindowSize);
                var displayMode = XNAGameWrapper.GraphicsDevice.DisplayMode;
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

            OnBegin();
            OnBegin = null;

            Scene?.Begin();

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
        protected virtual void DebugRender(GraphicsMetrics metrics) {
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
                        Time.ToString(@"hh\:mm\:ss\.fff"),
                        metrics.DrawCount,
                        metrics.SpriteCount,
                        metrics.TextureCount,
                        Physics.UpdatePositionExecutionTime,
                        Physics.SolveConstraintsExecutionTime,
                        Physics.CollidersBroadPhaseCount, Physics.CollisionDetectionBroadPhaseExecutionTime,
                        Physics.CollidersNarrowPhaseCount, Physics.CollisionDetectionNarrowPhaseExecutionTime,
                        Scene == null ? "0" : Scene.UpdatableCount.ToString(),
                        Scene == null ? "0" : Scene.RenderableCount.ToString(),
                        Scene == null ? "0" : Scene.SceneObjectsCount.ToString()
                    )
                );

                // framerate monitor frame
                Debug.DrawRectangle(null, _framerateMonitorFrame, Color.White);

                // plot framerate values
                int previousFramerateValue = FramerateValues[0], currentFramerateValue;
                Vector2 monitorBottomLeft = _framerateMonitorFrame.BottomLeft;
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
            if (!Disposed) {
                if (disposing) {
                    XNAGameWrapper.Dispose();
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

            Scene = NextScene;

            if (Scene != null && MainRenderer != null) {
                Scene.Begin();
            }
        }

        private void InternalOnWindowResize(object sender, System.EventArgs e) {
            var windowClientBounds = XNAGameWrapper.Window.ClientBounds;

            // checks if preffered backbuffer size is the same as current window size
            if (XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferWidth != windowClientBounds.Width || XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferHeight != windowClientBounds.Height) {
                var displayMode = XNAGameWrapper.GraphicsDevice.DisplayMode;
                XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferWidth = (int) Math.Clamp(windowClientBounds.Width, WindowMinimunSize.Width, displayMode.Width);
                XNAGameWrapper.GraphicsDeviceManager.PreferredBackBufferHeight = (int) Math.Clamp(windowClientBounds.Height, WindowMinimunSize.Height, displayMode.Height);
                XNAGameWrapper.GraphicsDeviceManager.ApplyChanges();
                return;
            }

            if (WindowSize.Width == windowClientBounds.Width && WindowSize.Height == windowClientBounds.Height) {
                return;
            }

            WindowSize = new Size(windowClientBounds.Width, windowClientBounds.Height);
            WindowCenter = (WindowSize / 2f).ToVector2();
            Size = WindowSize / PixelScale;
            Center = (Size / 2f).ToVector2();

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

            // user callback
            OnWindowResize();
        }

        private void InternalLoadContent() {
            if (XNAGameWrapper.GraphicsDeviceManager.IsFullScreen) {
                ResizeWindow(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
            }

            MainSpriteBatch = new SpriteBatch(GraphicsDevice);

            DebugRenderer = new Renderer(Graphics.BlendState.AlphaBlend);
            MainRenderer = new Renderer(Graphics.BlendState.AlphaBlend) {
                RecalculateProjectionSize = () => {
                    float zoom = Camera.Current == null ? 1f : Camera.Current.Zoom;
                    float scaleFactor = 1f / (zoom * PixelScale);
                    return new Size(WindowWidth * scaleFactor, WindowHeight * scaleFactor);
                }
            };

            MainCanvas = new Canvas(Width, Height, false, Graphics.SurfaceFormat.Color, Graphics.DepthFormat.None, 0, CanvasUsage.PreserveContents) {
                InternalRenderer = MainRenderer
            };

#if DEBUG
            DebugCanvas = new Canvas(WindowWidth, WindowHeight, false, Graphics.SurfaceFormat.Color, Graphics.DepthFormat.None, 0, CanvasUsage.PreserveContents) {
                InternalRenderer = DebugRenderer
            };

            float monitorFrameWidth = ((FramerateMonitorValuesCount - 1) * FramerateMonitorDataSpacing) + 1;
            _framerateMonitorFrame = new Rectangle(new Vector2(WindowWidth - 260 - monitorFrameWidth - 32, 15), new Size(monitorFrameWidth, 82));
            for (int i = 0; i < FramerateMonitorValuesCount; i++) {
                FramerateValues.Add(0);
            }
#endif

            // default content
            DefaultResourceContentManager = new ResourceContentManager(XNAGameWrapper.Services, Resource.ResourceManager);
            StdFont = new Font(DefaultResourceContentManager.Load<SpriteFont>("Zoomy"));
            BasicShader = new BasicShader(DefaultResourceContentManager.Load<Effect>("BasicEffect"));

            Initialize();
        }

        private void InternalUnloadContent() {
            Scene.UnloadContent();
            DefaultResourceContentManager.Unload();
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
            MainCanvas.Begin(BackgroundColor);

            foreach (Renderer renderer in Instance.Renderers) {
                renderer.Begin(SpriteSortMode.Texture, SamplerState.PointClamp, DepthStencilState.Default, null, BasicShader.XNAEffect, null);
            }
            
            Render();

            foreach (Renderer renderer in Instance.Renderers) {
                PrepareShader(renderer);
                renderer.End();
                CleanupShader();
            }

            MainCanvas.End();

#if DEBUG
            GraphicsMetrics metrics = GraphicsDevice.Metrics;

            // debug render
            DebugCanvas.Begin(Color.Transparent);
            DebugRender(metrics);
            DebugCanvas.End();
#endif

            // draw main render target to screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            MainSpriteBatch.Begin(SpriteSortMode.Immediate, Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null);

            // TODO: Add a way to register custom canvas to render here (each one should have a ID for sorting)
            MainSpriteBatch.Draw(MainCanvas.XNARenderTarget, Microsoft.Xna.Framework.Vector2.Zero, null, Color.White, 0f, Microsoft.Xna.Framework.Vector2.Zero, new Microsoft.Xna.Framework.Vector2(PixelScale), SpriteEffects.None, 0f);

#if DEBUG
            MainSpriteBatch.Draw(DebugCanvas.XNARenderTarget, Microsoft.Xna.Framework.Vector2.Zero, Color.White);
#endif

            MainSpriteBatch.End();

            _fpsCount++;
        }

        private void PrepareShader(Renderer renderer) {
            BasicShader.View = renderer.View;
            BasicShader.Projection = renderer.Projection;
            BasicShader.TextureEnabled = true;
            BasicShader.UpdateParameters();
        }

        private void CleanupShader() {
            BasicShader.ResetParameters();
        }

        #endregion Private Methods
    }
}
