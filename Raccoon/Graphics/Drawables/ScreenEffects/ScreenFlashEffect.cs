using Raccoon.Graphics.Primitives;
using Raccoon.Util;

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
        }

        #endregion Constructors

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            _screenRectangleGraphic.Render(
                Position + position, 
                Rotation + rotation, 
                Scale * scale, 
                Flipped ^ flip, 
                Color * color * Opacity,
                Scroll * scroll, 
                shader, 
                ConvertLayerDepthToLayer(layerDepth)
            );
        }

        protected override void Reseted() {
            _screenRectangleGraphic.Opacity = 1f;
        }

        protected override void CycleBegin() {
            Reseted();
        }

        protected override void CycleUpdate(int delta) {
            Time = Math.Max(1f - Time, 0f);
            _screenRectangleGraphic.Opacity = Time;
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
