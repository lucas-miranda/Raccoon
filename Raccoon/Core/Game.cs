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
        private int _unitsToPixel;
        private float _pixelScale;

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
            try {
                System.Console.Title = "Raccoon Debug";
            } catch { }
#endif

            System.AppDomain.CurrentDomain.UnhandledException += (object sender, System.UnhandledExceptionEventArgs args) => {
                System.Exception e = (System.Exception) args.ExceptionObject;
                Debug.Log("crash-report", $"[Unhandled Exception] {e.Message}\n{e.StackTrace}\n");
            };

            // fps
            TargetFramerate = targetFramerate;

            // wrapper
            Core = new XNAGameWrapper(windowWidth, windowHeight, TargetFramerate, fullscreen, vsync, InternalLoadContent, InternalUnloadContent, InternalUpdate, InternalDraw);
            Title = title;

            // content
            Core.Content.RootDirectory = "Content/";

            // background
            BackgroundColor = Color.Black;

            // window and resolution
            WindowSize = new Size(windowWidth, windowHeight);
            WindowCenter = (WindowSize / 2f).ToVector2();
            Size = WindowSize / PixelScale;
            Center = (Size / 2f).ToVector2();

            // events
            Core.Window.ClientSizeChanged += InternalOnWindowResize;

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
        public bool VSync { get { return Core.GraphicsDeviceManager.SynchronizeWithVerticalRetrace; } set { Core.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = value; } }
        public bool IsFullscreen { get { return Core.GraphicsDeviceManager.IsFullScreen; } }
        public bool IsMouseVisible { get { return Core.IsMouseVisible; } set { Core.IsMouseVisible = value; } }
        public bool AllowResize { get { return Core.Window.AllowUserResizing; } set { Core.Window.AllowUserResizing = value; } }
        public bool HasFocus { get { return Core.IsActive; } }
        public bool HardwareModeSwitch { get { return Core.GraphicsDeviceManager.HardwareModeSwitch; } set { Core.GraphicsDeviceManager.HardwareModeSwitch = value; } }
        public bool IsRunningSlowly { get; private set; }
        public string ContentDirectory { get { return Core.Content.RootDirectory; } set { Core.Content.RootDirectory = value; } }
        public int LastUpdateDeltaTime { get; private set; }
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
        public Font StdFont { get; private set; }
        public Color BackgroundColor { get { return new Color(XNABackgroundColor.R, XNABackgroundColor.G, XNABackgroundColor.B, XNABackgroundColor.A); } set { XNABackgroundColor = value; } }
        public Renderer MainRenderer { get; private set; }
        public Renderer DebugRenderer { get; private set; }
        public Canvas MainCanvas { get; private set; }
        public System.TimeSpan Time { get; private set; }

        public string Title {
            get {
                return _title;
            }

            set {
                _title = value;
#if DEBUG
                Core.Window.Title = string.Format(WindowTitleDetailed, _title, _fps, System.GC.GetTotalMemory(false) / 1048576f);
#else
                Core.Window.Title = _title;
#endif
            }
        }

        public int UnitToPixels {
            get {
                return MainRenderer != null ? MainRenderer.UnitToPixels : _unitsToPixel;
            }

            set {
                _unitsToPixel = value;

                if (!IsRunning) {
                    return;
                }

                MainRenderer.UnitToPixels = DebugRenderer.UnitToPixels = _unitsToPixel;
            }
        }

        public float PixelScale {
            get {
                return MainRenderer != null ? MainRenderer.PixelScale : _pixelScale;
            }

            set {
                _pixelScale = value;

                Size = WindowSize / _pixelScale;
                Center = (Size / 2f).ToVector2();

                if (!IsRunning) {
                    return;
                }

                MainRenderer.PixelScale = _pixelScale;
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

        internal XNAGameWrapper Core { get; set; }
        internal GraphicsDevice GraphicsDevice { get { return Core.GraphicsDevice; } }
        internal SpriteBatch MainSpriteBatch { get; private set; }
        internal BasicEffect BasicEffect { get; private set; }
        internal Canvas DebugCanvas { get; private set; }
        internal List<Renderer> Surfaces { get; private set; } = new List<Renderer>();
        internal Microsoft.Xna.Framework.Color XNABackgroundColor { get; private set; }
        internal Stack<RenderTarget2D> RenderTargetStack { get; private set; } = new Stack<RenderTarget2D>();
        internal ResourceContentManager AdditionalResourceContentManager { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        public void Start() {
            Debug.Info("| Raccoon Started |");
            IsRunning = true;
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
            Debug.WriteLine("Exiting...");
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
            if (!_scenes.TryGetValue(name, out Scene scene)) {
                throw new System.ArgumentException($"Scene '{name}' not found", "name");
            }

            NextScene = scene;
            return NextScene;
        }

        public T SwitchScene<T>() where T : Scene {
            return SwitchScene(typeof(T).Name.Replace("Scene", "")) as T;
        }

        public void AddSurface(Renderer surface) {
            if (Surfaces.Contains(surface)) {
                return;
            }

            Surfaces.Add(surface);
        }

        public void RemoveSurface(Renderer surface) {
            Surfaces.Remove(surface);
        }

        public void ClearSurfaces() {
            Surfaces.Clear();
        }

        public void ResizeWindow(int width, int height) {
            Debug.Assert(width > 0 && height > 0, $"Width and Height can't be zero or smaller (width: {width}, height: {height})");

            var displayMode = Core.GraphicsDevice.DisplayMode;

            Core.GraphicsDeviceManager.PreferredBackBufferWidth = (int) Math.Clamp(width, WindowMinimunSize.Width, displayMode.Width);
            Core.GraphicsDeviceManager.PreferredBackBufferHeight = (int) Math.Clamp(height, WindowMinimunSize.Height, displayMode.Height);
            Core.GraphicsDeviceManager.ApplyChanges();
        }

        public void ResizeWindow(Size size) {
            ResizeWindow((int) size.Width, (int) size.Height);
        }

        public void ToggleFullscreen() {
            bool isSwitchingToFullscreen = !IsFullscreen;

            Size newWindowSize = Size.Empty;
            if (!isSwitchingToFullscreen) {
                Core.GraphicsDeviceManager.ToggleFullScreen();
                newWindowSize = _windowedModeBounds.Size;
                WindowPosition = _windowedModeBounds.Position;
            } else {
                _windowedModeBounds = new Rectangle(WindowPosition, WindowSize);
                var displayMode = Core.GraphicsDevice.DisplayMode;
                newWindowSize = new Size(displayMode.Width, displayMode.Height);
            }

            ResizeWindow(newWindowSize);

            if (isSwitchingToFullscreen) {
                Core.GraphicsDeviceManager.ToggleFullScreen();
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
            Debug.Instance.Initialize();

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
                Debug.DrawString(Camera.Current, new Vector2(WindowWidth - 260, 15), $"Time: {Time.ToString(@"hh\:mm\:ss\.fff")}\n\nDraw calls: {metrics.DrawCount}, Sprites: {metrics.SpriteCount}\nTextures: {metrics.TextureCount}\n\nPhysics:\n  Update Position: {Physics.UpdatePositionExecutionTime}ms\n  Solve Constraints: {Physics.SolveConstraintsExecutionTime}ms\n  Collision Broad Phase (C: {Physics.CollidersBroadPhaseCount}): {Physics.CollisionDetectionBroadPhaseExecutionTime}ms\n  Collision Narrow Phase (C: {Physics.CollidersNarrowPhaseCount}): {Physics.CollisionDetectionNarrowPhaseExecutionTime}ms\n\nScene:\n  Entities: {(Scene == null ? "0" : Scene.EntitiesCount.ToString())}\n  Graphics: {(Scene == null ? "0" : Scene.GraphicsCount.ToString())}");

                // framerate monitor frame
                Debug.DrawRectangle(Camera.Current, _framerateMonitorFrame, Color.White);

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

                    Debug.DrawLine(Camera.Current, previousPos, currentPos, color);
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

            Scene = NextScene;

            if (Scene != null && MainRenderer != null) {
                Scene.Begin();
            }
        }

        private void InternalOnWindowResize(object sender, System.EventArgs e) {
            var windowClientBounds = Core.Window.ClientBounds;

            // checks if preffered backbuffer size is the same as current window size
            if (Core.GraphicsDeviceManager.PreferredBackBufferWidth != windowClientBounds.Width || Core.GraphicsDeviceManager.PreferredBackBufferHeight != windowClientBounds.Height) {
                var displayMode = Core.GraphicsDevice.DisplayMode;
                Core.GraphicsDeviceManager.PreferredBackBufferWidth = (int) Math.Clamp(windowClientBounds.Width, WindowMinimunSize.Width, displayMode.Width);
                Core.GraphicsDeviceManager.PreferredBackBufferHeight = (int) Math.Clamp(windowClientBounds.Height, WindowMinimunSize.Height, displayMode.Height);
                Core.GraphicsDeviceManager.ApplyChanges();
                return;
            }

            WindowSize = new Size(Core.GraphicsDeviceManager.PreferredBackBufferWidth, Core.GraphicsDeviceManager.PreferredBackBufferHeight);
            WindowCenter = (WindowSize / 2f).ToVector2();
            Size = WindowSize / PixelScale;
            Center = (Size / 2f).ToVector2();

            // internal resize
            // surface
            var projection = Microsoft.Xna.Framework.Matrix.CreateOrthographicOffCenter(0f, WindowWidth, WindowHeight, 0f, 0f, 1f); // maybe request to recalculate projection?
            foreach (Renderer surface in Surfaces) {
                surface.Projection = projection;
            }

            // canvas
            RenderTargetStack.Clear();

            // game renderers projection
            float zoom = Camera.Current == null ? 1f : Camera.Current.Zoom;
            float scaleFactor = 1f / (zoom * PixelScale);
            projection = Microsoft.Xna.Framework.Matrix.CreateOrthographicOffCenter(0f, WindowWidth * scaleFactor, WindowHeight * scaleFactor, 0f, 0f, 1f);

#if DEBUG

            if (DebugRenderer != null) {
                DebugRenderer.Projection = projection;
            }

            if (DebugCanvas != null) {
                DebugCanvas.Resize(WindowSize);
                DebugCanvas.ClippingRegion = DebugCanvas.SourceRegion;
                RenderTargetStack.Push(DebugCanvas.XNARenderTarget);
            }
#endif

            if (MainRenderer != null) {
                MainRenderer.Projection = projection;
            }

            if (MainCanvas != null) {
                MainCanvas.Resize(WindowSize);
                MainCanvas.ClippingRegion = MainCanvas.SourceRegion;
                RenderTargetStack.Push(MainCanvas.XNARenderTarget);
            }

            // user callback
            OnWindowResize();
        }

        private void InternalLoadContent() {
            if (Core.GraphicsDeviceManager.IsFullScreen) {
                ResizeWindow(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
            }

            MainSpriteBatch = new SpriteBatch(GraphicsDevice);
            MainCanvas = new Canvas(WindowWidth, WindowHeight, false, Graphics.SurfaceFormat.Color, Graphics.DepthFormat.None, 0, CanvasUsage.PreserveContents);

#if DEBUG
            DebugCanvas = new Canvas(WindowWidth, WindowHeight, false, Graphics.SurfaceFormat.Color, Graphics.DepthFormat.None, 0, CanvasUsage.PreserveContents);
            RenderTargetStack.Push(DebugCanvas.XNARenderTarget);

            float monitorFrameWidth = ((FramerateMonitorValuesCount - 1) * FramerateMonitorDataSpacing) + 1;
            _framerateMonitorFrame = new Rectangle(new Vector2(WindowWidth - 260 - monitorFrameWidth - 32, 15), new Size(monitorFrameWidth, 82));
            for (int i = 0; i < FramerateMonitorValuesCount; i++) {
                FramerateValues.Add(0);
            }
#endif

            RenderTargetStack.Push(MainCanvas.XNARenderTarget);

            DebugRenderer = new Renderer(Graphics.BlendState.AlphaBlend) {
                UnitToPixels = UnitToPixels
            };

            MainRenderer = new Renderer(Graphics.BlendState.AlphaBlend) {
                UnitToPixels = UnitToPixels,
                PixelScale = PixelScale
            };

            // default content
            AdditionalResourceContentManager = new ResourceContentManager(Core.Services, Resource.ResourceManager);
            StdFont = new Font(AdditionalResourceContentManager.Load<SpriteFont>("Zoomy"));
            BasicEffect = new BasicEffect(GraphicsDevice) {
                VertexColorEnabled = true
            };

            Initialize();
        }

        private void InternalUnloadContent() {
            Scene.UnloadContent();
            AdditionalResourceContentManager.Unload();
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
            GraphicsDevice.SetRenderTarget(MainCanvas.XNARenderTarget);
            GraphicsDevice.Clear(XNABackgroundColor);

            MainRenderer.Begin(SpriteSortMode.Immediate, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            foreach (Renderer surface in Instance.Surfaces) {
                surface.Begin(SpriteSortMode.Immediate, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            }
            
            Render();

            MainRenderer.End();
            foreach (Renderer surface in Instance.Surfaces) {
                surface.End();
            }

#if DEBUG
            GraphicsMetrics metrics = GraphicsDevice.Metrics;

            // debug render
            GraphicsDevice.SetRenderTarget(DebugCanvas.XNARenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            DebugRenderer.Begin(SpriteSortMode.Immediate, SamplerState.PointClamp, null, null, null);

            DebugRender(metrics);

            DebugRenderer.End();
#endif

            // draw main render target to screen
            GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.Clear(ClearOptions.Target, Game.Instance.Core.BackgroundColor, 0f, 0);
            GraphicsDevice.Clear(Color.Black);
            MainSpriteBatch.Begin(SpriteSortMode.Immediate, Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            MainSpriteBatch.Draw(MainCanvas.XNARenderTarget, Microsoft.Xna.Framework.Vector2.Zero, Color.White);
#if DEBUG
            MainSpriteBatch.Draw(DebugCanvas.XNARenderTarget, Microsoft.Xna.Framework.Vector2.Zero, Color.White);
#endif
            MainSpriteBatch.End();

            _fpsCount++;
        }

        #endregion Private Methods
    }
}
