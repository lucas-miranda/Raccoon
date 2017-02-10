using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon {
    internal class Core : Microsoft.Xna.Framework.Game {
        #region Public Events

        public event Action OnBegin, OnExit, OnUnloadContent, OnBeforeUpdate, OnLateUpdate, OnRender, OnDebugRender;
        public event Game.TickHandler OnUpdate;

        #endregion Public Events

        #region Private Members

        private string _windowTitleDetailed = "{0} | {1} FPS {2:0.00} MB";
        private int _fpsCount, _fps;
        private float _scale = 1f;
        private TimeSpan _lastFpsTime;

        #endregion Private Members

        #region Constructor

        public Core(string title, int width, int height, int targetFPS, bool fullscreen, bool vsync) {
            Title = title;

#if DEBUG
            Window.Title = string.Format(_windowTitleDetailed, Title, 0, GC.GetTotalMemory(false) / 1048576f);
#else
            Window.Title = Title;
#endif

            Content.RootDirectory = "Content/";
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
        public Graphics.Surface DefaultSurface { get; private set; }
        public Graphics.Surface DebugSurface { get; private set; }
        public Graphics.Font StdFont { get; private set; }
        public TimeSpan Time { get; private set; }
        public int DeltaTime { get; private set; }
        public Color BackgroundColor { get; set; }
        public string Title { get; set; }
        public BasicEffect BasicEffect { get; private set; }
        public SpriteBatch MainSpriteBatch { get; private set; }
        public RenderTarget2D MainRenderTarget { get; private set; }
        public RenderTarget2D SecondaryRenderTarget { get; private set; }
        public Stack<RenderTarget2D> RenderTargetStack { get; private set; } = new Stack<RenderTarget2D>();

        public float Scale {
            get {
                return _scale;
            }

            set {
                _scale = value;
                if (DefaultSurface != null) {
                    DefaultSurface.Scale = new Vector2(_scale) * (Camera.Current != null ? Camera.Current.Zoom : 1f);
                }
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
            MainSpriteBatch = new SpriteBatch(GraphicsDevice);
            MainRenderTarget = new RenderTarget2D(GraphicsDevice, Game.Instance.WindowWidth, Game.Instance.WindowHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            SecondaryRenderTarget = new RenderTarget2D(GraphicsDevice, Game.Instance.WindowWidth, Game.Instance.WindowHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            RenderTargetStack.Push(MainRenderTarget);

            DebugSurface = new Graphics.Surface();
            DefaultSurface = new Graphics.Surface() {
                Scale = new Vector2(_scale) * (Camera.Current != null ? Camera.Current.Zoom : 1f)
            };

            // default content
            ResourceContentManager resourceContentManager = new ResourceContentManager(Services, Resource.ResourceManager);
            StdFont = new Graphics.Font(resourceContentManager.Load<SpriteFont>("Zoomy"));
            BasicEffect = new BasicEffect(GraphicsDevice) {
                VertexColorEnabled = true
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
            base.Draw(gameTime);

            // game render
            GraphicsDevice.SetRenderTarget(MainRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            DefaultSurface.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            foreach (Graphics.Surface surface in Game.Instance.Surfaces) {
                surface.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            }
            
            OnRender.Invoke();

            DefaultSurface.End();
            foreach (Graphics.Surface surface in Game.Instance.Surfaces) {
                surface.End();
            }

#if DEBUG
            GraphicsMetrics metrics = GraphicsDevice.Metrics;

            // debug render
            DebugSurface.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            
            if (Debug.ShowPerformanceDiagnostics) {
                Debug.DrawString(false, new Vector2(Graphics.PreferredBackBufferWidth - 200, 15), "Time: {0}\n\nDraw calls: {1}, Sprites: {2}\nTextures: {3}", Time.ToString(@"hh\:mm\:ss\.fff"), metrics.DrawCount, metrics.SpriteCount, metrics.TextureCount);
            }

            if (Game.Instance.DebugMode) {
                OnDebugRender.Invoke();
            }

            DebugSurface.End();
#endif

            // draw main render target to screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target, Game.Instance.Core.BackgroundColor, 1f, 0);
            MainSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            MainSpriteBatch.Draw(MainRenderTarget, Microsoft.Xna.Framework.Vector2.Zero);
            MainSpriteBatch.End();
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
