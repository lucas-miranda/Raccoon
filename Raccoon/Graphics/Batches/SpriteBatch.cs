using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class SpriteBatch {
        #region Public Members

        public const int StartBatchItemsCount = 100;

        #endregion Public Members

        #region Private Members

        private VertexPositionColorTexture[] _vertexBuffer = new VertexPositionColorTexture[StartBatchItemsCount * 4];
        private short[] _indexBuffer = new short[StartBatchItemsCount * 6];

        private SpriteBatchItem[] _batchItems = new SpriteBatchItem[StartBatchItemsCount],
                                  _transparencyBatchItems = null;

        private int _nextItemIndex, _nextItemWithTransparencyIndex;

        #endregion Private Members

        #region Constructors

        public SpriteBatch(GraphicsDevice graphicsDevice = null, bool autoHandleAlphaBlendedSprites = false) {
            GraphicsDevice = graphicsDevice ?? Game.Instance.GraphicsDevice;
            Shader = Game.Instance.BasicShader;

            AutoHandleAlphaBlendedSprites = autoHandleAlphaBlendedSprites;

            if (AutoHandleAlphaBlendedSprites) {
                _transparencyBatchItems = new SpriteBatchItem[_batchItems.Length / 2];
                InitializeTransparencyItemsBuffers();
            }

            Initialize();
        }

        #endregion Constructors

        #region Public Properties

#if DEBUG

        /// <summary>
        /// Track number of draw calls.
        /// </summary>
        public static int TotalDrawCalls { get; private set; }

        /// <summary>
        /// Sprite count at current buffer.
        /// </summary>
        public static int SpriteCount { get; private set; }

#endif

        public GraphicsDevice GraphicsDevice { get; set; }
        public bool IsBatching { get; private set; }
        public Shader Shader { get; set; }

        /// <summary>
        /// Auto handle any non-opaque (i.e. with some transparency; Opacity < 1.0f) sprite rendering.
        /// By drawing first all opaque sprites, with depth write enabled, followed by non-opaque sprites, with only depth read enabled.
        /// </summary>
        public bool AutoHandleAlphaBlendedSprites { get; private set; }
        public bool AllowIBasicShaderEffectParameterClone { get; set; } = true;
        public BatchMode BatchMode { get; private set; }
        public BlendState BlendState { get; private set; }
        public SamplerState SamplerState { get; private set; }
        public DepthStencilState DepthStencilState { get; private set; }
        public RasterizerState RasterizerState { get; private set; }
        public Matrix Transform { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Begin(BatchMode batchMode = BatchMode.DrawOrder, BlendState blendState = null, SamplerState sampler = null, DepthStencilState depthStencil = null, RasterizerState rasterizer = null, Matrix? transform = null) {
            BatchMode = batchMode;
            BlendState = blendState ?? BlendState.AlphaBlend;
            SamplerState = sampler ?? SamplerState.PointClamp;
            DepthStencilState = depthStencil ?? DepthStencilState.None;
            RasterizerState = rasterizer ?? RasterizerState.CullNone;
            Transform = transform ?? Matrix.Identity;

            IsBatching = true;
        }

        public void End() {
            if (!IsBatching) {
                throw new System.InvalidOperationException("Begin() must be called before End().");
            }

            Flush();

            IsBatching = false;
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (!IsBatching) {
                throw new System.InvalidOperationException("Begin() must be called before any Draw() operation.");
            }

            ref SpriteBatchItem batchItem = ref GetBatchItem(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            Draw(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, null, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (!IsBatching) {
                throw new System.InvalidOperationException("Begin() must be called before any Draw() operation.");
            }

            ref SpriteBatchItem batchItem = ref GetBatchItem(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(texture, destinationRectangle, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            Text.RenderData glyphs = font.RenderMap.PrepareTextRenderData(text, out double textEmWidth, out double textEmHeight);
            DrawString(font, glyphs, new Rectangle(position, new Size((float) font.ConvertEmToPx(textEmWidth), (float) font.ConvertEmToPx(textEmHeight))), rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            DrawString(font, text, position, rotation, scale, flip, color, origin, scroll, shader, null, layerDepth);
        }

        /// <summary>
        /// Send all stored batches to rendering, but doesn't end batching.
        /// If auto handle alpha blended sprites is active, be careful! Since it can includes alpha blended sprites too.
        /// </summary>
        /// <param name="includeAlphaBlendedSprites">True, if flush can include stored alpha blended sprites (possibly breaking rendering order, unless you know what are doing), otherwise False.</param>
        public void Flush(bool includeAlphaBlendedSprites = true) {
            if (!IsBatching) {
                return;
            }

            if (Shader is IShaderTransform shader) {
                shader.World = Transform * shader.World;
            }

            if (Shader is BasicShader basicShader) {
                basicShader.TextureEnabled = true;
            }

            Render(ref _batchItems, _nextItemIndex, DepthStencilState);

            if (AutoHandleAlphaBlendedSprites && includeAlphaBlendedSprites) {
                Render(ref _transparencyBatchItems, _nextItemWithTransparencyIndex, DepthStencilState.DepthRead);

#if DEBUG

                SpriteCount += _nextItemWithTransparencyIndex;

#endif

                _nextItemWithTransparencyIndex = 0;
            }

#if DEBUG

            SpriteCount += _nextItemIndex;

#endif

            _nextItemIndex = 0;
        }

        #endregion Public Methods

        #region Private Methods

        private ref SpriteBatchItem GetBatchItem(bool needsTransparency) {
            if (needsTransparency) {
                if (_nextItemWithTransparencyIndex >= _transparencyBatchItems.Length) {
                    SetTransparencyBuffersCapacity(_transparencyBatchItems.Length + _transparencyBatchItems.Length / 2);
                }

                ref SpriteBatchItem transparencyBatchItem = ref _transparencyBatchItems[_nextItemWithTransparencyIndex];
                _nextItemWithTransparencyIndex++;

                return ref transparencyBatchItem;
            }

            if (_nextItemIndex >= _batchItems.Length) {
                SetBuffersCapacity(_batchItems.Length + _batchItems.Length / 2);
            }

            ref SpriteBatchItem batchItem = ref _batchItems[_nextItemIndex];
            _nextItemIndex++;

            return ref batchItem;
        }

        private void SetBuffersCapacity(int newBatchItemsCapacity) {
            if (_batchItems.Length >= newBatchItemsCapacity) {
                return;
            }

            int previousSize = _batchItems.Length;
            System.Array.Resize(ref _batchItems, newBatchItemsCapacity);
            System.Array.Resize(ref _vertexBuffer, newBatchItemsCapacity * 4);
            System.Array.Resize(ref _indexBuffer, newBatchItemsCapacity * 6);

            Initialize(previousSize);
        }

        private void SetTransparencyBuffersCapacity(int newBatchItemsCapacity) {
            if ( _transparencyBatchItems.Length >= newBatchItemsCapacity) {
                return;
            }

            int previousTransparencyItemsBatchSize = _transparencyBatchItems.Length;
            System.Array.Resize(ref _transparencyBatchItems, newBatchItemsCapacity);

            InitializeTransparencyItemsBuffers(previousTransparencyItemsBatchSize);
        }

        private void Initialize(int startIndex = 0) {
            for (int i = startIndex; i < _batchItems.Length; i++) {
                _batchItems[i] = new SpriteBatchItem();

                _indexBuffer[i * 6] = (short) (i * 4 + 3);
                _indexBuffer[i * 6 + 1] = (short) (i * 4);
                _indexBuffer[i * 6 + 2] = (short) (i * 4 + 2);

                _indexBuffer[i * 6 + 3] = (short) (i * 4 + 2);
                _indexBuffer[i * 6 + 4] = (short) (i * 4);
                _indexBuffer[i * 6 + 5] = (short) (i * 4 + 1);
            }
        }

        private void InitializeTransparencyItemsBuffers(int startIndex = 0) {
            for (int i = startIndex; i < _transparencyBatchItems.Length; i++) {
                _transparencyBatchItems[i] = new SpriteBatchItem();
            }
        }

        private void Render(ref SpriteBatchItem[] batchItems, int itemsCount, DepthStencilState depthStencilState) {
            if (itemsCount == 0) {
                return;
            }

            // pre-process batches, some modes demands it
            switch (BatchMode) {
                case BatchMode.DepthSortAscending:
                    System.Array.Sort(batchItems, 0, itemsCount, new BatchModeComparer.DepthAscending());
                    break;

                case BatchMode.DepthSortDescending:
                    System.Array.Sort(batchItems, 0, itemsCount, new BatchModeComparer.DepthDescending());
                    break;

                case BatchMode.DepthBuffer:
                    System.Array.Sort(batchItems, 0, itemsCount, new BatchModeComparer.DepthBuffer());
                    break;

                case BatchMode.DrawOrder:
                case BatchMode.Immediate:
                    break;

                default:
                    throw new System.NotImplementedException($"SpriteBatch doesn't implements BatchMode '{BatchMode}'.");
            }

            SpriteBatchItem batchItem = batchItems[0];
            Texture texture = batchItem.Texture;
            Shader shader = batchItem.Shader ?? Shader;
            IShaderParameters parameters = batchItem.ShaderParameters;

            int startIndex = 0,
                endIndex = 0;

            for (int i = 0; i < itemsCount; i++) {
                batchItem = batchItems[i];

                if (batchItem.Texture != texture
                  || (batchItem.Shader != shader && (batchItem.Shader != null || shader != Shader))
                  || (batchItem.ShaderParameters == null && parameters != null) || (batchItem.ShaderParameters != null && !batchItem.ShaderParameters.Equals(parameters))) {
                    DrawQuads(startIndex, endIndex - 1, texture, shader, parameters, depthStencilState);

                    texture = batchItem.Texture;
                    shader = batchItem.Shader ?? Shader;
                    parameters = batchItem.ShaderParameters;
                    startIndex = endIndex;
                }

                batchItem.VertexData.CopyTo(_vertexBuffer, endIndex * 4);
                batchItem.Clear();
                endIndex++;
            }

            DrawQuads(startIndex, endIndex - 1, texture, shader, parameters, depthStencilState);
        }

        private void DrawQuads(int startBatchIndex, int endBatchIndex, Texture texture, Shader shader, IShaderParameters parameters, DepthStencilState depthStencilState) {
            int batchCount = endBatchIndex - startBatchIndex + 1;

            if (shader is IShaderTexture currentShaderText) {
                currentShaderText.TextureEnabled = true;
                currentShaderText.Texture = texture;
            }

            if (AllowIBasicShaderEffectParameterClone && shader != Shader && Shader is IShaderTransform defaultShader && shader is IShaderTransform currentShaderTrans) {
                currentShaderTrans.World = defaultShader.World;
                currentShaderTrans.View = defaultShader.View;
                currentShaderTrans.Projection = defaultShader.Projection;
            }

            parameters?.ApplyParameters(shader);

            // prepare device
            GraphicsDevice.BlendState = BlendState;
            GraphicsDevice.SamplerStates[0] = SamplerState;
            GraphicsDevice.DepthStencilState = depthStencilState;
            GraphicsDevice.RasterizerState = RasterizerState;

            foreach (object pass in shader) {
                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertexBuffer,
                    startBatchIndex * 4,
                    batchCount * 4,
                    _indexBuffer,
                    0,
                    batchCount * 2,
                    VertexPositionColorTexture.VertexDeclaration
                );
            }

#if DEBUG
            TotalDrawCalls++;
#endif
        }

        #endregion Private Methods

        #region Internal Methods

#if DEBUG

        internal void DrawString(Font font, Text.RenderData glyphs, Rectangle destinationRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (!IsBatching) {
                throw new System.InvalidOperationException("Begin() must be called before any Draw() operation.");
            }

            bool applyRotation = false,
                 needsTransparency = AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue;

            float cos = 0f,
                  sin = 0f;

            if (!Util.Math.EqualsEstimate(rotation, 0f)) {
                applyRotation = true;
                cos = Util.Math.Cos(rotation);
                sin = Util.Math.Sin(rotation);
            }

            Size textSize = destinationRectangle.Size;

            foreach (Text.RenderData.Glyph glyph in glyphs) {
                Vector2 pos = new Vector2(glyph.X, glyph.Y);

                if ((flip & ImageFlip.Horizontal) != ImageFlip.None) {
                    pos.X = textSize.Width - pos.X - glyph.SourceArea.Width;
                }

                if ((flip & ImageFlip.Vertical) != ImageFlip.None) {
                    pos.Y = textSize.Height - pos.Y - glyph.SourceArea.Height;
                }

                pos = pos * scale - origin;

                if (applyRotation) {
                    pos = new Vector2(pos.X * cos - pos.Y * sin, pos.X * sin + pos.Y * cos);
                }

                ref SpriteBatchItem batchItem = ref GetBatchItem(needsTransparency);
                batchItem.Set(
                    font.RenderMap.Texture,
                    destinationRectangle.Position + pos,
                    glyph.SourceArea,
                    rotation,
                    scale,
                    flip,
                    color,
                    Vector2.Zero,
                    scroll,
                    shader,
                    shaderParameters,
                    layerDepth
                );
            }

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        internal static void ResetMetrics() {
            TotalDrawCalls = SpriteCount = 0;
        }

#endif

        #endregion Internal Methods
    }
}
