﻿namespace Raccoon.Graphics {
    public class Image : Graphic, System.IDisposable {
        #region Private Members

        private Texture _texture;
        private Rectangle _sourceRegion, _clippingRegion, _destinationRegion;

        #endregion Private Members

        #region Constructors

        public Image() {
        }

        public Image(Texture texture) {
            Texture = texture;
        }

        public Image(string filename) : this(new Texture(filename)) { }

        public Image(AtlasSubTexture subTexture) : this(subTexture.Texture) {
            SourceRegion = subTexture.SourceRegion;
            ClippingRegion = subTexture.ClippingRegion;
            Origin = subTexture.OriginalFrame.Position;
        }

        public Image(Atlas atlas, string name) : this(atlas.RetrieveSubTexture(name)) {
        }

        public Image(AtlasAnimation animTexture) : this(animTexture.Texture) {
            Texture = animTexture.Texture;
            SourceRegion = animTexture.SourceRegion;
            AtlasAnimationFrame firstFrame = animTexture["all"][0];
            ClippingRegion = firstFrame.ClippingRegion;
            Origin = firstFrame.OriginalFrame.Position;
        }

        public Image(AtlasAnimation atlasAnimation, AtlasAnimationFrame animFrameSubTexture) : this(atlasAnimation.Texture) {
            SourceRegion = atlasAnimation.SourceRegion;
            ClippingRegion = animFrameSubTexture.ClippingRegion;
            Origin = animFrameSubTexture.OriginalFrame.Position;
        }

        public Image(AtlasAnimation atlasAnimation, string tag, int frameIndex) : this(atlasAnimation, atlasAnimation[tag][frameIndex]) {
        }

        public Image(Image image) : this(image.Texture) {
            SourceRegion = image.SourceRegion;
            ClippingRegion = image.ClippingRegion;

            if (!image.DestinationRegion.IsEmpty) {
                DestinationRegion = image.DestinationRegion;
            }
        }

        public Image(AtlasAnimation atlasAnimation, int frameIndex) : this(atlasAnimation, AtlasAnimation.DefaultAllFramesTrackName, frameIndex) {
        }

        public Image(Atlas atlas, string name, int frameIndex) : this(atlas.RetrieveAnimation(name), frameIndex) {
        }

        public Image(Animation animation, string tag, int frameIndex) {
            Texture = animation.Texture;
            SourceRegion = animation.SourceRegion;

            Animation.Track track = animation[tag];
            ref Animation.Track.Frame frame = ref track.Frames[frameIndex];
            ClippingRegion = frame.FrameRegion;

            if (frame.FrameDestination.HasValue) {
                Origin = frame.FrameDestination.Value.Position;
            }

            if (!animation.DestinationRegion.IsEmpty) {
                DestinationRegion = animation.DestinationRegion;
            }
        }

        #endregion Constructors

        #region Public Properties

        public Texture Texture {
            get {
                return _texture;
            }

            set {
                _texture = value ?? throw new System.ArgumentNullException("Invalid texture");

                if (SourceRegion == ClippingRegion) {
                    _clippingRegion = Raccoon.Rectangle.Empty;
                }

                SourceRegion = _texture.Bounds;
                if (ClippingRegion.IsEmpty) {
                    ClippingRegion = SourceRegion;
                } else {
                    ClippingRegion = Util.Math.Clamp(ClippingRegion, SourceRegion);
                }
            }
        }

        public Rectangle SourceRegion {
            get {
                return _sourceRegion;
            }

            set {
                if (value.Left < Texture.Bounds.Left || value.Top < Texture.Bounds.Top || value.Right > Texture.Bounds.Right || value.Bottom > Texture.Bounds.Bottom) {
                    throw new System.ArgumentOutOfRangeException("SourceRegion", value, "Value must be within texture bounds");
                }

                _sourceRegion = value;
            }
        }

        public Rectangle ClippingRegion {
            get {
                return _clippingRegion;
            }

            set {
                if (value.Left < 0 || value.Top < 0 || value.Right > _sourceRegion.Width || value.Bottom > _sourceRegion.Height) {
                    throw new System.ArgumentOutOfRangeException("ClippingRegion", value, $"Value must be within source region bounds {_sourceRegion}");
                }

                _clippingRegion = value;

                if (DestinationRegion.IsEmpty) {
                    Size = _clippingRegion.Size;
                }
            }
        }

        public Rectangle DestinationRegion {
            get {
                return _destinationRegion;
            }

            set {
                _destinationRegion = value;
                Size = DestinationRegion.Size;
            }
        }

        #endregion Public Propeties

        #region Public Methods

        #region Primitives

        public static Image Rectangle(int width, int height, bool filled = true) {
            Debug.Assert(Game.Instance.XNAGameWrapper.GraphicsDevice != null, "Primitive needs a valid graphics device. Maybe are you creating before Scene.Start() is called?");
            Debug.Assert(width * height > 0, "Invalid primitive size.");

            Color[] data = new Color[width * height];

            if (filled) {
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        data[x + y * width] = Color.White;
                    }
                }
            } else {
                // left & right columns
                for (int x = 0; x < width; x++) {
                    data[x] = Color.White;
                    data[x + (height - 1) * width] = Color.White;
                }

                // top & bottom rows
                for (int y = 1; y < height - 1; y++) {
                    data[y * width] = Color.White;
                    data[width - 1 + y * width] = Color.White;
                }

            }

            Texture texture = new Texture(width, height);
            texture.SetData(data);
            return new Image(texture);
        }

        public static Image Circle(int radius) {
            Debug.Assert(Game.Instance.XNAGameWrapper.GraphicsDevice != null, "Primitive needs a valid graphics device. Maybe are you creating before Scene.Start() is called?");
            Debug.Assert(radius > 0, "Invalid primitive size.");

            int diameter = radius * radius;
            int width = diameter,
                height = diameter;

            Color[] data = new Color[(width + 1) * (height + 1)];

            // midpoint circle algorithm
            int x = radius, y = 0, err = 0, x0 = radius, y0 = radius;
            while (x >= y) {
                data[x0 + x + (y0 + y) * (width + 1)] = Color.White;
                data[x0 + y + (y0 + x) * (width + 1)] = Color.White;
                data[x0 - y + (y0 + x) * (width + 1)] = Color.White;
                data[x0 - x + (y0 + y) * (width + 1)] = Color.White;
                data[x0 - x + (y0 - y) * (width + 1)] = Color.White;
                data[x0 - y + (y0 - x) * (width + 1)] = Color.White;
                data[x0 + y + (y0 - x) * (width + 1)] = Color.White;
                data[x0 + x + (y0 - y) * (width + 1)] = Color.White;

                y += 1;
                err += 1 + 2 * y;
                if (2 * (err - x) + 1 > 0) {
                    x -= 1;
                    err += 1 - 2 * x;
                }
            }

            Texture texture = new Texture(width + 1, height + 1);
            texture.SetData(data);
            return new Image(texture);
        }

        #endregion Primitives

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            _texture = null;

            base.Dispose();
        }

        public override string ToString() {
            return $"[Image | Position: {Position}, Texture: {Texture}]";
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            if (DestinationRegion.IsEmpty) {
                Renderer.Draw(
                    Texture,
                    position,
                    SourceRegion.Position + ClippingRegion,
                    rotation,
                    scale,
                    flip,
                    new Color(color, (color.A / 255f) * Opacity),
                    origin,
                    scroll,
                    shader,
                    shaderParameters,
                    layerDepth
                );
                return;
            }

            Renderer.Draw(
                Texture,
                new Rectangle(position, DestinationRegion.Size * scale),
                SourceRegion.Position + ClippingRegion,
                rotation,
                flip,
                new Color(color, (color.A / 255f) * Opacity),
                origin,
                scroll,
                shader,
                shaderParameters,
                layerDepth
            );
        }

        #endregion Protected Methods
    }
}
