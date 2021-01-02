namespace Raccoon.Graphics.ScreenEffects {
    public class GraphicScreenFadeEffect : ScreenFadeEffect {
        #region Constructors

        public GraphicScreenFadeEffect(FadeTarget fade, uint cycleDuration, int repeatTimes = 0, bool pingpong = false) : base(fade, cycleDuration, repeatTimes, pingpong) {
        }

        public GraphicScreenFadeEffect(Graphic graphic, FadeTarget fade, uint cycleDuration, int repeatTimes = 0, bool pingpong = false) : base(fade, cycleDuration, repeatTimes, pingpong) {
            Graphic = graphic;
        }

        #endregion Constructors

        #region Public Properties

        public Graphic Graphic { get; set; }

        #endregion Public Properties

        #region Public Methods

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            Graphic.Render(
                Graphic.Position + position, 
                Graphic.Rotation + rotation, 
                Graphic.Scale * scale, 
                Graphic.Flipped ^ flip, 
                Graphic.Color * color * Opacity,
                Graphic.Scroll * scroll, 
                shader, 
                Graphic.Layer + ConvertLayerDepthToLayer(layerDepth)
            );
        }

        /*
        protected override void BeginFadeIn() {
        }

        protected override void EndFadeIn() {
        }

        protected override void BeginFadeOut() {
        }

        protected override void EndFadeOut() {
        }
        */

        protected override void Reseted() {
            switch (Fade) {
                case FadeTarget.In:
                    Graphic.Opacity = 0f;
                    break;

                case FadeTarget.Out:
                    Graphic.Opacity = 1f;
                    break;

                default:
                    throw new System.NotImplementedException($"Fade: {Fade}");
            }
        }

        protected override void CycleBegin() {
            Reseted();
        }

        protected override void CycleUpdate(int delta) {
            Graphic.Opacity = Time;
            Graphic.Update(delta);
        }

        protected override void CycleEnd() {
        }

        protected override void Disposed() {
            Graphic = null;
        }

        #endregion Protected Methods
    }
}
