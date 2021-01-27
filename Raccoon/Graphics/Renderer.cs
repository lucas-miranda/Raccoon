using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    /// <summary>
    /// An all-in-one provider, containing means to draw anything that Raccoon.Graphics can offer.
    /// Also aims to centralize the rendering setups to make everything looks consistently.
    /// </summary>
    public class Renderer : System.IDisposable {
        #region Public Members

        public static readonly System.Func<Size> DefaultRecalculateProjectionSize = () => Game.Instance.WindowSize;

        public System.Func<Size> RecalculateProjectionSize = DefaultRecalculateProjectionSize;

        public System.Action OnBeforeRender = delegate { },
                             OnAfterRender = delegate { };

        #endregion Public Members

        #region Private Members

        private Size _previousProjectionSize;

        private Matrix _projection = Matrix.Identity;

        #endregion Private Members

        #region Constructors

        public Renderer(bool autoHandleAlphaBlendedSprites = false) {
            if (Game.Instance.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Renderer needs a valid graphics device. Maybe are you using before first Scene.Start() is called?");
            }

            Batch = new MixedBatch(Game.Instance.GraphicsDevice, autoHandleAlphaBlendedSprites);
            RecalculateProjection();
        }

        #endregion Constructors

        #region Public Properties

        public bool IsBatching { get { return Batch.IsBatching; } }
        public MixedBatch Batch { get; private set; }
        public BatchMode SpriteBatchMode { get; set; }
        public BlendState BlendState { get; set; }
        public SamplerState SamplerState { get; set; }
        public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.None;
        public RasterizerState RasterizerState { get; set; }
        public Shader Shader { get; set; }
        public Matrix World { get; set; } = Matrix.Identity;
        public Matrix View { get; set; } = Matrix.Identity;
        public bool IsDisposed { get; private set; }

        public Matrix Projection {
            get {
                return _projection;
            }

            set {
                _projection = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Begin(BatchMode? batchMode = null, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Matrix? transform = null) {
            Batch.Begin(
                batchMode ?? SpriteBatchMode,
                blendState ?? BlendState,
                samplerState ?? SamplerState,
                depthStencilState ?? DepthStencilState,
                rasterizerState ?? RasterizerState,
                transform
            );
        }

        public void End() {
            InternalFlush();
        }

        public Vector2 ConvertScreenToWorld(Vector2 screenPosition) {
			Matrix inverseWVPMatrix = Matrix.Invert(
                Matrix.Multiply(
                    Matrix.Multiply(World, View),
                    Projection
                )
            );

			screenPosition.X = (2f * (screenPosition.X /  _previousProjectionSize.Width)) - 1f;
			screenPosition.Y = 1f - (2f * (screenPosition.Y / _previousProjectionSize.Height));

			float x = (screenPosition.X * inverseWVPMatrix.M11)
                    + (screenPosition.Y * inverseWVPMatrix.M21)
                    + inverseWVPMatrix.M41;

			float y = (screenPosition.X * inverseWVPMatrix.M12)
                    + (screenPosition.Y * inverseWVPMatrix.M22)
                    + inverseWVPMatrix.M42;

			float a = (screenPosition.X * inverseWVPMatrix.M14) 
			        + (screenPosition.Y * inverseWVPMatrix.M24) 
			        + inverseWVPMatrix.M44;

			if (!Math.EqualsEstimate(a, 1.0f)) {
                x /= a;
				y /= a;
			}

            return new Vector2(x, y);
        }

        public Vector2 ConvertWorldToScreen(Vector2 worldPosition) {
			Matrix wvpMatrix = Matrix.Multiply(
				Matrix.Multiply(World, View),
				Projection
			);

			float x = (worldPosition.X * wvpMatrix.M11)
                    + (worldPosition.Y * wvpMatrix.M21)
                    + wvpMatrix.M41;

			float y = (worldPosition.X * wvpMatrix.M12)
                    + (worldPosition.Y * wvpMatrix.M22)
                    + wvpMatrix.M42;

			float a = (worldPosition.X * wvpMatrix.M14) 
			        + (worldPosition.Y * wvpMatrix.M24) 
			        + wvpMatrix.M44;

			if (!Math.EqualsEstimate(a, 1.0f)) {
                x /= a;
				y /= a;
			}

            return new Vector2(
                (x + 1f) * .5f * _previousProjectionSize.Width,
                (1f - y) * .5f * _previousProjectionSize.Height
            );
        }

        public ref readonly Matrix RecalculateProjection() {
            Size size = RecalculateProjectionSize();

            if (size == _previousProjectionSize) {
                return ref _projection;
            }

            Matrix.CreateOrthographicOffCenter(0f, size.Width, size.Height, 0f, 0f, 1f, out _projection);

            _previousProjectionSize = size;

            return ref _projection;
        }

        /// <summary>
        /// Forces every batcher to render stored batches.
        /// </summary>
        /// <param name="reinitializeBatches">True, if reinitilizing batchers after flushing is intended, False otherwise.</param>
        public void Flush(bool reinitializeBatches = true) {
            if (!Batch.IsBatching) {
                return;
            }

            InternalFlush();

            if (reinitializeBatches) {
                Begin(
                    Batch.BatchMode, 
                    Batch.BlendState, 
                    Batch.SamplerState, 
                    Batch.DepthStencilState, 
                    Batch.RasterizerState, 
                    Batch.Transform
                );
            }
        }

        #region Draw Texture With Explicit Vertices

        public void Draw(Texture texture, Vector2[] vertices, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, vertices, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, vertices, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, VertexPositionColorTexture[] vertexData, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, vertexData, position, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, vertexData, position, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        #endregion Draw Texture With Explicit Vertices

        #region Draw Texture on Destination Rectangle

        public void Draw(Canvas canvas, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(canvas.Texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(canvas.Texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, destinationRectangle, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, destinationRectangle, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, destinationRectangle, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, destinationRectangle, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            Draw(texture, destinationRectangle, color, Vector2.One, shader, shaderParameters, layerDepth);
        }

        #endregion Draw Texture on Destination Rectangle

        #region Draw Texture with Position

        public void Draw(Canvas canvas, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(canvas.Texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(canvas.Texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, position, sourceRectangle, rotation, new Vector2(scale), flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, position, sourceRectangle, rotation, new Vector2(scale), flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, position, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, position, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Color color, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.Draw(texture, position, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.Draw(texture, position, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Color color, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            Draw(texture, position, color, Vector2.One, shader, shaderParameters, layerDepth);
        }

        #endregion Draw Texture with Position

        #region Draw Text from String

        public void DrawString(Font font, Text.RenderData glyphs, int glyphStartIndex, int glyphCount, Rectangle destinationRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawString(font, glyphs, glyphStartIndex, glyphCount, destinationRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawString(font, glyphs, glyphStartIndex, glyphCount, destinationRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawString(Font font, Text.RenderData glyphs, Rectangle destinationRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawString(font, glyphs, destinationRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawString(font, glyphs, destinationRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawString(font, text, position, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawString(font, text, position, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawString(font, text, position, rotation, new Vector2(scale), flip, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawString(font, text, position, rotation, new Vector2(scale), flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scroll, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Shader shader = null, IShaderParameters shaderParameters = null, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, Vector2.One, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, Vector2.One, shader, shaderParameters, layerDepth);
        }

        #endregion Draw Text from String

        #region Line

        public void DrawLines(IList<Vector2> points, Vector2 position, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, bool cyclic = true, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawLines(points, position, color, rotation, scale, origin, scroll, shader, shaderParameters, cyclic, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawLines(points, position, color, rotation, scale, origin, scroll, shader, shaderParameters, cyclic, layerDepth);
        }

        public void DrawLineStroke(Vector2 startPoint, Vector2 endPoint, float thickness, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (thickness <= 0f) {
                throw new System.ArgumentException("Thickness must be greater than zero.");
            }

            Vector2 direction = (endPoint - startPoint).Normalized();

            Vector2 normal = direction.PerpendicularCCW(),
                    antiNormal = direction.PerpendicularCW();

            float halfThickness = thickness / 2f;

            List<Vector2> vertices = new List<Vector2>(4) {
                startPoint  + normal * halfThickness,
                endPoint    + normal * halfThickness,
                endPoint    + antiNormal * halfThickness,
                startPoint  + antiNormal * halfThickness
            };

            int[] indices = new int[] {
                3, 0, 2,
                2, 0, 1
            };

            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawVertices(
                    vertices, 
                    minVertexIndex: 0,
                    verticesLength: vertices.Count,
                    indices,
                    minIndex: 0,
                    primitivesCount: 2,
                    isHollow: false,
                    position: Vector2.Zero, 
                    rotation, 
                    scale, 
                    color, 
                    origin, 
                    scroll, 
                    shader, 
                    shaderParameters, 
                    layerDepth
                );
                AfterRender();
                return;
            }

            Batch.DrawVertices(
                vertices, 
                minVertexIndex: 0,
                verticesLength: vertices.Count,
                indices,
                minIndex: 0,
                primitivesCount: 2,
                isHollow: false,
                position: Vector2.Zero, 
                rotation, 
                scale, 
                color, 
                origin, 
                scroll, 
                shader, 
                shaderParameters, 
                layerDepth
            );
        }

        #endregion Line

        #region Rectangle

        public void DrawFilledRectangle(Vector2 position, Size size, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawFilledRectangle(position, size, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawFilledRectangle(position, size, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawFilledRectangle(Rectangle rectangle, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            DrawFilledRectangle(rectangle.Position, rectangle.Size, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawHollowRectangle(Vector2 position, Size size, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawHollowRectangle(position, size, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawHollowRectangle(position, size, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawHollowRectangle(Rectangle rectangle, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            DrawHollowRectangle(rectangle.Position, rectangle.Size, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
        }

        #endregion Rectangle

        #region Circle

        public void DrawFilledCircle(Vector2 center, float radius, Color color, float scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, int segments = 0, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawFilledCircle(center, radius, color, scale, origin, scroll, shader, shaderParameters, segments, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawFilledCircle(center, radius, color, scale, origin, scroll, shader, shaderParameters, segments, layerDepth);
        }

        public void DrawFilledCircle(Circle circle, Color color, float scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, int segments = 0, float layerDepth = 1f) {
            DrawFilledCircle(circle.Center, circle.Radius, color, scale, origin, scroll, shader, shaderParameters, segments, layerDepth);
        }

        public void DrawHollowCircle(Vector2 center, float radius, Color color, float scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, int segments = 0, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawHollowCircle(center, radius, color, scale, origin, scroll, shader, shaderParameters, segments, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawHollowCircle(center, radius, color, scale, origin, scroll, shader, shaderParameters, segments, layerDepth);
        }

        public void DrawHollowCircle(Circle circle, Color color, float scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, int segments = 0, float layerDepth = 1f) {
            DrawHollowCircle(circle.Center, circle.Radius, color, scale, origin, scroll, shader, shaderParameters, segments, layerDepth);
        }

        #endregion Circle

        #region Polygon

        public void DrawFilledPolygon(Polygon polygon, Vector2 position, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawFilledPolygon(polygon, position, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawFilledPolygon(polygon, position, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawHollowPolygon(IList<Vector2> points, Vector2 position, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawHollowPolygon(points, position, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawHollowPolygon(points, position, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawHollowPolygon(Polygon polygon, Vector2 position, Color color, float rotation, Vector2 scale, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawHollowPolygon(polygon, position, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawHollowPolygon(polygon, position, color, rotation, scale, origin, scroll, shader, shaderParameters, layerDepth);
        }

        #endregion Polygon

        #region Others

        public void DrawVertices(IList<Vector2> vertices, int minVertexIndex, int verticesLength, int[] indices, int minIndex, int primitivesCount, bool isHollow, Vector2 position, float rotation, Vector2 scale, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawVertices(vertices, minVertexIndex, verticesLength, indices, minIndex, primitivesCount, isHollow, position, rotation, scale, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawVertices(vertices, minVertexIndex, verticesLength, indices, minIndex, primitivesCount, isHollow, position, rotation, scale, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawVertices(Texture texture, VertexPositionColorTexture[] vertexData, int minVertexIndex, int verticesLength, int[] indices, int minIndex, int primitivesCount, bool isHollow, Vector2 position, float rotation, Vector2 scale, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawVertices(texture, vertexData, minVertexIndex, verticesLength, indices, minIndex, primitivesCount, isHollow, position, rotation, scale, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawVertices(texture, vertexData, minVertexIndex, verticesLength, indices, minIndex, primitivesCount, isHollow, position, rotation, scale, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void DrawVertices(VertexPositionColor[] vertexData, int minVertexIndex, int verticesLength, int[] indices, int minIndex, int primitivesCount, bool isHollow, Vector2 position, float rotation, Vector2 scale, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (Batch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                Batch.DrawVertices(vertexData, minVertexIndex, verticesLength, indices, minIndex, primitivesCount, isHollow, position, rotation, scale, color, origin, scroll, shader, shaderParameters, layerDepth);
                AfterRender();
                return;
            }

            Batch.DrawVertices(vertexData, minVertexIndex, verticesLength, indices, minIndex, primitivesCount, isHollow, position, rotation, scale, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        #endregion Others

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            IsDisposed = true;

            RecalculateProjectionSize = null;
            OnBeforeRender = null;
            OnAfterRender = null;
            Shader = null;
            Batch = null;
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void BeforeRender() {
            OnBeforeRender();
        }

        protected virtual void AfterRender() {
            OnAfterRender();
        }

        #endregion Protected Methods

        #region Private Methods

        public void InternalFlush() {
            if (!Batch.IsBatching) {
                return;
            }

            PrepareBeforeRender();
            Batch.End();
            AfterRender();
        }

        #endregion Private Methods

        #region Private Methods

        private void PrepareBeforeRender() {
            if (!IsBatching) {
                throw new System.InvalidOperationException("SpriteBatch must be initialized and Begin() called previously.");
            }

            BeforeRender();

            if (Shader != null && Batch.Shader != Shader) {
                Batch.Shader = Shader;
            }

            // pass along transfom matrices
            if (Batch.Shader is IShaderTransform spriteBatch_Shader_Transform) {
                spriteBatch_Shader_Transform.World = World;
                spriteBatch_Shader_Transform.View = View;
                spriteBatch_Shader_Transform.Projection = Projection;
            }
        }

        #endregion Private Methods
    }
}
