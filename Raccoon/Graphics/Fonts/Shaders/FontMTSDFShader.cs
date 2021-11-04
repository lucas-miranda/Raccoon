using Microsoft.Xna.Framework.Graphics;
using Raccoon.Graphics;

namespace Raccoon.Fonts {
    public class FontMTSDFShader : BasicShader {
        private float _screenPixelRange;

        public FontMTSDFShader(string filename) : base(filename) {
        }

        public FontMTSDFShader(byte[] bytecode)
            : base(
                new Microsoft.Xna.Framework.Graphics.Effect(
                    Game.Instance.GraphicsDevice,
                    bytecode
                ) {
                    Name = "FontMTSDFShader"
                }
            )
        {
        }

        [ShaderParameter]
        public float ScreenPixelRange {
            get {
                return _screenPixelRange;
            }

            set {
                if (value == _screenPixelRange) {
                    return;
                }

                _screenPixelRange = value;

                if (TryGetParameter(
                    "ScreenPixelRange",
                    out EffectParameter parameter
                )) {
                    parameter.SetValue(_screenPixelRange);
                }
            }
        }

        protected override void OnApply() {
            DepthWriteEnabled = true;
            TextureEnabled = true;
            base.OnApply();
        }
    }
}
