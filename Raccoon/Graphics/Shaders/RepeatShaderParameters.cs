
namespace Raccoon.Graphics {
    public class RepeatShaderParameters : IShaderParameters {
        public RepeatShaderParameters() {
        }

        public float[] TextureAreaClip { get; set; }
        public Vector2 Repeat { get; set; } = Vector2.One;

        public void SetupTextureAreaClip(Size textureSize, Rectangle sourceRegion, Rectangle clippingRegion) {
            if (TextureAreaClip == null || TextureAreaClip.Length < 4) {
                TextureAreaClip = new float[4];
            }

            // x
            TextureAreaClip[0] = (sourceRegion.X + clippingRegion.X) / textureSize.Width;

            // y
            TextureAreaClip[1] = (sourceRegion.Y + clippingRegion.Y) / textureSize.Height;

            // width
            TextureAreaClip[2] = clippingRegion.Width / textureSize.Width;

            // height
            TextureAreaClip[3] = clippingRegion.Height / textureSize.Height;
        }

        public void ApplyParameters(Shader shader) {
            if (!(shader is RepeatShader repeatShader)) {
                throw new System.ArgumentException($"Expected '{nameof(RepeatShader)}', but got '{shader.GetType().ToString()}' instead.");
            }

            if (TextureAreaClip != null) {
                repeatShader.TextureAreaClipCopyFrom(TextureAreaClip);
            }

            repeatShader.Repeat = Repeat;
        }

        /// <summary>
        /// Performs a shallow copy of contents.
        /// </summary>
        public IShaderParameters Clone() {
            return new RepeatShaderParameters() {
                TextureAreaClip = TextureAreaClip,
                Repeat = Repeat,
            };
        }

        public bool Equals(IShaderParameters other) {
            if (other == null
             || !(other is RepeatShaderParameters o)
             || o.Repeat != Repeat
            ) {
                return false;
            }

            if (TextureAreaClip != null) {
                if (o.TextureAreaClip == null
                 || TextureAreaClip.Length != o.TextureAreaClip.Length
                ) {
                    return false;
                }

                for (int i = 0; i < TextureAreaClip.Length; i++) {
                    if (TextureAreaClip[i] != o.TextureAreaClip[i]) {
                        return false;
                    }
                }
            } else if (o.TextureAreaClip != null) {
                return false;
            }

            return true;
        }
    }
}
