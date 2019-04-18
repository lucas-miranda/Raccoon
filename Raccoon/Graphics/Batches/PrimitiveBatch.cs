using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    public class PrimitiveBatch {
        #region Private Members

        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private List<PrimitiveBatchItem> _batchFilledItems = new List<PrimitiveBatchItem>(),
                                         _batchHollowItems = new List<PrimitiveBatchItem>();

        private int _filledVerticesCount, _filledIndicesCount, _hollowVerticesCount, _hollowIndicesCount;

        private Matrix _world, _view, _projection;

        #endregion Private Members

        #region Constructors

        public PrimitiveBatch() {
            Shader = Game.Instance.BasicShader;
        }

        #endregion Constructors

        #region Public Properties

        public Shader Shader { get; set; }
        public bool IsBatching { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Begin(Matrix world, Matrix view, Matrix projection) {
            IsBatching = true;
            _world = world;
            _view = view;
            _projection = projection;
            _batchFilledItems.Clear();
            _batchHollowItems.Clear();
            _filledVerticesCount = _filledIndicesCount = _hollowVerticesCount = _hollowIndicesCount = 0;
        }

        public void End() {
            GraphicsDevice graphicsDevice = Game.Instance.GraphicsDevice;
            RenderFilledItems(graphicsDevice, ref _world, ref _view, ref _projection);
            RenderHollowItems(graphicsDevice, ref _world, ref _view, ref _projection);
            IsBatching = false;
        }

        public void DrawLines(IList<Vector2> points, Color color, float rotation, Vector2 scale, Vector2 origin, bool cyclic = true) {
            VertexPositionColor[] vertexData = new VertexPositionColor[points.Count];

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
                    vertexData[i] = new VertexPositionColor(new Vector3(Math.Rotate((point - origin) * scale, rotation), 0f), color);

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
                    vertexData[i] = new VertexPositionColor(new Vector3((point - origin) * scale, 0f), color);

                    if (j < points.Count - 1 || cyclic) {
                        indexData[2 * i] = i;
                        indexData[2 * i + 1] = (i + 1) % vertexData.Length;
                    }

                    i++;
                }
            }

            PrimitiveBatchItem batchItem = new PrimitiveBatchItem(vertexData, indexData);
            _batchHollowItems.Add(batchItem);
            _hollowVerticesCount += vertexData.Length;
            _hollowIndicesCount += indexData.Length;
        }

        #region Rectangle

        public void DrawFilledRectangle(Vector2 position, Size size, Color color, float rotation, Vector2 scale, Vector2 origin) {
            Vector2 topLeft = position + Math.Rotate(-origin * scale, rotation),
                    topRight = position + Math.Rotate((-origin + new Vector2(size.Width, 0f)) * scale, rotation),
                    bottomRight = position + Math.Rotate((-origin + size) * scale, rotation),
                    bottomLeft = position + Math.Rotate((-origin + new Vector2(0f, size.Height)) * scale, rotation);

            VertexPositionColor[] vertexData = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(topLeft, 0f), color),
                new VertexPositionColor(new Vector3(topRight, 0f), color),
                new VertexPositionColor(new Vector3(bottomRight, 0f), color),
                new VertexPositionColor(new Vector3(bottomLeft, 0f), color)
            };

            int[] indexData = new int[] {
                3, 0, 2,
                2, 0, 1
            };

            PrimitiveBatchItem batchItem = new PrimitiveBatchItem(vertexData, indexData);
            _batchFilledItems.Add(batchItem);
            _filledVerticesCount += vertexData.Length;
            _filledIndicesCount += indexData.Length;
        }

        public void DrawFilledRectangle(Rectangle rectangle, Color color, float rotation, Vector2 scale, Vector2 origin) {
            DrawFilledRectangle(rectangle.Position, rectangle.Size, color, rotation, scale, origin);
        }

        public void DrawHollowRectangle(Vector2 position, Size size, Color color, float rotation, Vector2 scale, Vector2 origin) {
            Vector2 topLeft = position + Math.Rotate(-origin * scale, rotation),
                    topRight = position + Math.Rotate((-origin + new Vector2(size.Width, 0f)) * scale, rotation),
                    bottomRight = position + Math.Rotate((-origin + size) * scale, rotation),
                    bottomLeft = position + Math.Rotate((-origin + new Vector2(0f, size.Height)) * scale, rotation);

            VertexPositionColor[] vertexData = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(topLeft, 0f), color),
                new VertexPositionColor(new Vector3(topRight, 0f), color),
                new VertexPositionColor(new Vector3(bottomRight, 0f), color),
                new VertexPositionColor(new Vector3(bottomLeft, 0f), color)
            };

            int[] indexData = new int[] {
                0, 1,
                1, 2,
                2, 3,
                3, 0
            };

            PrimitiveBatchItem batchItem = new PrimitiveBatchItem(vertexData, indexData);
            _batchHollowItems.Add(batchItem);
            _hollowVerticesCount += vertexData.Length;
            _hollowIndicesCount += indexData.Length;
        }

        public void DrawHollowRectangle(Rectangle rectangle, Color color, float rotation, Vector2 scale, Vector2 origin) {
            DrawHollowRectangle(rectangle.Position, rectangle.Size, color, rotation, scale, origin);
        }

        #endregion Rectangle

        #region Circle

        public void DrawFilledCircle(Vector2 center, float radius, Color color, float scale, Vector2 origin, int segments = 0) {
            radius *= scale;
            center -= origin;

            if (segments <= 0) {
                segments = (int) (radius + radius);
            }

            VertexPositionColor[] vertexData = new VertexPositionColor[segments + 1];
            int[] indexData = new int[(segments + 1) * 3];

            // update vertices
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            float theta = (float) (2.0 * Math.PI / segments);
            float t, c = (float) System.Math.Cos(theta), s = (float) System.Math.Sin(theta); // precalculate the sine and cosine

            float x = radius * Math.Cos(0),
                  y = radius * Math.Sin(0);

            // center
            int centerIndex = vertexData.Length - 1;
            vertexData[centerIndex] = new VertexPositionColor(new Vector3(center.X, center.Y, 0f), color);

            int i;
            for (i = 0; i < vertexData.Length; i++) {
                vertexData[i] = new VertexPositionColor(new Vector3(center.X + x, center.Y + y, 0f), color);

                // apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;

                indexData[i * 3] = centerIndex; // circle center
                indexData[i * 3 + 1] = i; // current vertex
                indexData[i * 3 + 2] = (i + 1) % vertexData.Length; // next vertex (cyclic)
            }

            PrimitiveBatchItem batchItem = new PrimitiveBatchItem(vertexData, indexData);
            _batchFilledItems.Add(batchItem);
            _filledVerticesCount += vertexData.Length;
            _filledIndicesCount += indexData.Length;
        }

        public void DrawFilledCircle(Circle circle, Color color, float scale, Vector2 origin, int segments = 0) {
            DrawFilledCircle(circle.Center, circle.Radius, color, scale, origin, segments);
        }

        public void DrawHollowCircle(Vector2 center, float radius, Color color, float scale, Vector2 origin, int segments = 0) {
            radius *= scale;
            center -= origin;

            if (segments <= 0) {
                segments = (int) (radius + radius);
            }

            VertexPositionColor[] vertexData = new VertexPositionColor[segments];
            int[] indexData = new int[segments * 2];

            // update vertices
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            float theta = (float) (2.0 * Math.PI / segments);
            float t, c = (float) System.Math.Cos(theta), s = (float) System.Math.Sin(theta); // precalculate the sine and cosine

            float x = radius * Math.Cos(0),
                  y = radius * Math.Sin(0);

            for (int i = 0; i < vertexData.Length; i++) {
                vertexData[i] = new VertexPositionColor(new Vector3(center.X + x, center.Y + y, 0f), color);

                // apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;

                indexData[2 * i] = i; // current vertex
                indexData[2 * i + 1] = (i + 1) % vertexData.Length; 
            }

            PrimitiveBatchItem batchItem = new PrimitiveBatchItem(vertexData, indexData);
            _batchHollowItems.Add(batchItem);
            _hollowVerticesCount += vertexData.Length;
            _hollowIndicesCount += indexData.Length;
        }

        public void DrawHollowCircle(Circle circle, Color color, float scale, Vector2 origin, int segments = 0) {
            DrawHollowCircle(circle.Center, circle.Radius, color, scale, origin, segments);
        }

        #endregion Circle

        #region Polygon

        public void DrawFilledPolygon(Polygon polygon, Color color, float rotation, Vector2 scale, Vector2 origin) {
            VertexPositionColor[] vertexData = new VertexPositionColor[polygon.VertexCount + 1];
            int centerVertexId = vertexData.Length - 1;

            int[] indexData = new int[polygon.VertexCount * 3];

            int i = 0;
            if (rotation % 360 != 0) {
                vertexData[centerVertexId] = new VertexPositionColor(new Vector3(Math.Rotate((polygon.Center - origin) * scale, rotation), 0f), color);

                foreach (Vector2 point in polygon) {
                    vertexData[i] = new VertexPositionColor(new Vector3(Math.Rotate((point - origin) * scale, rotation), 0f), color);

                    indexData[3 * i] = i;
                    indexData[3 * i + 1] = (i + 1) % polygon.VertexCount;
                    indexData[3 * i + 2] = centerVertexId;
                    i++;
                }
            } else {
                vertexData[centerVertexId] = new VertexPositionColor(new Vector3((polygon.Center - origin) * scale, 0f), color);

                foreach (Vector2 point in polygon) {
                    vertexData[i] = new VertexPositionColor(new Vector3((point - origin) * scale, 0f), color);

                    indexData[3 * i] = i;
                    indexData[3 * i + 1] = (i + 1) % polygon.VertexCount;
                    indexData[3 * i + 2] = centerVertexId;
                    i++;
                }
            }

            PrimitiveBatchItem batchItem = new PrimitiveBatchItem(vertexData, indexData);
            _batchFilledItems.Add(batchItem);
            _filledVerticesCount += vertexData.Length;
            _filledIndicesCount += indexData.Length;
        }

        public void DrawHollowPolygon(IList<Vector2> points, Color color, float rotation, Vector2 scale, Vector2 origin) {
            DrawLines(points, color, rotation, scale, origin, cyclic: true);
        }

        public void DrawHollowPolygon(Polygon polygon, Color color, float rotation, Vector2 scale, Vector2 origin) {
            DrawLines(polygon.Vertices, color, rotation, scale, origin, cyclic: true);
        }

        #endregion Polygon

        #endregion Public Methods

        #region Private Methods

        private void RenderFilledItems(GraphicsDevice graphicsDevice, ref Matrix world, ref Matrix view, ref Matrix projection) {
            if (_batchFilledItems.Count == 0) {
                return;
            }

            if (_vertexBuffer == null || _vertexBuffer.VertexCount < _filledVerticesCount) {
                _vertexBuffer = new DynamicVertexBuffer(graphicsDevice, VertexPositionColor.VertexDeclaration, _filledVerticesCount, BufferUsage.WriteOnly);
            }

            if (_indexBuffer == null || _indexBuffer.IndexCount < _filledIndicesCount) {
                _indexBuffer = new DynamicIndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, _filledIndicesCount, BufferUsage.WriteOnly);
            }

            int vertexId = 0,
                indexId = 0;

            int[] indices = new int[_filledIndicesCount];
            VertexPositionColor[] vertices = new VertexPositionColor[_filledVerticesCount];

            foreach (PrimitiveBatchItem batchItem in _batchFilledItems) {
                System.Array.Copy(batchItem.VertexData, 0, vertices, vertexId, batchItem.VertexData.Length);

                for (int i = 0; i < batchItem.IndexData.Length; i++) {
                    indices[indexId + i] = vertexId + batchItem.IndexData[i];
                }

                vertexId += batchItem.VertexData.Length;
                indexId += batchItem.IndexData.Length;
            }

            _vertexBuffer.SetData(
                vertices,
                0,
                vertices.Length,
                SetDataOptions.Discard
            );

            _indexBuffer.SetData(
                indices,
                0,
                indices.Length,
                SetDataOptions.Discard
            );

            BasicShader shader = Shader as BasicShader;

            // transformations
            shader.World = Game.Instance.MainRenderer.World;
            shader.View = Game.Instance.MainRenderer.View;
            shader.Projection = Game.Instance.MainRenderer.Projection;

            shader.DiffuseColor = Color.White;
            shader.Alpha = 1f;

            shader.TextureEnabled = false;

            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            
            foreach (object pass in shader) {
                graphicsDevice.Indices = _indexBuffer;
                graphicsDevice.SetVertexBuffer(_vertexBuffer);
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount, 0, indexId / 3);
            }

            shader.ResetParameters();
        }

        private void RenderHollowItems(GraphicsDevice graphicsDevice, ref Matrix world, ref Matrix view, ref Matrix projection) {
            if (_batchHollowItems.Count == 0) {
                return;
            }

            if (_vertexBuffer == null || _vertexBuffer.VertexCount < _hollowVerticesCount) {
                _vertexBuffer = new DynamicVertexBuffer(graphicsDevice, VertexPositionColor.VertexDeclaration, _hollowVerticesCount, BufferUsage.WriteOnly);
            }

            if (_indexBuffer == null || _indexBuffer.IndexCount < _hollowIndicesCount) {
                _indexBuffer = new DynamicIndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, _hollowIndicesCount, BufferUsage.WriteOnly);
            }

            int vertexId = 0,
                indexId = 0;

            int[] indices = new int[_hollowIndicesCount];
            VertexPositionColor[] vertices = new VertexPositionColor[_hollowVerticesCount];

            foreach (PrimitiveBatchItem batchItem in _batchHollowItems) {
                System.Array.Copy(batchItem.VertexData, 0, vertices, vertexId, batchItem.VertexData.Length);

                for (int i = 0; i < batchItem.IndexData.Length; i++) {
                    indices[indexId + i] = vertexId + batchItem.IndexData[i];
                }

                vertexId += batchItem.VertexData.Length;
                indexId += batchItem.IndexData.Length;
            }

            _vertexBuffer.SetData(
                0,
                vertices,
                0,
                vertices.Length,
                _vertexBuffer.VertexDeclaration.VertexStride,
                SetDataOptions.Discard
            );

            _indexBuffer.SetData(
                0,
                indices,
                0,
                indices.Length,
                SetDataOptions.Discard
            );


            BasicShader shader = Shader as BasicShader;
            shader.ResetParameters();

            // transformations
            shader.World = world;
            shader.View = view;
            shader.Projection = projection;

            shader.DiffuseColor = Color.White;
            shader.Alpha = 1f;

            shader.TextureEnabled = false;

            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            
            foreach (object pass in shader) {
                graphicsDevice.Indices = _indexBuffer;
                graphicsDevice.SetVertexBuffer(_vertexBuffer);
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, _vertexBuffer.VertexCount, 0, _hollowIndicesCount / 2);
            }

            shader.ResetParameters();
        }

        #endregion Private Methods
    }
}
