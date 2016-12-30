using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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

        #region Public Events

        public event Action OnBegin, OnExit, OnUnloadContent, OnBeforeUpdate, OnLateUpdate, OnRender, OnDebugRender;
        public event Game.TickHandler OnUpdate;

        #endregion Public Events

        #region Constructor

        public Core(string title, int width, int height, int targetFPS, bool fullscreen, bool vsync) {
            Title = title;

#if DEBUG
            Window.Title = string.Format(_windowTitleDetailed, Title, 0, GC.GetTotalMemory(false) / 1048576f);
#else
            Window.Title = Title;
#endif

            Content.RootDirectory = "Content";
            TargetElapsedTime = TimeSpan.FromTicks((long) Math.Round(10000000 / (double) targetFPS)); // time between frames
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
        public Matrix ScreenTransform { get { return _screenTransform; } set { _screenTransform = BasicEffect.View = value; } }
        public Matrix ScreenDebugTransform { get { return _debugScreenTransform; } set { _debugScreenTransform = value; } }
        public BasicEffect BasicEffect { get; set; }

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
            BasicEffect = new BasicEffect(GraphicsDevice) {
                VertexColorEnabled = true,
                World = Matrix.CreateLookAt(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), Vector3.Up),
                Projection = Matrix.CreateOrthographicOffCenter(0, Game.Instance.ScreenWidth, Game.Instance.ScreenHeight, 0, 1f, 0f)
            };

            OnUnloadContent += resourceContentManager.Unload;

            // scene OnBegin
            OnBegin.Invoke();
            OnBegin = null;

            Util.Tween.Tweener.Instance.Start();
            base.LoadContent();
        }

        protected override void UnloadContent() {
            Raccoon.Graphics.Texture.White.Dispose();
            Raccoon.Graphics.Texture.Black.Dispose();
            OnUnloadContent.Invoke();
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
            OnBeforeUpdate.Invoke();
            OnUpdate.Invoke(delta);
            Coroutine.Instance.Update(delta);
            OnLateUpdate.Invoke();
            Util.Tween.Tweener.Instance.Update(delta);
            
            // fps
            _fpsCount++;
            if (Time.Subtract(_lastFpsTime).Seconds >= 1) {
                _lastFpsTime = Time;
                _fps = _fpsCount;
                _fpsCount = 0;
#if DEBUG
                Window.Title = string.Format(_windowTitleDetailed, Title, _fps, GC.GetTotalMemory(false) / 1048576f);
#endif
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            // game render
            GraphicsDevice.SetRenderTarget(_mainRenderTarget);
            Graphics.GraphicsDevice.Clear(BackgroundColor);
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null, ScreenTransform);
            OnRender.Invoke();
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
                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, ScreenDebugTransform);
                OnDebugRender.Invoke();
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
