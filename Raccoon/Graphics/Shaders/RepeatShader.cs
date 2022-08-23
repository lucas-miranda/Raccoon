using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class RepeatShader : BasicShader {
        #region Private Members

        private float[] _textureAreaClip = new float[4];
        private Vector2 _repeat = Vector2.One;

        #endregion Private Members

        #region Constructors

        public RepeatShader(string filename) : base(filename) {
            Initialize();
        }

        public RepeatShader(Effect repeatEffect) : base(repeatEffect) {
            Initialize();
        }

        public RepeatShader(byte[] repeatEffectCode)
            : base(new Effect(Game.Instance.GraphicsDevice, repeatEffectCode) {
                  Name = "RepeatShader"
              })
        {
            Initialize();
        }

        #endregion Constructors

        #region Public Properties

        public float[] TextureAreaClip {
            get {
                return _textureAreaClip;
            }
        }

        public Vector2 Repeat {
            get {
                return _repeat;
            }

            set {
                _repeat = value;
                RepeatParameter.SetValue(_repeat);
            }
        }

        #endregion Public Properties

        #region Protected Properties

        protected EffectParameter TextureAreaClipParameter { get; private set; }
        protected EffectParameter RepeatParameter { get; private set; }

        #endregion Protected Properties

        #region Public Methods

        public void SetupTextureAreaClip(Rectangle sourceRegion, Rectangle clippingRegion) {
            if (Texture == null) {
                throw new System.InvalidOperationException("Texture is undefined.");
            }

            // x
            _textureAreaClip[0] = (sourceRegion.X + clippingRegion.X) / Texture.Width;

            // y
            _textureAreaClip[1] = (sourceRegion.Y + clippingRegion.Y) / Texture.Height;

            // width
            _textureAreaClip[2] = clippingRegion.Width / Texture.Width;

            // height
            _textureAreaClip[3] = clippingRegion.Height / Texture.Height;

            //

            TextureAreaClipParameter.SetValue(_textureAreaClip);
        }

        public void TextureAreaClipCopyFrom(float[] textureAreaClip) {
            if (textureAreaClip == null) {
                throw new System.ArgumentNullException(nameof(textureAreaClip));
            }

            textureAreaClip.CopyTo(_textureAreaClip, 0);
            TextureAreaClipParameter.SetValue(_textureAreaClip);
        }

        #endregion Public Methods

        #region Private Members

        private void Initialize() {
            TextureAreaClipParameter = RequestParameter("TextureAreaClip");
            RepeatParameter = RequestParameter("Repeat");
        }

        #endregion Private Members
    }
}
