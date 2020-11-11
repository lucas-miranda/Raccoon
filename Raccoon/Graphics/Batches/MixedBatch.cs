using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    public class MixedBatch {
        #region Public Members

        public const int StartBatchItemsCount = 100;

        public static readonly DepthStencilState DefaultDepthReadStencilState = new DepthStencilState() {
                                                     Name = "MixedBatch.DepthReadState",
                                                     DepthBufferEnable = true,
                                                     DepthBufferWriteEnable = false,
                                                     DepthBufferFunction = CompareFunction.LessEqual,
                                                     StencilEnable = false,
                                                     StencilFunction = CompareFunction.Always,
                                                     StencilPass = StencilOperation.Keep,
                                                     StencilFail = StencilOperation.Keep,
                                                     StencilDepthBufferFail = StencilOperation.Keep,
                                                     TwoSidedStencilMode = false,
                                                     CounterClockwiseStencilFunction = CompareFunction.Always,
                                                     CounterClockwiseStencilFail = StencilOperation.Keep,
                                                     CounterClockwiseStencilPass = StencilOperation.Keep,
                                                     CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep,
                                                     StencilMask = System.Int32.MaxValue,
                                                     StencilWriteMask = System.Int32.MaxValue,
                                                     ReferenceStencil = 0
                                                 };

        #endregion Public Members

        #region Private Members

        private VertexPositionColorTexture[] _vertexPreBuffer;
        private int[] _indexPreBuffer;
        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private int _vertexBufferBeginOffset, _vertexBufferEndOffset, _indexBufferBeginOffset, _indexBufferEndOffset;

        private IBatchItem[] _batchItems = new IBatchItem[StartBatchItemsCount],
                            _transparencyBatchItems = null;

        private int _nextItemIndex, _nextItemWithTransparencyIndex;

        #endregion Private Members

        public MixedBatch(GraphicsDevice graphicsDevice = null, bool autoHandleAlphaBlendedSprites = false) {
            GraphicsDevice = graphicsDevice ?? Game.Instance.GraphicsDevice;
            Shader = Game.Instance.BasicShader;
            DepthReadState = DefaultDepthReadStencilState;

            AutoHandleAlphaBlendedSprites = autoHandleAlphaBlendedSprites;
            if (AutoHandleAlphaBlendedSprites) {
                _transparencyBatchItems = new IBatchItem[_batchItems.Length / 2];
            }

            _vertexBuffer = new DynamicVertexBuffer(
                GraphicsDevice,
                VertexPositionColorTexture.VertexDeclaration,
                _batchItems.Length,
                BufferUsage.WriteOnly
            );

            _indexBuffer = new DynamicIndexBuffer(
                GraphicsDevice,
                IndexElementSize.ThirtyTwoBits,
                _batchItems.Length,
                BufferUsage.WriteOnly
            );

            _vertexPreBuffer = new VertexPositionColorTexture[_vertexBuffer.VertexCount];
            _indexPreBuffer = new int[_indexBuffer.IndexCount];
        }

        #region Public Properties

        public static int TotalDrawCalls { get; private set; }
        public static int TotalSpriteDrawCalls { get; private set; }
        public static int TotalHollowPrimitiveDrawCalls { get; private set; }
        public static int TotalFilledPrimitiveDrawCalls { get; private set; }
        public static int PrimitivesCount { get; private set; }
        public static int SpriteCount { get; private set; }

        public GraphicsDevice GraphicsDevice { get; set; }
        public bool IsBatching { get; private set; }
        public Shader Shader { get; set; }
        public bool AutoHandleAlphaBlendedSprites { get; private set; }
        public bool AllowIBasicShaderEffectParameterClone { get; set; } = true;
        public BatchMode BatchMode { get; private set; }
        public BlendState BlendState { get; private set; }
        public SamplerState SamplerState { get; private set; }
        public DepthStencilState DepthStencilState { get; private set; }
        public RasterizerState RasterizerState { get; private set; }
        public Matrix Transform { get; private set; }
        public DepthStencilState DepthReadState { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void Begin(BatchMode batchMode = BatchMode.DrawOrder, BlendState blendState = null, SamplerState sampler = null, DepthStencilState depthStencil = null, RasterizerState rasterizer = null, Matrix? transform = null) {
            BatchMode = batchMode;
            BlendState = blendState ?? BlendState.AlphaBlend;
            SamplerState = sampler ?? SamplerState.PointClamp;
            DepthStencilState = depthStencil ?? DepthStencilState.Default;
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

        #region Draw with Texture

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            SpriteBatchItem batchItem = GetBatchItem<SpriteBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            Draw(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, null, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            SpriteBatchItem batchItem = GetBatchItem<SpriteBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(texture, destinationRectangle, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void Draw(Texture texture, Vector2[] vertices, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            SpriteBatchItem batchItem = GetBatchItem<SpriteBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(texture, vertices, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void Draw(Texture texture, VertexPositionColorTexture[] vertexData, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            SpriteBatchItem batchItem = GetBatchItem<SpriteBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(texture, vertexData, position, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        #endregion Draw with Texture

        #region Draw String

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            Text.RenderData glyphs = font.RenderMap.PrepareText(text, out Size textSize);
            DrawString(font, glyphs, new Rectangle(position, textSize), rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 0f) {
            DrawString(font, text, position, rotation, scale, flip, color, origin, scroll, shader, shaderParameters: null, layerDepth);
        }

        public void DrawString(Font font, Text.RenderData glyphs, int glyphStartIndex, int glyphCount, Rectangle destinationRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            if (glyphCount == 0) {
                return;
            }

            if (glyphs.GlyphCount <= 0) {
                throw new System.InvalidOperationException("Glyphs are empty.");
            }

            if (glyphStartIndex < 0 || glyphStartIndex >= glyphs.GlyphCount) {
                throw new System.ArgumentOutOfRangeException(nameof(glyphStartIndex), $"Glyph should be zero or a positive integer in range [0, {glyphs.GlyphCount - 1}]");
            }

            if (glyphStartIndex + glyphCount > glyphs.GlyphCount) {
                throw new System.ArgumentOutOfRangeException(nameof(glyphCount), $"Selected glyph range [{glyphStartIndex}, {glyphStartIndex + glyphCount - 1}] is out of bounds [0, {glyphs.GlyphCount - 1}]");
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


            if (!applyRotation && flip == ImageFlip.None) {
                for (int i = 0; i < glyphCount; i++) {
                    Text.RenderData.Glyph glyph = glyphs[glyphStartIndex + i];
                    DrawGlyph(glyph, glyph.Position * scale - origin);
                }
            } else {
                for (int i = 0; i < glyphCount; i++) {
                    Text.RenderData.Glyph glyph = glyphs[glyphStartIndex + i];
                    Vector2 pos = glyph.Position;

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

                    DrawGlyph(glyph, pos);
                }
            }

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }

            return;

            void DrawGlyph(Text.RenderData.Glyph glyph, Vector2 pos) {
                SpriteBatchItem batchItem = GetBatchItem<SpriteBatchItem>(needsTransparency);
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
        }

        public void DrawString(Font font, Text.RenderData glyphs, Rectangle destinationRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            DrawString(
                font,
                glyphs,
                glyphStartIndex: 0,
                glyphCount: glyphs.GlyphCount,
                destinationRectangle,
                rotation,
                scale,
                flip,
                color,
                origin,
                scroll,
                shader,
                shaderParameters,
                layerDepth
            );
        }

        #endregion Draw String

        #region Line

        public void DrawLines(IList<Vector2> points, Vector2 position, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, bool cyclic = true, float layerDepth = 1f) {
            PreDrawItemCheck();

            VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[points.Count];

            int[] indexData;
            if (cyclic) {
                indexData = new int[points.Count * 2];
            } else {
                indexData = new int[(points.Count * 2) - 2];
            }

            int i = 0;
            if (rotation % 360 != 0) {
                for (int j = 0; j < points.Count; j++) {
                    Vector2 point = points[j];
                    vertexData[i] = new VertexPositionColorTexture(
                        new Vector3(position + Math.Rotate(point * scale - origin, rotation), layerDepth),
                        color,
                        Microsoft.Xna.Framework.Vector2.Zero
                    );

                    // only add index if isn't last point or is cyclic
                    if (j < points.Count - 1 || cyclic) {
                        indexData[2 * i] = i;
                        indexData[2 * i + 1] = (i + 1) % vertexData.Length;
                    }

                    i++;
                }
            } else {
                for (int j = 0; j < points.Count; j++) {
                    Vector2 point = points[j];
                    vertexData[i] = new VertexPositionColorTexture(
                        new Vector3(position + point * scale - origin, layerDepth), 
                        color,
                        Microsoft.Xna.Framework.Vector2.Zero
                    );

                    if (j < points.Count - 1 || cyclic) {
                        indexData[2 * i] = i;
                        indexData[2 * i + 1] = (i + 1) % vertexData.Length;
                    }

                    i++;
                }
            }

            PrimitiveBatchItem batchItem = GetBatchItem<PrimitiveBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(
                vertexData,
                indexData,
                isHollow: true,
                shader,
                shaderParameters,
                texture: null
            );

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        #endregion Line

        #region Rectangle

        public void DrawFilledRectangle(Vector2 position, Size size, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            Vector2 topLeft,
                    topRight,
                    bottomRight,
                    bottomLeft;

            if (rotation == 0f) {
                topLeft = position - origin;
                topRight = position - origin + new Vector2(size.Width, 0f) * scale;
                bottomRight = position - origin + size * scale;
                bottomLeft = position - origin + new Vector2(0f, size.Height) * scale;
            } else {
                float cos = Math.Cos(rotation),
                      sin = Math.Sin(rotation);

                topLeft = position + new Vector2((-origin.X) * cos - (-origin.Y) * sin, (-origin.X) * sin + (-origin.Y) * cos);

                topRight = new Vector2(size.Width, 0f) * scale - origin;
                topRight = position + new Vector2(topRight.X * cos - topRight.Y * sin, topRight.X * sin + topRight.Y * cos);

                bottomRight = (size * scale).ToVector2() - origin;
                bottomRight = position + new Vector2(bottomRight.X * cos - bottomRight.Y * sin, bottomRight.X * sin + bottomRight.Y * cos);

                bottomLeft = new Vector2(0f, size.Height) * scale - origin;
                bottomLeft = position + new Vector2(bottomLeft.X * cos - bottomLeft.Y * sin, bottomLeft.X * sin + bottomLeft.Y * cos);
            }

            VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[] {
                new VertexPositionColorTexture(
                    new Vector3(topLeft, layerDepth), 
                    color, 
                    Microsoft.Xna.Framework.Vector2.Zero
                ),
                new VertexPositionColorTexture(
                    new Vector3(topRight, layerDepth), 
                    color, 
                    Microsoft.Xna.Framework.Vector2.Zero
                ),
                new VertexPositionColorTexture(
                    new Vector3(bottomRight, layerDepth), 
                    color, 
                    Microsoft.Xna.Framework.Vector2.Zero
                ),
                new VertexPositionColorTexture(
                    new Vector3(bottomLeft, layerDepth), 
                    color, 
                    Microsoft.Xna.Framework.Vector2.Zero
                )
            };

            int[] indexData = new int[] {
                3, 0, 2,
                2, 0, 1
            };

            PrimitiveBatchItem batchItem = GetBatchItem<PrimitiveBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(
                vertexData,
                indexData,
                isHollow: false,
                shader,
                shaderParameters,
                texture: null
            );

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void DrawHollowRectangle(Vector2 position, Size size, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            Vector2 topLeft,
                    topRight,
                    bottomRight,
                    bottomLeft;

            if (Math.EqualsEstimate(rotation, 0f)) {
                topLeft = position - origin;
                topRight = position - origin + new Vector2(size.Width, 0f) * scale;
                bottomRight = position - origin + size * scale;
                bottomLeft = position - origin + new Vector2(0f, size.Height) * scale;
            } else {
                float cos = Math.Cos(rotation),
                      sin = Math.Sin(rotation);

                topLeft = position + new Vector2((-origin.X) * cos - (-origin.Y) * sin, (-origin.X) * sin + (-origin.Y) * cos);

                topRight = new Vector2(size.Width, 0f) * scale - origin;
                topRight = position + new Vector2(topRight.X * cos - topRight.Y * sin, topRight.X * sin + topRight.Y * cos);

                bottomRight = (size * scale).ToVector2() - origin;
                bottomRight = position + new Vector2(bottomRight.X * cos - bottomRight.Y * sin, bottomRight.X * sin + bottomRight.Y * cos);

                bottomLeft = new Vector2(0f, size.Height) * scale - origin;
                bottomLeft = position + new Vector2(bottomLeft.X * cos - bottomLeft.Y * sin, bottomLeft.X * sin + bottomLeft.Y * cos);
            }

            VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[] {
                new VertexPositionColorTexture(
                    new Vector3(topLeft, layerDepth), 
                    color,
                    Microsoft.Xna.Framework.Vector2.Zero
                ),
                new VertexPositionColorTexture(
                    new Vector3(topRight, layerDepth), 
                    color,
                    Microsoft.Xna.Framework.Vector2.Zero
                ),
                new VertexPositionColorTexture(
                    new Vector3(bottomRight, layerDepth), 
                    color,
                    Microsoft.Xna.Framework.Vector2.Zero
                ),
                new VertexPositionColorTexture(
                    new Vector3(bottomLeft, layerDepth), 
                    color,
                    Microsoft.Xna.Framework.Vector2.Zero
                )
            };

            int[] indexData = new int[] {
                0, 1,
                1, 2,
                2, 3,
                3, 0
            };

            PrimitiveBatchItem batchItem = GetBatchItem<PrimitiveBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(
                vertexData,
                indexData,
                isHollow: true,
                shader,
                shaderParameters,
                texture: null
            );

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        #endregion Rectangle

        #region Circle

        public void DrawFilledCircle(Vector2 center, float radius, Color color, float scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, int segments = 0, float layerDepth = 1f) {
            PreDrawItemCheck();

            radius *= scale;
            center -= origin;

            if (segments <= 0) {
                segments = (int) (radius + radius);
            }

            VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[segments + 1];
            int[] indexData = new int[(segments + 1) * 3];

            // update vertices
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            float theta = (float) (2.0 * Math.PI / segments);
            float t, c = (float) System.Math.Cos(theta), s = (float) System.Math.Sin(theta); // precalculate the sine and cosine

            float x = radius * Math.Cos(0),
                  y = radius * Math.Sin(0);

            // center
            int centerIndex = vertexData.Length - 1;
            vertexData[centerIndex] = new VertexPositionColorTexture(
                new Vector3(center.X, center.Y, layerDepth), 
                color,
                Microsoft.Xna.Framework.Vector2.Zero
            );

            int i;
            for (i = 0; i < vertexData.Length; i++) {
                vertexData[i] = new VertexPositionColorTexture(
                    new Vector3(center.X + x, center.Y + y, layerDepth), 
                    color,
                    Microsoft.Xna.Framework.Vector2.Zero
                );

                // apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;

                indexData[i * 3] = centerIndex; // circle center
                indexData[i * 3 + 1] = i; // current vertex
                indexData[i * 3 + 2] = (i + 1) % vertexData.Length; // next vertex (cyclic)
            }

            PrimitiveBatchItem batchItem = GetBatchItem<PrimitiveBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(
                vertexData,
                indexData,
                isHollow: false,
                shader,
                shaderParameters,
                texture: null
            );

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void DrawHollowCircle(Vector2 center, float radius, Color color, float scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, int segments = 0, float layerDepth = 1f) {
            PreDrawItemCheck();

            radius *= scale;
            center -= origin;

            if (segments <= 0) {
                segments = (int) (radius + radius);
            }

            VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[segments];
            int[] indexData = new int[segments * 2];

            // update vertices
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            float theta = (float) (2.0 * Math.PI / segments);
            float t, c = (float) System.Math.Cos(theta), s = (float) System.Math.Sin(theta); // precalculate the sine and cosine

            float x = radius * Math.Cos(0),
                  y = radius * Math.Sin(0);

            for (int i = 0; i < vertexData.Length; i++) {
                vertexData[i] = new VertexPositionColorTexture(
                    new Vector3(center.X + x, center.Y + y, layerDepth), 
                    color,
                    Microsoft.Xna.Framework.Vector2.Zero
                );

                // apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;

                indexData[2 * i] = i; // current vertex
                indexData[2 * i + 1] = (i + 1) % vertexData.Length;
            }

            PrimitiveBatchItem batchItem = GetBatchItem<PrimitiveBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(
                vertexData,
                indexData,
                isHollow: true,
                shader,
                shaderParameters,
                texture: null
            );

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        #endregion Circle

        #region Polygon

        public void DrawFilledPolygon(Polygon polygon, Vector2 position, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[polygon.VertexCount + 1];
            int centerVertexId = vertexData.Length - 1;

            int[] indexData = new int[polygon.VertexCount * 3];

            int i = 0;
            if (rotation % 360 != 0) {
                vertexData[centerVertexId] = new VertexPositionColorTexture(
                    new Vector3(position + Math.Rotate(polygon.Center * scale - origin, rotation), layerDepth),
                    color,
                    Microsoft.Xna.Framework.Vector2.Zero
                );

                foreach (Vector2 point in polygon) {
                    vertexData[i] = new VertexPositionColorTexture(
                        new Vector3(position + Math.Rotate(point * scale - origin, rotation), layerDepth),
                        color,
                        Microsoft.Xna.Framework.Vector2.Zero
                    );

                    indexData[3 * i] = i;
                    indexData[3 * i + 1] = (i + 1) % polygon.VertexCount;
                    indexData[3 * i + 2] = centerVertexId;
                    i++;
                }
            } else {
                vertexData[centerVertexId] = new VertexPositionColorTexture(
                    new Vector3(position + polygon.Center * scale - origin, layerDepth), 
                    color,
                    Microsoft.Xna.Framework.Vector2.Zero
                );

                foreach (Vector2 point in polygon) {
                    vertexData[i] = new VertexPositionColorTexture(
                        new Vector3(position + point * scale - origin, layerDepth), 
                        color,
                        Microsoft.Xna.Framework.Vector2.Zero
                    );

                    indexData[3 * i] = i;
                    indexData[3 * i + 1] = (i + 1) % polygon.VertexCount;
                    indexData[3 * i + 2] = centerVertexId;
                    i++;
                }
            }

            PrimitiveBatchItem batchItem = GetBatchItem<PrimitiveBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(
                vertexData,
                indexData,
                isHollow: false,
                shader,
                shaderParameters,
                texture: null
            );

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void DrawHollowPolygon(IList<Vector2> points, Vector2 position, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            DrawLines(points, position, color, rotation, scale, origin, scroll, shader, shaderParameters, cyclic: true, layerDepth);
        }

        public void DrawHollowPolygon(Polygon polygon, Vector2 position, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            DrawLines(polygon.Vertices, position, color, rotation, scale, origin, scroll, shader, shaderParameters, cyclic: true, layerDepth);
        }

        #endregion Polygon

        #region Others

        public void DrawVertices(IList<Vector2> vertices, int minVertexIndex, int verticesLength, int[] indices, int minIndex, int primitivesCount, bool isHollow, Vector2 position, float rotation, Vector2 scale, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[verticesLength];

            if (rotation != 0f) { 
                float cos = Math.Cos(rotation),
                      sin = Math.Sin(rotation);

                for (int i = minVertexIndex; i < verticesLength; i++) {
                    Vector2 v = vertices[i] * scale - origin;

                    vertexData[i] = new VertexPositionColorTexture(
                        new Vector3(
                            position.X + v.X * cos - v.Y * sin, 
                            position.Y + v.X * sin + v.Y * cos, 
                            layerDepth
                        ),
                        color,
                        Microsoft.Xna.Framework.Vector2.Zero
                    );
                }
            } else {
                for (int i = minVertexIndex; i < verticesLength; i++) {
                    Vector2 v = position + vertices[i] * scale - origin;

                    vertexData[i] = new VertexPositionColorTexture(
                        new Vector3(v.X, v.Y, layerDepth),
                        color,
                        Microsoft.Xna.Framework.Vector2.Zero
                    );
                }
            }

            int indicesCount;

            if (isHollow) {
                // implemented as line list
                indicesCount = primitivesCount * 2;
            } else {
                // implemented as triangle list
                indicesCount = primitivesCount * 3;
            }

            int[] indexData = new int[indicesCount];
            System.Array.Copy(indices, minIndex, indexData, 0, indicesCount);

            PrimitiveBatchItem batchItem = GetBatchItem<PrimitiveBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(
                vertexData,
                indexData,
                isHollow,
                shader,
                shaderParameters,
                texture: null
            );

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        public void DrawVertices(Texture texture, VertexPositionColorTexture[] vertexData, int minVertexIndex, int verticesLength, int[] indices, int minIndex, int primitivesCount, bool isHollow, Vector2 position, float rotation, Vector2 scale, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            PreDrawItemCheck();

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[verticesLength];

            if (rotation != 0f) { 
                float cos = Math.Cos(rotation),
                      sin = Math.Sin(rotation);

                for (int i = minVertexIndex; i < verticesLength; i++) {
                    VertexPositionColorTexture vertex = vertexData[i];
                    Vector2 v = new Vector2(vertex.Position.X, vertex.Position.Y) * scale - origin;

                    vertices[i] = new VertexPositionColorTexture(
                        new Vector3(
                            position.X + v.X * cos - v.Y * sin, 
                            position.Y + v.X * sin + v.Y * cos, 
                            layerDepth
                        ),
                        new Microsoft.Xna.Framework.Color(
                            (int) ((color.R * vertex.Color.R) / 255f),
                            (int) ((color.G * vertex.Color.G) / 255f),
                            (int) ((color.B * vertex.Color.B) / 255f),
                            (int) ((color.A * vertex.Color.A) / 255f)
                        ),
                        vertex.TextureCoordinate
                    );
                }
            } else {
                for (int i = minVertexIndex; i < verticesLength; i++) {
                    VertexPositionColorTexture vertex = vertexData[i];
                    Vector2 v = new Vector2(vertex.Position.X, vertex.Position.Y) * scale - origin;

                    vertices[i] = new VertexPositionColorTexture(
                        new Vector3(
                            position.X + v.X, 
                            position.Y + v.Y, 
                            layerDepth
                        ),
                        new Microsoft.Xna.Framework.Color(
                            (int) ((color.R * vertex.Color.R) / 255f),
                            (int) ((color.G * vertex.Color.G) / 255f),
                            (int) ((color.B * vertex.Color.B) / 255f),
                            (int) ((color.A * vertex.Color.A) / 255f)
                        ),
                        vertex.TextureCoordinate
                    );
                }
            }

            int indicesCount;

            if (isHollow) {
                // implemented as line list
                indicesCount = primitivesCount * 2;
            } else {
                // implemented as triangle list
                indicesCount = primitivesCount * 3;
            }

            int[] indexData = new int[indicesCount];
            System.Array.Copy(indices, minIndex, indexData, 0, indicesCount);

            PrimitiveBatchItem batchItem = GetBatchItem<PrimitiveBatchItem>(AutoHandleAlphaBlendedSprites && color.A < byte.MaxValue);
            batchItem.Set(
                vertices,
                indexData,
                isHollow,
                shader,
                shaderParameters,
                texture
            );

            if (BatchMode == BatchMode.Immediate) {
                Flush();
            }
        }

        #endregion Others

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
                Render(ref _transparencyBatchItems, _nextItemWithTransparencyIndex, DepthReadState);

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

        private T GetBatchItem<T>(bool needsTransparency) where T : IBatchItem {
            if (needsTransparency) {
                if (_nextItemWithTransparencyIndex >= _transparencyBatchItems.Length) {
                    SetTransparencyBuffersCapacity(_transparencyBatchItems.Length + _transparencyBatchItems.Length / 2);
                }

                ref IBatchItem genericTransparencyBatchItem = ref _transparencyBatchItems[_nextItemWithTransparencyIndex];
                _nextItemWithTransparencyIndex++;

                if (genericTransparencyBatchItem is T transparencyBatchItem && transparencyBatchItem != null) {
                    return transparencyBatchItem;
                }

                genericTransparencyBatchItem = (T) System.Activator.CreateInstance(typeof(T));
                return (T) genericTransparencyBatchItem;
            }

            if (_nextItemIndex >= _batchItems.Length) {
                SetBuffersCapacity(_batchItems.Length + _batchItems.Length / 2);
            }

            ref IBatchItem genericBatchItem = ref _batchItems[_nextItemIndex];
            _nextItemIndex++;

            if (genericBatchItem is T batchItem && batchItem != null) {
                return batchItem;
            }

            genericBatchItem = (T) System.Activator.CreateInstance(typeof(T));
            return (T) genericBatchItem;
        }

        private void SetBuffersCapacity(int newBatchItemsCapacity) {
            if (_batchItems.Length >= newBatchItemsCapacity) {
                return;
            }

            //int previousSize = _batchItems.Length;
            System.Array.Resize(ref _batchItems, newBatchItemsCapacity);
            //System.Array.Resize(ref _vertexBuffer, newBatchItemsCapacity * 4);
            //System.Array.Resize(ref _indexBuffer, newBatchItemsCapacity * 6);

            //Initialize(previousSize);
        }

        private void SetTransparencyBuffersCapacity(int newBatchItemsCapacity) {
            if ( _transparencyBatchItems.Length >= newBatchItemsCapacity) {
                return;
            }

            //int previousTransparencyItemsBatchSize = _transparencyBatchItems.Length;
            System.Array.Resize(ref _transparencyBatchItems, newBatchItemsCapacity);

            //InitializeTransparencyItemsBuffers(previousTransparencyItemsBatchSize);
        }

        private void Render(ref IBatchItem[] batchItems, int itemsCount, DepthStencilState depthStencilState) {
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

                case BatchMode.DepthBufferDescending:
                    System.Array.Sort(batchItems, 0, itemsCount, new BatchModeComparer.DepthBufferDescending());
                    break;

                case BatchMode.DrawOrder:
                case BatchMode.Immediate:
                    break;

                default:
                    throw new System.NotImplementedException($"SpriteBatch doesn't implements BatchMode '{BatchMode}'.");
            }

            IBatchItem batchItem = batchItems[0];
            System.Type batchItemType = batchItem.GetType();
            Texture texture = batchItem.Texture;
            Shader shader = batchItem.Shader ?? Shader;
            IShaderParameters parameters = batchItem.ShaderParameters;

            // primitive batch item
            bool isHollow = false;
            if (batchItem is PrimitiveBatchItem firstPrimitiveBatchItem) {
                isHollow = firstPrimitiveBatchItem.IsHollow;
            }

            int startIndex = 0,
                endIndex = 0;

            _vertexBufferBeginOffset = _vertexBufferEndOffset
                = _indexBufferBeginOffset = _indexBufferEndOffset = 0;

            for (int i = 0; i < itemsCount; i++) {
                batchItem = batchItems[i];

                if (batchItem.Texture != texture
                  || batchItem.GetType() != batchItemType
                  || (batchItem.Shader != shader && (batchItem.Shader != null || shader != Shader))
                  || (batchItem.ShaderParameters == null && parameters != null) || (batchItem.ShaderParameters != null && !batchItem.ShaderParameters.Equals(parameters))
                  || (batchItem is PrimitiveBatchItem pbi && pbi.IsHollow != isHollow)) {
                    Draw();

                    batchItemType = batchItem.GetType();
                    texture = batchItem.Texture;
                    shader = batchItem.Shader ?? Shader;
                    parameters = batchItem.ShaderParameters;
                    startIndex = endIndex;
                    _vertexBufferEndOffset = 0;
                    _indexBufferEndOffset = 0;
                }
                
                // check buffers and reallocate them if needed
                bool needRecreateVertexBuffer = _vertexBuffer.VertexCount < _vertexBufferEndOffset + batchItem.VertexData.Length,
                     needRecreateIndexBuffer = _indexBuffer.IndexCount < _indexBufferEndOffset + batchItem.IndexData.Length;

                if (needRecreateVertexBuffer || needRecreateIndexBuffer) {
                    if (needRecreateIndexBuffer) {
                        int newIndexBufferSize = (int) Math.Ceiling((_indexBufferEndOffset + batchItem.IndexData.Length) * 1.5f);
                        _indexBuffer = new DynamicIndexBuffer(
                            GraphicsDevice,
                            IndexElementSize.ThirtyTwoBits,
                            newIndexBufferSize,
                            BufferUsage.WriteOnly
                        );

                        System.Array.Resize(ref _indexPreBuffer, _indexBuffer.IndexCount);
                    }

                    if (needRecreateVertexBuffer) {
                        int newVertexBufferSize = (int) Math.Ceiling((_vertexBufferEndOffset + batchItem.VertexData.Length) * 1.5f);
                        _vertexBuffer = new DynamicVertexBuffer(
                            GraphicsDevice,
                            VertexPositionColorTexture.VertexDeclaration,
                            newVertexBufferSize,
                            BufferUsage.WriteOnly
                        );

                        System.Array.Resize(ref _vertexPreBuffer, _vertexBuffer.VertexCount);
                    }
                }

                // buffers 

                batchItem.VertexData.CopyTo(_vertexPreBuffer, _vertexBufferEndOffset);

                /*
                _vertexBuffer.SetData(
                    _vertexBufferEndOffset * VertexPositionColorTexture.VertexDeclaration.VertexStride,
                    batchItem.VertexData,
                    startIndex: 0,
                    batchItem.VertexData.Length,
                    VertexPositionColorTexture.VertexDeclaration.VertexStride,
                    SetDataOptions.None
                );
                */

                for (int j = 0; j < batchItem.IndexData.Length; j++) {
                    _indexPreBuffer[_indexBufferEndOffset + j] = _vertexBufferEndOffset + batchItem.IndexData[j];
                }

                /*
                _indexBuffer.SetData(
                    _indexBufferEndOffset * sizeof(int),
                    preparedIndexData,
                    startIndex: 0,
                    batchItem.IndexData.Length
                );
                */

                //

                _vertexBufferEndOffset += batchItem.VertexData.Length;
                _indexBufferEndOffset += batchItem.IndexData.Length;

                batchItem.Clear();
                endIndex++;
            }

            Draw();
            _vertexBufferEndOffset = 0;
            _indexBufferEndOffset = 0;
            return;

            void Draw() {
                _vertexBuffer.SetData(
                    offsetInBytes: 0,
                    _vertexPreBuffer,
                    startIndex: 0,
                    _vertexPreBuffer.Length,
                    VertexPositionColorTexture.VertexDeclaration.VertexStride,
                    SetDataOptions.None
                );

                _indexBuffer.SetData(
                    offsetInBytes: 0,
                    _indexPreBuffer,
                    startIndex: 0,
                    _indexPreBuffer.Length
                );

                if (batchItemType == typeof(PrimitiveBatchItem)) {
                    DrawPrimitiveBatchItem(
                        startIndex, 
                        endIndex - 1, 
                        texture, 
                        shader, 
                        parameters, 
                        depthStencilState,
                        isHollow
                    );
                } else if (batchItemType == typeof(SpriteBatchItem)) {
                        DrawSpriteBatchItem(
                            startIndex, 
                            endIndex - 1, 
                            texture, 
                            shader, 
                            parameters, 
                            depthStencilState
                        );
                } else {
                        throw new System.NotImplementedException($"Batch item '{batchItem.GetType().Name}'");
                }

                if (batchItem is PrimitiveBatchItem pbi) {
                    isHollow = pbi.IsHollow;
                }
            }
        }

        private void PrepareDraw(Texture texture, Shader shader, IShaderParameters parameters, DepthStencilState depthStencilState) {
            if (shader is IShaderTexture currentShaderText) {
                if (texture != null) {
                    currentShaderText.TextureEnabled = true;
                    currentShaderText.Texture = texture;
                } else {
                    currentShaderText.TextureEnabled = false;
                    currentShaderText.Texture = null;
                }
            }

            if (AllowIBasicShaderEffectParameterClone && shader != Shader && Shader is IShaderTransform defaultShader && shader is IShaderTransform currentShaderTrans) {
                currentShaderTrans.World = defaultShader.World;
                currentShaderTrans.View = defaultShader.View;
                currentShaderTrans.Projection = defaultShader.Projection;
            }

            if (shader is IShaderDepthWrite currentShaderDepthWrite) {
                currentShaderDepthWrite.DepthWriteEnabled = depthStencilState.DepthBufferEnable;
            }

            parameters?.ApplyParameters(shader);

            // prepare device
            GraphicsDevice.BlendState = BlendState;
            GraphicsDevice.SamplerStates[0] = SamplerState;
            GraphicsDevice.DepthStencilState = depthStencilState;
            GraphicsDevice.RasterizerState = RasterizerState;
        }

        private void DrawSpriteBatchItem(int startBatchIndex, int endBatchIndex, Texture texture, Shader shader, IShaderParameters parameters, DepthStencilState depthStencilState) {
            int batchCount = endBatchIndex - startBatchIndex + 1;
            PrepareDraw(texture, shader, parameters, depthStencilState);

            foreach (object pass in shader) {
                GraphicsDevice.Indices = _indexBuffer;
                GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    _vertexBufferBeginOffset,
                    _vertexBufferEndOffset - _vertexBufferBeginOffset,
                    _indexBufferBeginOffset,
                    batchCount * 2
                );
            }

#if DEBUG
            TotalDrawCalls++;
#endif
        }

        private void DrawPrimitiveBatchItem(int startBatchIndex, int endBatchIndex, Texture texture, Shader shader, IShaderParameters parameters, DepthStencilState depthStencilState, bool isHollow) {
            int batchCount = endBatchIndex - startBatchIndex + 1;
            PrepareDraw(texture, shader, parameters, depthStencilState);

            if (isHollow) {
                foreach (object pass in shader) {
                    GraphicsDevice.Indices = _indexBuffer;
                    GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                    GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.LineList,
                        baseVertex: 0,
                        _vertexBufferBeginOffset,
                        _vertexBufferEndOffset - _vertexBufferBeginOffset,
                        _indexBufferBeginOffset,
                        _indexBufferEndOffset / 2
                    );
                }
            } else {
                foreach (object pass in shader) {
                    GraphicsDevice.Indices = _indexBuffer;
                    GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                    GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        baseVertex: 0,
                        _vertexBufferBeginOffset,
                        _vertexBufferEndOffset - _vertexBufferBeginOffset,
                        _indexBufferBeginOffset,
                        _indexBufferEndOffset / 3
                    );
                }
            }

#if DEBUG
            TotalDrawCalls++;
#endif
        }

        private void PreDrawItemCheck() {
            if (!IsBatching) {
                throw new System.InvalidOperationException("Begin() must be called before any Draw() operation.");
            }
        }

        #endregion Private Methods

        #region Internal Methods

#if DEBUG
        internal static void ResetMetrics() {
            TotalDrawCalls = SpriteCount = 0;
        }

#endif

        #endregion Internal Methods
    }
}
