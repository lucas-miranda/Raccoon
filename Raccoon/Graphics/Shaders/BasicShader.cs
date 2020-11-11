using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    public class BasicShader : Shader, IShaderTransform, IShaderTexture, IShaderVertexColor, IShaderDepthWrite {
        #region Public Members

        [System.Flags]
        public enum DirtyFlag {
            None                    = 0,
            TechniqueIndex          = 1 << 0,
            WorldViewProjection     = 1 << 1,
            MaterialColor           = 1 << 2
        }

        #endregion Public Members

        #region Private Members

        private Matrix _world, _view, _projection;
        private Color _diffuseColor = Color.White;
        private float _alpha = 1f;
        private bool _textureEnabled, _depthWriteEnabled;
        private Texture _texture;

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

        public BasicShader(byte[] basicEffectCode) : this(new Effect(Game.Instance.GraphicsDevice, basicEffectCode) { Name = "BasicShader" }) {
        }

        #endregion Constructors

        #region Public Properties

        public Matrix World {
            get {
                return _world;
            }

            set {
                _world = value;
                DirtyFlags |= DirtyFlag.WorldViewProjection;
            }
        }

        public Matrix View {
            get {
                return _view;
            }

            set {
                _view = value;
                DirtyFlags |= DirtyFlag.WorldViewProjection;
            }
        }

        public Matrix Projection {
            get {
                return _projection;
            }

            set {
                _projection = value;
                DirtyFlags |= DirtyFlag.WorldViewProjection;
            }
        }

        public Color DiffuseColor {
            get {
                return _diffuseColor;
            }

            set {
                _diffuseColor = value;
                DirtyFlags |= DirtyFlag.MaterialColor;
            }
        }

        public float Alpha {
            get {
                return _alpha;
            }

            set {
                _alpha = value;
                DirtyFlags |= DirtyFlag.MaterialColor;
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
                DirtyFlags |= DirtyFlag.TechniqueIndex;
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

        public bool DepthWriteEnabled {
            get {
                return _depthWriteEnabled;
            }

            set {
                if (value == _depthWriteEnabled) {
                    return;
                }

                _depthWriteEnabled = value;
                DirtyFlags |= DirtyFlag.TechniqueIndex;
            }
        }

        #endregion Public Properties

        #region Internal Properties

        protected BitTag DirtyFlags { get; set; }

        protected internal EffectParameter WorldViewProjectionParameter { get; set; }
        protected internal EffectParameter DiffuseColorParameter { get; set; }
        protected internal EffectParameter TextureParameter { get; set; }
        protected Matrix WorldViewProjection { get; private set; }

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

        public virtual void UpdateParameters() {
            if (DirtyFlags.Has(DirtyFlag.WorldViewProjection)) {
                Matrix.Multiply(ref _world, ref _view, out Matrix worldView);
                Matrix.Multiply(ref worldView, ref _projection, out Matrix worldViewProjection);
                WorldViewProjection = worldViewProjection;
                WorldViewProjectionParameter.SetValue(worldViewProjection);

                DirtyFlags -= DirtyFlag.WorldViewProjection;
            }

            if (DirtyFlags.Has(DirtyFlag.MaterialColor)) {
                Vector4 diffuseColor = new Vector4(
                    DiffuseColor.R / 255f * Alpha,
                    DiffuseColor.G / 255f * Alpha,
                    DiffuseColor.B / 255f * Alpha,
                    Alpha
                );

                DiffuseColorParameter.SetValue(diffuseColor);

                DirtyFlags -= DirtyFlag.MaterialColor;
            }

            if (DirtyFlags.Has(DirtyFlag.TechniqueIndex)) {
                OnUpdateTechniqueIndex();
                DirtyFlags -= DirtyFlag.TechniqueIndex;
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

        protected virtual void OnUpdateTechniqueIndex() {
            int techniqueIndex = 0;

            if (TextureEnabled) {
                techniqueIndex |= 1;
            }

            if (DepthWriteEnabled) {
                techniqueIndex |= 1 << 1;
            }

            SetCurrentTechnique(techniqueIndex);
        }

        #endregion Protected Methods
    }
}
