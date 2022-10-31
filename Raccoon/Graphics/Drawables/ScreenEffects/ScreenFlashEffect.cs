using Raccoon.Graphics.Primitives;
using Raccoon.Util;
using Raccoon.Util.Tween;

namespace Raccoon.Graphics.ScreenEffects {
    public class ScreenFlashEffect : ScreenEffect {
        #region Private Members

        private RectanglePrimitive _screenRectangleGraphic;

        #endregion Private Members

        #region Constructors

        public ScreenFlashEffect(uint duration, int repeatTimes = 0) : base(duration, repeatTimes, pingpong: false) {
            _screenRectangleGraphic = new RectanglePrimitive(Game.Instance.Size) {
                Renderer = Renderer,
                Filled = true
            };

            Easing = Ease.Linear;
        }

        #endregion Constructors

        #region Public Properties

        public System.Func<float, float> Easing { get; private set; }

        #endregion Public Properties

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            _screenRectangleGraphic.Render(
                _screenRectangleGraphic.Position + position,
                _screenRectangleGraphic.Rotation + rotation,
                _screenRectangleGraphic.Scale * scale,
                _screenRectangleGraphic.Flipped ^ flip,
                _screenRectangleGraphic.Color * color * Opacity,
                _screenRectangleGraphic.Scroll * scroll,
                shader,
                _screenRectangleGraphic.Layer + ConvertLayerDepthToLayer(layerDepth)
            );
        }

        protected override void Reseted() {
            _screenRectangleGraphic.Opacity = Easing(0f);
        }

        protected override void CycleBegin() {
            Reseted();
        }

        protected override void CycleUpdate(int delta) {
            _screenRectangleGraphic.Opacity = Math.Clamp(Easing(1f - Time), 0f, 1f);
            _screenRectangleGraphic.Update(delta);
        }

        protected override void CycleEnd() {
        }

        protected override void Disposed() {
            _screenRectangleGraphic.Dispose();
            _screenRectangleGraphic = null;
        }

        #endregion Protected Methods
    }
}
