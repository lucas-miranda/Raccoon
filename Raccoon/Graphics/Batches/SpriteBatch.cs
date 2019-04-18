using System.Collections.Generic;

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
            GraphicsDevice = Game.Instance.GraphicsDevice;
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

        public GraphicsDevice GraphicsDevice { get; set; }
        public bool IsBatching { get; private set; }
        public Shader Shader { get; set; }
        public bool AutoHandleAlphaBlendedSprites { get; private set; }
        public bool AllowIBasicShaderEffectParameterClone { get; set; } = true;

        #endregion Public Properties

        #region Private Properties

        private BlendState BlendState { get; set; }
        private SamplerState SamplerState { get; set; }
        private DepthStencilState DepthStencilState { get; set; }
        private RasterizerState RasterizerState { get; set; }
        private Matrix Transform { get; set; }

        #endregion Private Properties

        #region Public Methods

        public void Begin(BlendState blendState = null, SamplerState sampler = null, DepthStencilState depthStencil = null, RasterizerState rasterizer = null, Matrix? transform = null) {
            BlendState = blendState ?? BlendState.AlphaBlend;
            SamplerState = sampler ?? SamplerState.PointClamp;
            DepthStencilState = depthStencil ?? DepthStencilState.None;
            RasterizerState = rasterizer ?? RasterizerState.CullNone;
            Transform = transform ?? Matrix.Identity;

            _nextItemIndex = _nextItemWithTransparencyIndex = 0;
            IsBatching = true;
        }

        public void End() {
            if (!IsBatching) {
                throw new System.InvalidOperationException("Begin() must be called before End().");
            }

            if (Shader is IBasicShader shader) {
                shader.World = Transform * shader.World;
            }

            if (Shader is BasicShader basicShader) {
                basicShader.TextureEnabled = true;
            }

            Render(ref _batchItems, _nextItemIndex, DepthStencilState);

            if (AutoHandleAlphaBlendedSprites) {
                Render(ref _transparencyBatchItems, _nextItemWithTransparencyIndex, DepthStencilState.DepthRead);
            }

            IsBatching = false;
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            ref SpriteBatchItem batchItem = ref GetBatchItem(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            ref SpriteBatchItem batchItem = ref GetBatchItem(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(texture, destinationRectangle, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            List<(Vector2, Rectangle)> glyphs = font.RenderMap.PrepareText(text, out Size textSize);

            float cos = 0f, 
                  sin = 0f;

            if (rotation != 0f) {
                cos = Util.Math.Cos(rotation);
                sin = Util.Math.Sin(rotation);
            }

            foreach ((Vector2 GlyphPosition, Rectangle SourceArea) in glyphs) {
                ref SpriteBatchItem batchItem = ref GetBatchItem(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);

                Vector2 pos = GlyphPosition;

                if ((flip & ImageFlip.Horizontal) != ImageFlip.None) {
                    pos.X = textSize.Width - pos.X - SourceArea.Width;
                }

                if ((flip & ImageFlip.Vertical) != ImageFlip.None) {
                    pos.Y = textSize.Height - pos.Y - SourceArea.Height;
                }

                pos *= scale;

                if (rotation != 0f) {
                    pos = origin + new Vector2(pos.X * cos - pos.Y * sin, pos.X * sin + pos.Y * cos);
                }

                batchItem.Set(
                    font.RenderMap.Texture,
                    position + pos,
                    SourceArea,
                    rotation,
                    scale,
                    flip,
                    color,
                    origin,
                    scroll,
                    shader,
                    layerDepth
                );
            }
        }

        #endregion Public Methods

        #region Private Methods

        private ref SpriteBatchItem GetBatchItem(bool needsTransparency) {
            if (needsTransparency) {
                if (_nextItemWithTransparencyIndex >= _transparencyBatchItems.Length) {
                    SetTransparencyBuffersCapacity(_batchItems.Length / 2);
                }

                ref SpriteBatchItem transparencyBatchItem = ref _transparencyBatchItems[_nextItemWithTransparencyIndex];
                _nextItemWithTransparencyIndex++;

                return ref transparencyBatchItem;
            }

            if (_nextItemIndex >= _batchItems.Length) {
                SetBuffersCapacity(_batchItems.Length * 2);
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

            System.Array.Sort(batchItems, 0, itemsCount);

            GraphicsDevice.BlendState = BlendState;
            GraphicsDevice.SamplerStates[0] = SamplerState;
            GraphicsDevice.DepthStencilState = depthStencilState;
            GraphicsDevice.RasterizerState = RasterizerState;

            Texture texture = batchItems[0].Texture;
            Shader shader = batchItems[0].Shader ?? Shader;

            int startIndex = 0,
                endIndex = 0;

            for (int i = 0; i < itemsCount; i++) {
                SpriteBatchItem batchItem = batchItems[i];

                if (batchItem.Texture != texture || (batchItem.Shader != shader && (batchItem.Shader != null || shader != Shader))) {
                    DrawQuads(startIndex, endIndex - 1, texture, shader);
                    texture = batchItem.Texture;
                    shader = batchItem.Shader ?? Shader;
                    startIndex = endIndex;
                }

                batchItem.Texture = null;
                batchItem.Shader = null;
                batchItem.VertexData.CopyTo(_vertexBuffer, endIndex * 4);
                endIndex++;
            }

            DrawQuads(startIndex, endIndex - 1, texture, shader);
        }

        private void DrawQuads(int startBatchIndex, int endBatchIndex, Texture texture, Shader shader) {
            int batchCount = endBatchIndex - startBatchIndex + 1;

            if (shader is IBasicShader currentShader) {
                currentShader.Texture = texture;

                if (AllowIBasicShaderEffectParameterClone && currentShader != Shader && Shader is IBasicShader defaultShader) {
                    currentShader.World = defaultShader.World;
                    currentShader.View = defaultShader.View;
                    currentShader.Projection = defaultShader.Projection;
                }
            } 
            
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
        }

        #endregion Private Methods
    }
}
