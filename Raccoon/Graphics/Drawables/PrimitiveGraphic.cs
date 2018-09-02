namespace Raccoon.Graphics {
    public abstract class PrimitiveGraphic : Graphic {
        #region Private Members

        private bool _restartRenderer;

        #endregion Private Members

        #region Protected Methods

        protected override void BeforeDraw() {
            base.BeforeDraw();
            
            if (Renderer != null && Renderer.IsBatching) {
                BasicShader bs = Game.Instance.BasicShader;
                _restartRenderer = true;
                bs.View = Renderer.View;
                bs.Projection = Renderer.Projection;
                bs.TextureEnabled = true;
                bs.UpdateParameters();
                Renderer.End();
                bs.ResetParameters();
            }
        }

        protected override void AfterDraw() {
            base.AfterDraw();

            if (_restartRenderer) {
                _restartRenderer = false;
                Renderer.Begin();
            }
        }

        #endregion Protected Methods
    }
}
