using System;

namespace Raccoon.Graphics {
    public class Image : Graphic {
        #region Private Members

        private Texture _texture;
        private Rectangle _sourceRegion, _clippingRegion;
        private Size _destinationSize;

        #endregion Private Members

        #region Constructors

        public Image() { }

        public Image(Texture texture) {
            Texture = texture;
        }

        public Image(string filename) : this(new Texture(filename)) { }

        public Image(AtlasSubTexture subTexture) : this(subTexture.Texture) {
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
                if (value == null) throw new ArgumentNullException("Invalid texture");

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
            }
        }

        public Rectangle ClippingRegion {
            get {
                return _clippingRegion;
            }

            set {
                if (value.Left < 0 || value.Top < 0 || value.Right > _sourceRegion.Width || value.Bottom > _sourceRegion.Height)
                    throw new ArgumentOutOfRangeException("ClippingRegion", value, $"Value must be within source region bounds {_sourceRegion}");

                _clippingRegion = value;

                if (_destinationSize.IsEmpty) {
                    Size = _clippingRegion.Size;
                }
            }
        }

        public Size DestinationSize {
            get {
                return _destinationSize;
            }

            set {
                Size = _destinationSize = value;
            }
        }

        #endregion Public Propeties

        #region Public Methods
        
        public override void Render(Vector2 position, float rotation) {
            Surface.Draw(
                Texture,
                position,
                (DestinationSize.IsEmpty ? Size : DestinationSize) * Scale,
                SourceRegion.Position + ClippingRegion,
                Origin,
                rotation * Util.Math.DegToRad,
                Vector2.One,
                FinalColor,
                Scroll,
                Flipped
            );
        }

        public override void Dispose() {
            if (Texture == null) {
                return;
            }

            Texture.Dispose();
        }

        public override string ToString() {
            return $"[Image | Position: {Position}, Texture: {Texture}]";
        }

        #endregion
    }
}
