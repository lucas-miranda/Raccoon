using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon {
    internal class Core : Microsoft.Xna.Framework.Game {
        #region Public Events

        public event Action OnBegin, OnExit, OnUnloadContent, OnBeforeUpdate, OnLateUpdate, OnRender, OnDebugRender;
        public event Action<int> OnUpdate;

        #endregion Public Events

        #region Private Members

        private string _windowTitleDetailed = "{0} | {1} FPS {2:0.00} MB";
        private int _fpsCount, _fps;
        private float _scale = 1f;
        private TimeSpan _lastFpsTime;

        #endregion Private Members

        #region Constructor

        public Core(string title, int width, int height, int targetFramerate, bool fullscreen, bool vsync) {
            Title = title;

#if DEBUG
            Window.Title = string.Format(_windowTitleDetailed, Title, 0, GC.GetTotalMemory(false) / 1048576f);
#else
            Window.Title = Title;
#endif

            Content.RootDirectory = "Content/";
            TargetElapsedTime = TimeSpan.FromTicks((long) Math.Round(10000000 / (double) targetFramerate)); // time between frames
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
        public Graphics.Surface MainSurface { get; private set; }
        public Graphics.Surface DebugSurface { get; private set; }
        public Graphics.Font StdFont { get; private set; }
        public TimeSpan Time { get; private set; }
        public int DeltaTime { get; private set; }
        public Color BackgroundColor { get; set; }
        public string Title { get; set; }
        public BasicEffect BasicEffect { get; private set; }
        public SpriteBatch MainSpriteBatch { get; private set; }
        public Stack<RenderTarget2D> RenderTargetStack { get; private set; } = new Stack<RenderTarget2D>();
        public Graphics.Canvas MainCanvas { get; private set; }

#if DEBUG
        public Graphics.Canvas DebugCanvas { get; private set; }
#endif

        public float Scale {
            get {
                return _scale;
            }

            set {
                _scale = value;
                if (MainSurface != null) {
                    MainSurface.Scale = new Vector2(_scale) * (Camera.Current != null ? Camera.Current.Zoom : 1f);
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
            MainCanvas = new Graphics.Canvas(Game.Instance.WindowWidth, Game.Instance.WindowHeight, false, Raccoon.Graphics.SurfaceFormat.Color, Raccoon.Graphics.DepthFormat.None, 0, Raccoon.Graphics.CanvasUsage.PreserveContents);

#if DEBUG
            DebugCanvas = new Graphics.Canvas(Game.Instance.WindowWidth, Game.Instance.WindowHeight, false, Raccoon.Graphics.SurfaceFormat.Color, Raccoon.Graphics.DepthFormat.None, 0, Raccoon.Graphics.CanvasUsage.PreserveContents);
            RenderTargetStack.Push(DebugCanvas.XNARenderTarget);
#endif

            RenderTargetStack.Push(MainCanvas.XNARenderTarget);

            DebugSurface = new Graphics.Surface(Raccoon.Graphics.BlendState.AlphaBlend);
            MainSurface = new Graphics.Surface(Raccoon.Graphics.BlendState.AlphaBlend) {
                Scale = new Vector2(_scale) * (Camera.Current != null ? Camera.Current.Zoom : 1f)
            };

            // default content
            ResourceContentManager resourceContentManager = new ResourceContentManager(Services, Resource.ResourceManager);
            StdFont = new Graphics.Font(resourceContentManager.Load<SpriteFont>("Zoomy"));
            BasicEffect = new BasicEffect(GraphicsDevice) {
                VertexColorEnabled = true
            };

            OnUnloadContent += resourceContentManager.Unload;

            // systems initialization
            Debug.Instance.Initialize();

            // scene OnBegin
            OnBegin();
            OnBegin = null;

            Util.Tween.Tweener.Instance.Start();
            base.LoadContent();
        }

        protected override void UnloadContent() {
            Raccoon.Graphics.Texture.White.Dispose();
            Raccoon.Graphics.Texture.Black.Dispose();
            OnUnloadContent();
            OnUnloadContent = null;
            Content.Unload();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {
            Time = gameTime.TotalGameTime;

            int delta = gameTime.ElapsedGameTime.Milliseconds;
            DeltaTime = delta;

            // updates
            Input.Input.Instance.Update(delta);
            Util.Tween.Tweener.Instance.Update(delta);
            OnBeforeUpdate();
            OnUpdate(delta);
            OnLateUpdate();
            Coroutines.Instance.Update(delta);

#if DEBUG
            Debug.Instance.Update(delta);
#endif

            // fps
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
            GraphicsDevice.SetRenderTarget(MainCanvas.XNARenderTarget);
            GraphicsDevice.Clear(Game.Instance.Core.BackgroundColor);

            MainSurface.Begin(SpriteSortMode.Immediate, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            foreach (Graphics.Surface surface in Game.Instance.Surfaces) {
                surface.Begin(SpriteSortMode.Immediate, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            }
            
            OnRender();

            MainSurface.End();
            foreach (Graphics.Surface surface in Game.Instance.Surfaces) {
                surface.End();
            }

#if DEBUG
            GraphicsMetrics metrics = GraphicsDevice.Metrics;

            MainSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, MainCanvas.Shader?.XNAEffect);
            MainSpriteBatch.Draw(MainCanvas.XNARenderTarget, Microsoft.Xna.Framework.Vector2.Zero, Color.White);
            MainSpriteBatch.End();

            // debug render
            GraphicsDevice.SetRenderTarget(DebugCanvas.XNARenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            DebugSurface.Begin(SpriteSortMode.Immediate, SamplerState.PointClamp, null, null, null);
            
            if (Game.Instance.DebugMode) {
                OnDebugRender();
            }

            Debug.Instance.Render();

            if (Debug.ShowPerformanceDiagnostics) {
                Debug.DrawString(Camera.Current, new Vector2(Game.Instance.WindowWidth - 200, 15), $"Time: {Time.ToString(@"hh\:mm\:ss\.fff")}\n\nDraw calls: {metrics.DrawCount}, Sprites: {metrics.SpriteCount}\nTextures: {metrics.TextureCount}");
            }

            DebugSurface.End();
#endif

            // draw main render target to screen
            GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.Clear(ClearOptions.Target, Game.Instance.Core.BackgroundColor, 0f, 0);
            GraphicsDevice.Clear(Color.Black);
            MainSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            MainSpriteBatch.Draw(MainCanvas.XNARenderTarget, Microsoft.Xna.Framework.Vector2.Zero, Color.White);
#if DEBUG
            MainSpriteBatch.Draw(DebugCanvas.XNARenderTarget, Microsoft.Xna.Framework.Vector2.Zero, Color.White);
#endif
            MainSpriteBatch.End();

            _fpsCount++;
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
