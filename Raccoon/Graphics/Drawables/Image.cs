using Microsoft.Xna.Framework.Graphics;
using System;

namespace Raccoon.Graphics {
    public class Image : Graphic {
        #region Private Members

        private Texture _texture;
        private Rectangle _sourceRegion, _clippingRegion;

        #endregion Private Members

        #region Constructors

        public Image() {
        }

        public Image(string filename) {
            Texture = new Texture(filename);
        }

        public Image(AtlasSubTexture subTexture) {
            Texture = subTexture.Texture;
            SourceRegion = subTexture.Region;
            ClippingRegion = new Rectangle(SourceRegion.Width, SourceRegion.Height);
        }

        #endregion Constructors

        #region Public Properties

        public Texture Texture {
            get {
                return _texture;
            }

            set {
                if (value == null) throw new ArgumentNullException("Texture");

                _texture = value;
                ClippingRegion = SourceRegion = _texture.Bounds;
            }
        }

        public Rectangle SourceRegion {
            get {
                return _sourceRegion;
            }

            set {
                if (value.Left < Texture.Bounds.Left || value.Top < Texture.Bounds.Top || value.Right > Texture.Bounds.Right || value.Bottom > Texture.Bounds.Bottom)
                    throw new ArgumentOutOfRangeException("SourceRegion", value, "Value must be within texture bounds");
                
                _sourceRegion = value;

                // keep clipping region in source region bounds
                if (_clippingRegion.Left < _sourceRegion.Left) {
                    _clippingRegion.Left = _sourceRegion.Left;
                }

                if (_clippingRegion.Right > _sourceRegion.Right) {
                    _clippingRegion.Right = _sourceRegion.Right;
                }

                if (_clippingRegion.Top < _sourceRegion.Top) {
                    _clippingRegion.Top = _sourceRegion.Top;
                }

                if (_clippingRegion.Bottom > _sourceRegion.Bottom) {
                    _clippingRegion.Bottom = _sourceRegion.Bottom;
                }
            }
        }

        public Rectangle ClippingRegion {
            get {
                return _clippingRegion;
            }

            set {
                if (value.Left < 0 || value.Top < 0 || value.Right > _sourceRegion.Width || value.Bottom > _sourceRegion.Height)
                    throw new ArgumentOutOfRangeException("ClippingRegion", value, "Value must be within source region size");

                _clippingRegion = value;
                Size = _clippingRegion.Size;
            }
        }

        #endregion Public Propeties

        #region Public Methods
        
        public override void Render() {
            Game.Instance.Core.SpriteBatch.Draw(
                Texture.XNATexture,
                Position,
                null,
                SourceRegion.Position + ClippingRegion,
                Origin,
                Rotation,
                Scale,
                FinalColor,
                (SpriteEffects) Flipped,
                LayerDepth
            );
        }

        public override void Dispose() {
            if (Texture != null) {
                Texture.Dispose();
            }
        }
        
        #endregion
    }
}
