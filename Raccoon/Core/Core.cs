using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Raccoon {
    internal class Core : Microsoft.Xna.Framework.Game {
        #region Private Members

        private Matrix _screenTransform = Matrix.Identity;
        private int _fpsCount, _fps;
        private TimeSpan _lastFpsTime;

        #endregion Private Members

        #region Public Delegates

        public delegate void GeneralHandler();
        public delegate void TickHandler(int delta);

        #endregion Public Delegates

        #region Public Events

        public event GeneralHandler OnInitialize;
        public event GeneralHandler OnLoadContent;
        public event GeneralHandler OnUnloadContent;
        public event GeneralHandler OnRender;
        public event TickHandler OnUpdate;

        #endregion Public Events

        #region Constructor

        public Core(string title, int width, int height, int targetFPS, bool fullscreen) {
            Title = title;
            Window.Title = Title + " | FPS: 0";
            Content.RootDirectory = "Content";
            TargetElapsedTime = TimeSpan.FromTicks((long) System.Math.Round(10000000 / (double) targetFPS)); // time between frames
            Scale = 1f;
            BackgroundColor = Color.Black;

            Graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = width,
                PreferredBackBufferHeight = height,
                IsFullScreen = fullscreen,
                PreferMultiSampling = false
            };
        }

        #endregion Constructor

        #region Public Properties

        public GraphicsDeviceManager Graphics { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public Graphics.Font StdFont { get; private set; }
        public TimeSpan Time { get; private set; }
        public int DeltaTime { get; private set; }
        public float Scale { get; set; }
        public Color BackgroundColor { get; set; }
        public string Title { get; set; }

        #endregion Public Properties

        #region Protected Methods

        protected override void Initialize() {
            Debug.WriteLine("Initializing... ");
            Matrix.CreateScale(Scale, Scale, 1, out _screenTransform);
            OnInitialize?.Invoke();
            OnInitialize = null;
            base.Initialize();
        }

        protected override void LoadContent() {
            Debug.WriteLine("Loading Content... ");
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // default content
            ResourceContentManager resourceContentManager = new ResourceContentManager(Services, Resource.ResourceManager);
            StdFont = new Graphics.Font(resourceContentManager.Load<SpriteFont>("Zoomy"));
            OnUnloadContent += () => resourceContentManager.Unload();

            //effect = Content.Load<Effect>("Test");
            //effect.CurrentTechnique = effect.Techniques["BasicColorDrawing"];

            OnLoadContent?.Invoke();
            OnLoadContent = null;
            base.LoadContent();
        }

        protected override void UnloadContent() {
            Debug.WriteLine("Unloading Content... ");
            OnUnloadContent?.Invoke();
            OnUnloadContent = null;
            SpriteBatch.BlankTexture().Dispose();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {
            Time = gameTime.TotalGameTime;
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            int delta = gameTime.ElapsedGameTime.Milliseconds;
            DeltaTime = delta;

            // updates
            Input.Mouse.Instance.Update(delta);
            Coroutine.Instance.Update(delta);
            OnUpdate?.Invoke(delta);
            
            // fps
            _fpsCount++;
            if (Time.Subtract(_lastFpsTime).Seconds >= 1) {
                _lastFpsTime = Time;
                _fps = _fpsCount;
                _fpsCount = 0;
                Window.Title = Title + " | FPS: " + _fps;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            Graphics.GraphicsDevice.Clear(BackgroundColor);
            SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _screenTransform);
            OnRender?.Invoke();

#if DEBUG
            SpriteBatch.End();
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            GraphicsMetrics metrics = GraphicsDevice.Metrics;
            SpriteBatch.DrawString(StdFont.SpriteFont, $"Time: {Time.ToString(@"hh\:mm\:ss\.fff")}\n\nDraw calls: {metrics.DrawCount}, Sprites: {metrics.SpriteCount}\nTextures: {metrics.TextureCount}", new Vector2(3, 2), Raccoon.Graphics.Color.White);
#endif

            SpriteBatch.End();
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
