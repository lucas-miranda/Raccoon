﻿using Microsoft.Xna.Framework;

namespace Raccoon {
    internal class XNAGameWrapper : Microsoft.Xna.Framework.Game {
        #region Private Members

        private System.Action OnPreLoadContent, OnPostLoadContent, OnUnloadContent;
        private readonly System.Action<GameTime> OnUpdate, OnDraw;

        #endregion Private Members

        #region Constructors

        public XNAGameWrapper(
            int width,
            int height,
            int targetFramerate,
            bool fullscreen,
            bool vsync,
            System.Action onPreLoadContent,
            System.Action onPostLoadContent,
            System.Action onUnloadContent,
            System.Action<GameTime> onUpdate,
            System.Action<GameTime> onDraw
        ) {
            OnPreLoadContent = onPreLoadContent;
            OnPostLoadContent = onPostLoadContent;
            OnUnloadContent = onUnloadContent;
            OnUpdate = onUpdate;
            OnDraw = onDraw;

            TargetElapsedTime = System.TimeSpan.FromTicks((long) System.Math.Round(10000000 / (double) targetFramerate)); // time between frames
            GraphicsDeviceManager = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = width,
                PreferredBackBufferHeight = height,
                IsFullScreen = fullscreen,
                PreferMultiSampling = false,
                SynchronizeWithVerticalRetrace = vsync
            };
        }

        #endregion Constructors

        #region Public Properties

        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

        #endregion Public Properties

        #region Protected Methods

        protected override void Initialize() {
            base.Initialize();
        }

        protected override void LoadContent() {
            if (OnPreLoadContent != null) {
                OnPreLoadContent();
                OnPreLoadContent = null;
            }

            base.LoadContent();

            if (OnPostLoadContent != null) {
                OnPostLoadContent();
                OnPostLoadContent = null;
            }
        }

        protected override void UnloadContent() {
            OnUnloadContent();
            OnUnloadContent = null;
            Content.Unload();
            base.UnloadContent();
        }

        protected override void BeginRun() {
            base.BeginRun();
        }

        protected override void Update(GameTime gameTime) {
            OnUpdate(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            OnDraw(gameTime);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
        }

        #endregion Protected Methods
    }
}
