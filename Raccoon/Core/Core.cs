using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Raccoon {
    internal class Core : Microsoft.Xna.Framework.Game {
        #region Private Members

        private string _windowTitleDetailed = "{0} | {1} FPS {2:0.00} MB";
        private Matrix _screenTransform = Matrix.Identity, _debugScreenTransform = Matrix.Identity, _scaleTransform = Matrix.Identity;
        private int _fpsCount, _fps;
        private float _scale;
        private TimeSpan _lastFpsTime;
        private RenderTarget2D _mainRenderTarget;

        #endregion Private Members

        #region Public Delegates

        public delegate void TickHandler(int delta);

        #endregion Public Delegates

        #region Public Events

        public event Action OnBegin, OnExit, OnUnloadContent, OnBeforeUpdate, OnLateUpdate, OnRender, OnDebugRender;
        public event TickHandler OnUpdate;

        #endregion Public Events

        #region Constructor

        public Core(string title, int width, int height, int targetFPS, bool fullscreen, bool vsync) {
            Title = title;
            Window.Title = string.Format(_windowTitleDetailed, Title, 0, GC.GetTotalMemory(false) / 1048576f);
            Content.RootDirectory = "Content";
            TargetElapsedTime = TimeSpan.FromTicks((long) System.Math.Round(10000000 / (double) targetFPS)); // time between frames
            Scale = 1f;
            BackgroundColor = Color.Black;

            Graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = width,
                PreferredBackBufferHeight = height,
                IsFullScreen = fullscreen,
                PreferMultiSampling = false,
                SynchronizeWithVerticalRetrace = vsync
            };
        }

        #endregion Constructor

        #region Public Properties

        public GraphicsDeviceManager Graphics { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public Graphics.Font StdFont { get; private set; }
        public TimeSpan Time { get; private set; }
        public int DeltaTime { get; private set; }
        public Color BackgroundColor { get; set; }
        public string Title { get; set; }
        public Matrix ScreenTransform { get { return _screenTransform; } set { _screenTransform = value; } }
        public Matrix ScreenDebugTransform { get { return _debugScreenTransform; } set { _debugScreenTransform = value; } }

        public float Scale {
            get {
                return _scale;
            }

            set {
                _scale = value;
                _scaleTransform = Matrix.CreateScale(_scale, _scale, 1);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void ClearCallbacks() {
            OnBegin = OnExit = OnUnloadContent = OnBeforeUpdate = OnLateUpdate = OnRender = OnDebugRender = null;
            OnUpdate = null;
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Initialize() {
            base.Initialize();
        }

        protected override void LoadContent() {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            _mainRenderTarget = new RenderTarget2D(GraphicsDevice, Game.Instance.ScreenWidth, Game.Instance.ScreenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            // default content
            ResourceContentManager resourceContentManager = new ResourceContentManager(Services, Resource.ResourceManager);
            StdFont = new Graphics.Font(resourceContentManager.Load<SpriteFont>("Zoomy"));
            OnUnloadContent += resourceContentManager.Unload;

            OnBegin?.Invoke();
            OnBegin = null;

            Util.Tween.Tweener.Instance.Start();
            base.LoadContent();
        }

        protected override void UnloadContent() {
            Raccoon.Graphics.Texture.White.Dispose();
            Raccoon.Graphics.Texture.Black.Dispose();
            OnUnloadContent?.Invoke();
            OnUnloadContent = null;
            Content.Unload();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {
            Time = gameTime.TotalGameTime;

#if DEBUG
            if (Input.IsKeyPressed(Key.Escape)) {
                Exit();
            }
#endif

            int delta = gameTime.ElapsedGameTime.Milliseconds;
            DeltaTime = delta;

            // updates
            Input.Instance.Update(delta);
            Coroutine.Instance.Update(delta);
            OnBeforeUpdate?.Invoke();
            OnUpdate?.Invoke(delta);
            OnLateUpdate?.Invoke();
            Util.Tween.Tweener.Instance.Update(delta);
            
            // fps
            _fpsCount++;
            if (Time.Subtract(_lastFpsTime).Seconds >= 1) {
                _lastFpsTime = Time;
                _fps = _fpsCount;
                _fpsCount = 0;
                Window.Title = string.Format(_windowTitleDetailed, Title, _fps, GC.GetTotalMemory(false) / 1048576f);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            // game render
            GraphicsDevice.SetRenderTarget(_mainRenderTarget);
            Graphics.GraphicsDevice.Clear(BackgroundColor);
            SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, ScreenTransform);
            OnRender?.Invoke();
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

#if DEBUG
            GraphicsMetrics metrics = GraphicsDevice.Metrics;
#endif

            // draw main render target to screen
            Graphics.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null, null, _scaleTransform);
            SpriteBatch.Draw(_mainRenderTarget, Microsoft.Xna.Framework.Vector2.Zero);
            SpriteBatch.End();

#if DEBUG
            // debug render
            if (Game.Instance.DebugMode) {
                SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, ScreenDebugTransform);
                OnDebugRender?.Invoke();
                Debug.DrawString(false, new Vector2(Graphics.PreferredBackBufferWidth - 200, 15), "Time: {0}\n\nDraw calls: {1}, Sprites: {2}\nTextures: {3}", Time.ToString(@"hh\:mm\:ss\.fff"), metrics.DrawCount, metrics.SpriteCount, metrics.TextureCount);
                SpriteBatch.End();
            }
#endif

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing) {
                Graphics.Dispose();
            }
        }

        #endregion Protected Methods
    }
}
