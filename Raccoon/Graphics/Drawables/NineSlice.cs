using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics {
    /// <summary>
    /// Creates a special 3 by 3 squared area which can resize it's textures properly, by changing scale or repeating, it's squares as needed.
    /// </summary>
    public class NineSlice : Graphic {
        #region Public Members

        public enum DrawMode {
            Stretch = 0,
            Repeat,
        }

        #endregion Public Members

        #region Private Members

        private static RepeatShader RepeatShader;

        private Patch[] _patches = new Patch[9];
        private VertexPositionColorTexture[] _vertices;
        private int _verticesCount, _indicesCount, _triangleCount;
        private int[] _indices;
        private bool _isUsingCustomSize;
        private Size _baseSize;

        #endregion Private Members

        #region Constructors

        private NineSlice() {
            for (int i = 0; i < _patches.Length; i++) {
                _patches[i] = new Patch(this);
            }
        }

        public NineSlice(Rectangle centerSlice, AtlasSubTexture subTexture) : this() {
            Prepare(centerSlice, subTexture);
        }

        public NineSlice(Rectangle centerSlice, AtlasAnimation atlasAnimation, string tag, int frameIndex) : this() {
            Prepare(centerSlice, atlasAnimation, tag, frameIndex);
        }

        public NineSlice(Rectangle centerSlice, AtlasAnimation atlasAnimation, int frameIndex) : this() {
            Prepare(centerSlice, atlasAnimation, frameIndex);
        }

        #endregion Constructors

        #region Public Properties

        public Texture Texture { get; set; }
        public Patch TopLeftPatch { get { return _patches[0]; } }
        public Patch TopCenterPatch { get { return _patches[1]; } }
        public Patch TopRightPatch { get { return _patches[2]; } }
        public Patch CenterLeftPatch { get { return _patches[3]; } }
        public Patch CenterPatch { get { return _patches[4]; } }
        public Patch CenterRightPatch { get { return _patches[5]; } }
        public Patch BottomLeftPatch { get { return _patches[6]; } }
        public Patch BottomCenterPatch { get { return _patches[7]; } }
        public Patch BottomRightPatch { get { return _patches[8]; } }
        public bool IsSingleDraw { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Setup(float width, float height) {
            Size = new Size(width, height);
            _isUsingCustomSize = true;

            // adjust patches sizes
            // TODO  store somewhere infos about every column and row and use here instead

            float centerWidth = width - (TopLeftPatch.Width + TopRightPatch.Width),
                  centerHeight = height - (TopLeftPatch.Height + BottomLeftPatch.Height);

            CenterPatch.Size = new Size(centerWidth, centerHeight);
            TopCenterPatch.Width = BottomCenterPatch.Width = centerWidth;
            CenterLeftPatch.Height = CenterRightPatch.Height = centerHeight;
            NeedsReload = true;
        }

        public void Setup(Size size) {
            Setup(size.Width, size.Height);
        }

        public override void DebugRender(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll) {
            float x,
                  y = 0f;

            for (int row = 0; row < 3; row++) {
                float rowHeight = 0f;
                x = 0f;

                for (int column = 0; column < 3; column++) {
                    Patch patch = _patches[row * 3 + column];

                    Debug.Draw.Rectangle.AtWorld(
                        new Rectangle(
                            new Vector2(position.X + x, position.Y + y),
                            patch.Size
                        ),
                        false,
                        Colors.Magenta
                    );

                    x += patch.Width;

                    if (patch.Height > rowHeight) {
                        rowHeight = patch.Height;
                    }
                }

                y += rowHeight;
            }
        }

        public void Prepare(Rectangle centerSlice, AtlasSubTexture subTexture) {
            if (subTexture == null) {
                throw new System.ArgumentNullException(nameof(subTexture));
            }

            Texture = subTexture.Texture;

            bool wasUsingCustomSize = _isUsingCustomSize;
            Size previousSize = Size;
            _isUsingCustomSize = false;

            Prepare(
                centerSlice,
                subTexture.SourceRegion,
                subTexture.ClippingRegion,
                subTexture.OriginalFrame
            );

            Load();
            _baseSize = Size;

            if (wasUsingCustomSize) {
                Setup(previousSize);
            }
        }

        public void Prepare(Rectangle centerSlice, AtlasAnimation atlasAnimation, string tag, int frameIndex) {
            if (atlasAnimation == null) {
                throw new System.ArgumentNullException(nameof(atlasAnimation));
            }

            if (!atlasAnimation.TryGetTrack(tag, out List<AtlasAnimationFrame> frames)) {
                throw new System.ArgumentException($"Supplied atlas animation doesn't contains track '{tag}'.");
            }

            if (frames.Count <= 0) {
                throw new System.InvalidOperationException($"There is no frames at track '{tag}'.");
            } else if (frameIndex < 0 || frameIndex >= frames.Count) {
                throw new System.IndexOutOfRangeException($"Supplied frame index '{frameIndex}' is out of valid frames range [0, {frames.Count - 1}] at track '{tag}'.");
            }

            Texture = atlasAnimation.Texture;
            AtlasAnimationFrame frame = frames[frameIndex];

            bool wasUsingCustomSize = _isUsingCustomSize;
            Size previousSize = Size;
            _isUsingCustomSize = false;

            Prepare(
                centerSlice,
                atlasAnimation.SourceRegion,
                frame.ClippingRegion,
                frame.OriginalFrame
            );

            Load();
            _baseSize = Size;

            if (wasUsingCustomSize) {
                Setup(previousSize);
            }
        }

        public void Prepare(Rectangle centerSlice, AtlasAnimation atlasAnimation, int frameIndex) {
            if (atlasAnimation == null) {
                throw new System.ArgumentNullException(nameof(atlasAnimation));
            }

            if (!atlasAnimation.TryGetDefaultTrack(out List<AtlasAnimationFrame> frames)) {
                throw new System.ArgumentException("Supplied atlas animation doesn't contains default track.");
            }

            if (frames.Count <= 0) {
                throw new System.InvalidOperationException("There is no frames at default track.");
            } else if (frameIndex < 0 || frameIndex >= frames.Count) {
                throw new System.IndexOutOfRangeException($"Supplied frame index '{frameIndex}' is out of valid frames range [0, {frames.Count - 1}] at default track.");
            }

            Texture = atlasAnimation.Texture;
            AtlasAnimationFrame frame = frames[frameIndex];

            bool wasUsingCustomSize = _isUsingCustomSize;
            Size previousSize = Size;
            _isUsingCustomSize = false;

            Prepare(
                centerSlice,
                atlasAnimation.SourceRegion,
                frame.ClippingRegion,
                frame.OriginalFrame
            );

            Load();
            _baseSize = Size;

            if (wasUsingCustomSize) {
                Setup(previousSize);
            }
        }

        public override void Dispose() {
            if (!IsDisposed) {

                return;
            }

            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();

            if (RepeatShader == null) {
                RepeatShader = new RepeatShader(Resource.RepeatShader) {
                    //DepthWriteEnabled = true,
                };
            }

            int requiredVertices = 0,
                requiredIndices = 0;

            _triangleCount = 0;
            IsSingleDraw = true;

            foreach (Patch patch in _patches) {
                patch.Prepare(Texture);

                if (patch.DrawMode == DrawMode.Repeat) {
                    // repeat mode is rendered separated
                    IsSingleDraw = false;
                    continue;
                }

                requiredVertices += patch.VerticesCount;
                requiredIndices += patch.IndicesCount;
                _triangleCount += patch.TriangleCount;
            }

            if (_vertices == null) {
                _vertices = new VertexPositionColorTexture[requiredVertices];
            } else if (_vertices.Length < requiredVertices) {
                System.Array.Resize(ref _vertices, requiredVertices);
            }

            if (_indices == null) {
                _indices = new int[requiredIndices];
            } else if (_indices.Length < requiredIndices) {
                System.Array.Resize(ref _indices, requiredIndices);
            }

            // flushing
            int verticesStartIndex = 0,
                indicesStartIndex = 0;

            float x,
                  y = 0f,
                  w = 0f;

            for (int row = 0; row < 3; row++) {
                float rowHeight = 0f;
                x = 0f;

                for (int column = 0; column < 3; column++) {
                    Patch patch = _patches[row * 3 + column];

                    patch.Flush(
                        _vertices,
                        verticesStartIndex,
                        _indices,
                        indicesStartIndex,
                        new Vector2(x, y)
                    );

                    x += patch.Width;

                    if (patch.Height > rowHeight) {
                        rowHeight = patch.Height;
                    }

                    if (patch.DrawMode != DrawMode.Repeat) {
                        verticesStartIndex += patch.VerticesCount;
                        indicesStartIndex += patch.IndicesCount;
                    }
                }

                if (x > w) {
                    w = x;
                }

                y += rowHeight;
            }

            //

            if (!_isUsingCustomSize) {
                Size = new Size(w, y);
            }

            _verticesCount = requiredVertices;
            _indicesCount = requiredIndices;
        }

        protected override void Draw(
            Vector2 position,
            float rotation,
            Vector2 scale,
            ImageFlip flip,
            Color color,
            Vector2 scroll,
            Shader shader,
            IShaderParameters shaderParameters,
            Vector2 origin,
            float layerDepth
        ) {
            if (_vertices == null
             || _vertices.Length == 0
             || _indices == null
             || _indices.Length == 0
            ) {
                return;
            }

            if (!IsSingleDraw) {
                // draw only patches which isn't registered at NineSlice's vertices
                float x,
                      y = 0f;

                for (int row = 0; row < 3; row++) {
                    float rowHeight = 0f;
                    x = 0f;

                    for (int column = 0; column < 3; column++) {
                        Patch patch = _patches[row * 3 + column];

                        switch (patch.DrawMode) {
                            case DrawMode.Stretch:
                                break;

                            case DrawMode.Repeat:
                                patch.Draw(
                                    Renderer,
                                    Texture,
                                    position + new Vector2(x, y) * scale,
                                    rotation,
                                    scale,
                                    flip,
                                    new Color(color, (color.A / 255f) * Opacity),
                                    scroll,
                                    shader,
                                    shaderParameters,
                                    origin,
                                    layerDepth
                                );
                                break;

                            default:
                                throw new System.NotImplementedException(
                                    $"{nameof(DrawMode)} '{patch.DrawMode}' isn't handled."
                                );
                        }

                        x += patch.Width;

                        if (patch.Height > rowHeight) {
                            rowHeight = patch.Height;
                        }
                    }

                    y += rowHeight;
                }
            }

            if (_vertices.Length == 0) {
                return;
            }

            Renderer.DrawVertices(
                texture:            Texture,
                vertexData:         _vertices,
                minVertexIndex:     0,
                verticesLength:     _vertices.Length,
                indices:            _indices,
                minIndex:           0,
                primitivesCount:    _triangleCount,
                isHollow:           false,
                position:           position,
                rotation:           rotation,
                scale:              scale,
                color:              new Color(color, (color.A / 255f) * Opacity),
                origin:             origin,
                scroll:             scroll,
                shader:             shader,
                shaderParameters:   shaderParameters,
                layerDepth:         layerDepth
            );
        }

        #endregion Protected Methods

        #region Private Methods

        private void Prepare(Rectangle centerSlice, Rectangle sourceRegion, Rectangle clippingRegion, Rectangle originalFrame) {
            //Origin = originalFrame.Position;
            centerSlice += originalFrame.Position;

            // top-left
            Patch p = _patches[0];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = new Rectangle(
                clippingRegion.Position,
                new Size(centerSlice.Left, centerSlice.Top)
            );

            // top-center
            p = _patches[1];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = new Rectangle(
                new Vector2(clippingRegion.X + centerSlice.Left, clippingRegion.Y),
                new Size(centerSlice.Width, centerSlice.Top)
            );

            // top-right
            p = _patches[2];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = new Rectangle(
                new Vector2(clippingRegion.X + centerSlice.Right, clippingRegion.Y),
                new Size(clippingRegion.Width - centerSlice.Right, centerSlice.Top)
            );


            // center-left
            p = _patches[3];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = new Rectangle(
                new Vector2(clippingRegion.X, clippingRegion.Y + centerSlice.Top),
                new Size(centerSlice.Left, centerSlice.Height)
            );

            // center
            p = _patches[4];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = centerSlice + clippingRegion.Position;

            // top-right
            p = _patches[5];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = new Rectangle(
                new Vector2(clippingRegion.X + centerSlice.Right, clippingRegion.Y + centerSlice.Top),
                new Size(clippingRegion.Width - centerSlice.Right, centerSlice.Height)
            );

            // bottom-left
            p = _patches[6];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = new Rectangle(
                new Vector2(clippingRegion.X, clippingRegion.Y + centerSlice.Bottom),
                new Size(centerSlice.Left, clippingRegion.Height - centerSlice.Bottom)
            );

            // bottom-center
            p = _patches[7];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = new Rectangle(
                new Vector2(clippingRegion.X + centerSlice.Left, clippingRegion.Y + centerSlice.Bottom),
                new Size(centerSlice.Width, clippingRegion.Height - centerSlice.Bottom)
            );

            // bottom-right
            p = _patches[8];
            p.SourceRegion = sourceRegion;
            p.ClippingRegion = new Rectangle(
                new Vector2(clippingRegion.X + centerSlice.Right, clippingRegion.Y + centerSlice.Bottom),
                new Size(
                    clippingRegion.Width - centerSlice.Right,
                    clippingRegion.Height - centerSlice.Bottom
                )
            );
        }

        #endregion Private Methods

        #region Patch Class

        public class Patch {
            private VertexPositionColorTexture[] _vertices;
            private int[] _indices;
            private Rectangle _sourceRegion, _clippingRegion;
            private Size? _size;
            private DrawMode _drawMode;
            private Vector2? _repeat;
            private Shader _shader;
            private IShaderParameters _shaderParameters;

            public Patch(NineSlice nineSlice) {
                NineSlice = nineSlice;
                _vertices = new VertexPositionColorTexture[4];
                _indices = new int[6] {
                    0, 1, 2,
                    2, 1, 3
                };

                TriangleCount = 2;
            }

            public Rectangle SourceRegion {
                get {
                    return _sourceRegion;
                }

                set {
                    _sourceRegion = value;
                    _clippingRegion = Math.Clamp(_clippingRegion, new Rectangle(_sourceRegion.Size));
                }
            }

            public Rectangle ClippingRegion {
                get {
                    return _clippingRegion;
                }

                set {
                    if (value.Left < 0
                     || value.Top < 0
                     || value.Right > SourceRegion.Width
                     || value.Bottom > SourceRegion.Height
                    ) {
                        throw new System.ArgumentException($"Value [{value}] can't be out of {nameof(SourceRegion)} bounds [{SourceRegion}].");
                    }

                    _clippingRegion = value;
                }
            }

            public Size Size {
                get {
                    return _size.GetValueOrDefault(ClippingRegion.Size);
                }

                set {
                    _size = value;
                }
            }

            public float Width {
                get {
                    return _size.HasValue ? _size.Value.Width : ClippingRegion.Width;
                }

                set {
                    _size = new Size(value, Height);
                }
            }

            public float Height {
                get {
                    return _size.HasValue ? _size.Value.Height : ClippingRegion.Height;
                }

                set {
                    _size = new Size(Width, value);
                }
            }

            public DrawMode DrawMode {
                get {
                    return _drawMode;
                }

                set {
                    if (value == _drawMode) {
                        return;
                    }

                    _drawMode = value;

                    switch (_drawMode) {
                        case DrawMode.Stretch:
                            NineSlice.NeedsReload = true;
                            break;

                        case DrawMode.Repeat:
                            NineSlice.NeedsReload = true;
                            break;

                        default:
                            throw new System.NotImplementedException(
                                $"{nameof(DrawMode)} '{_drawMode}' isn't handled."
                            );
                    }
                }
            }

            public Vector2? Repeat {
                get {
                    return _repeat;
                }

                set {
                    if (value == _repeat) {
                        return;
                    }

                    _repeat = value;

                    if (HasOrCreateShaderParameters(out RepeatShaderParameters repeatShaderParameters)) {
                        if (_repeat.HasValue) {
                            repeatShaderParameters.Repeat = _repeat.Value;
                        } else {
                            repeatShaderParameters.Repeat = Vector2.One;
                        }
                    }
                }
            }

            public Shader Shader {
                get {
                    return _shader;
                }

                set {
                    if (value == _shader) {
                        return;
                    }

                    _shader = value;
                    NineSlice.NeedsReload = true;
                }
            }

            public IShaderParameters ShaderParameters {
                get {
                    return _shaderParameters;
                }

                set {
                    if (value == _shaderParameters) {
                        return;
                    }

                    _shaderParameters = value;
                    NineSlice.NeedsReload = true;
                }
            }

            private NineSlice NineSlice { get; }

            internal bool HasUnflushedData { get; private set; }
            internal int VerticesCount { get { return _vertices == null ? 0 : _vertices.Length; } }
            internal int IndicesCount { get { return _indices == null ? 0 : _indices.Length; } }
            internal int TriangleCount { get; private set; }

            internal void Prepare(Texture texture) {
                //
                // Vertices layout:
                //
                //  1--3
                //  |\ |
                //  | \|
                //  0--2
                //

                if (texture == null) {
                    _vertices[0] = _vertices[1] =
                        _vertices[2] = _vertices[3] = new VertexPositionColorTexture();

                    HasUnflushedData = true;
                    return;
                }

                Rectangle textCoord = new Rectangle(
                    (SourceRegion.Left + ClippingRegion.Left) / texture.Width,
                    (SourceRegion.Top + ClippingRegion.Top) / texture.Height,
                    ClippingRegion.Width / texture.Width,
                    ClippingRegion.Height / texture.Height
                );

                Size size = Size;

                _vertices[0] = new VertexPositionColorTexture(
                    new Microsoft.Xna.Framework.Vector3(0f, size.Height, 0f),
                    Color.White,
                    textCoord.BottomLeft
                );

                _vertices[1] = new VertexPositionColorTexture(
                    new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f),
                    Color.White,
                    textCoord.TopLeft
                );

                _vertices[2] = new VertexPositionColorTexture(
                    new Microsoft.Xna.Framework.Vector3(size.Width, size.Height, 0f),
                    Color.White,
                    textCoord.BottomRight
                );

                _vertices[3] = new VertexPositionColorTexture(
                    new Microsoft.Xna.Framework.Vector3(size.Width, 0f, 0f),
                    Color.White,
                    textCoord.TopRight
                );

                if (HasOrCreateShaderParameters(out RepeatShaderParameters repeatShaderParameters)) {
                    if (!_repeat.HasValue) {
                        // calculate repeat using available size
                        if (!ClippingRegion.Size.IsEmpty) {
                            repeatShaderParameters.Repeat = new Vector2(
                                Math.Round(size.Width / ClippingRegion.Width),
                                Math.Round(size.Height / ClippingRegion.Height)
                            );
                        } else {
                            repeatShaderParameters.Repeat = Vector2.One;
                        }
                    }

                    repeatShaderParameters.SetupTextureAreaClip(texture.Size, SourceRegion, ClippingRegion);
                }

                HasUnflushedData = true;
            }

            internal void Flush(
                VertexPositionColorTexture[] vertices,
                int verticesStartIndex,
                int[] indices,
                int indicesStartIndex,
                Vector2 position
            ) {
                if (!HasUnflushedData || DrawMode == DrawMode.Repeat) {
                    return;
                }

                for (int i = 0; i < _vertices.Length; i++) {
                    VertexPositionColorTexture vertex = _vertices[i];

                    vertices[verticesStartIndex + i] = new VertexPositionColorTexture(
                        new Microsoft.Xna.Framework.Vector3(
                            vertex.Position.X + position.X,
                            vertex.Position.Y + position.Y,
                            vertex.Position.Z
                        ),
                        vertex.Color,
                        vertex.TextureCoordinate
                    );
                }

                for (int i = 0; i < _indices.Length; i++) {
                    indices[indicesStartIndex + i] = verticesStartIndex + _indices[i];
                }

                HasUnflushedData = false;
            }

            internal void Draw(
                Renderer renderer,
                Texture texture,
                Vector2 position,
                float rotation,
                Vector2 scale,
                ImageFlip flip,
                Color color,
                Vector2 scroll,
                Shader shader,
                IShaderParameters shaderParameters,
                Vector2 origin,
                float layerDepth
            ) {
                if (Shader != null) {
                    shader = Shader;
                } else if (shader == null) {
                    shader = RepeatShader;
                }

                if (ShaderParameters != null) {
                    shaderParameters = ShaderParameters;
                }

                renderer.DrawVertices(
                    texture:            texture,
                    vertexData:         _vertices,
                    minVertexIndex:     0,
                    verticesLength:     _vertices.Length,
                    indices:            _indices,
                    minIndex:           0,
                    primitivesCount:    2,
                    isHollow:           false,
                    position:           position,
                    rotation:           rotation,
                    scale:              scale,
                    color:              color,
                    origin:             origin,
                    scroll:             scroll,
                    shader:             shader,
                    shaderParameters:   shaderParameters,
                    layerDepth:         layerDepth
                );
            }

            private bool HasOrCreateShaderParameters(out RepeatShaderParameters repeatShaderParameters) {
                if (!(Shader == null || Shader is RepeatShader)) {
                    repeatShaderParameters = null;
                    return false;
                }

                if (ShaderParameters != null) {
                    if (!(ShaderParameters is RepeatShaderParameters parameters)) {
                        repeatShaderParameters = null;
                        return false;
                    }

                    repeatShaderParameters = parameters;
                } else {
                    repeatShaderParameters = new RepeatShaderParameters();
                    ShaderParameters = repeatShaderParameters;
                }

                return true;
            }
        }

        #endregion Patch Class
    }
}
