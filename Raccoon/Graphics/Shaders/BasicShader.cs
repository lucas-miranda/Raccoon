using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    public class BasicShader : Shader, IBasicShader {
        #region Public Members

        [System.Flags]
        public enum DirtyFlags {
            None                    = 0,
            TechniqueIndex          = 1 << 0,
            WorldViewProjection     = 1 << 1,
            MaterialColor           = 1 << 2
        }

        #endregion Public Members

        #region Private Members

        private Matrix _world, _view, _projection;
        private Color _diffuseColor;
        private float _alpha;
        private bool _textureEnabled;
        private Texture _texture;

        private BitTag _dirtyFlags = DirtyFlags.None;

        #endregion Private Members

        #region Constructors

        public BasicShader(string filename) : base(filename) {
            WorldViewProjectionParameter = XNAEffect.Parameters["WorldViewProj"];
            DiffuseColorParameter = XNAEffect.Parameters["DiffuseColor"];
            TextureParameter = XNAEffect.Parameters["Texture"];
            ResetParameters();
        }

        public BasicShader(Effect basicEffect) : base(basicEffect) {
            WorldViewProjectionParameter = XNAEffect.Parameters["WorldViewProj"];
            DiffuseColorParameter = XNAEffect.Parameters["DiffuseColor"];
            TextureParameter = XNAEffect.Parameters["Texture"];
            ResetParameters();
        }
        public BasicShader(byte[] basicEffectCode) : this(new Effect(Game.Instance.GraphicsDevice, basicEffectCode) { Name = "BasicEffect" }) {
        }

        #endregion Constructors

        #region Public Properties

        public Matrix World {
            get {
                return _world;
            }

            set {
                _world = value;
                _dirtyFlags |= DirtyFlags.WorldViewProjection;
            }
        }

        public Matrix View {
            get {
                return _view;
            }

            set {
                _view = value;
                _dirtyFlags |= DirtyFlags.WorldViewProjection;
            }
        }

        public Matrix Projection {
            get {
                return _projection;
            }

            set {
                _projection = value;
                _dirtyFlags |= DirtyFlags.WorldViewProjection;
            }
        }

        public Color DiffuseColor {
            get {
                return _diffuseColor;
            }

            set {
                _diffuseColor = value;
                _dirtyFlags |= DirtyFlags.MaterialColor;
            }
        }

        public float Alpha {
            get {
                return _alpha;
            }

            set {
                _alpha = value;
                _dirtyFlags |= DirtyFlags.MaterialColor;
            }
        }

        public bool TextureEnabled {
            get {
                return _textureEnabled;
            }

            set {
                if (value == _textureEnabled) {
                    return;
                }

                _textureEnabled = value;
                _dirtyFlags |= DirtyFlags.TechniqueIndex;
            }
        }

        public Texture Texture {
            get {
                return _texture;
            }

            set {
                _texture = value;

                if (TextureParameter == null) {
                    return;
                }

                if (_texture == null) {
                    TextureParameter.SetValue((Texture2D) null);
                    return;
                }

                TextureParameter.SetValue(_texture.XNATexture);
            }
        }

        #endregion Public Properties

        #region Internal Properties

        protected internal EffectParameter WorldViewProjectionParameter { get; set; }
        protected internal EffectParameter DiffuseColorParameter { get; set; }
        protected internal EffectParameter TextureParameter { get; set; }

        #endregion Internal Properties

        #region Public Methods

        public void SetTransforms(ref Matrix world, ref Matrix view, ref Matrix projection) {
            World = world;
            View = view;
            Projection = projection;
        }

        public void SetMaterial(Color diffuseColor, float alpha) {
            DiffuseColor = diffuseColor;
            Alpha = alpha;
        }

        public void SetMaterial(Color diffuseColor) {
            DiffuseColor = new Color(diffuseColor, 1f);
            Alpha = diffuseColor.A / 255f;
        }

        public void UpdateParameters() {
            if (_dirtyFlags.Has(DirtyFlags.WorldViewProjection)) {
                Matrix.Multiply(ref _world, ref _view, out Matrix worldView);
                Matrix.Multiply(ref worldView, ref _projection, out Matrix worldViewProjection);
                WorldViewProjectionParameter.SetValue(worldViewProjection);

                _dirtyFlags -= DirtyFlags.WorldViewProjection;
            }

            if (_dirtyFlags.Has(DirtyFlags.MaterialColor)) {
                Vector4 diffuseColor = new Vector4(
                    DiffuseColor.R / 255f * Alpha, 
                    DiffuseColor.G / 255f * Alpha,
                    DiffuseColor.B / 255f * Alpha,
                    Alpha
                );

                DiffuseColorParameter.SetValue(diffuseColor);

                _dirtyFlags -= DirtyFlags.MaterialColor;
            }

            if (_dirtyFlags.Has(DirtyFlags.TechniqueIndex)) {
                int techniqueIndex = 0;

                if (TextureEnabled) {
                    techniqueIndex += 1;
                }

                SetCurrentTechnique(techniqueIndex);

                _dirtyFlags -= DirtyFlags.TechniqueIndex;
            }
        }

        public void ResetParameters() {
            World = View = Projection = Matrix.Identity;
            DiffuseColor = Color.White;
            Alpha = 1f;
            TextureEnabled = false;
            Texture = null;
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnApply() {
            base.OnApply();
            UpdateParameters();
        }

        protected override void BeforePassApply() {
            base.BeforePassApply();

            XNAEffect.GraphicsDevice.Textures[0] = TextureEnabled ? Texture.XNATexture : null;
        }

        #endregion Protected Methods
    }
}
