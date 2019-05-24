using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Strip : PrimitiveGraphic {
        #region Private Members

        private DynamicVertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

#if DEBUG
        private IndexBuffer _debug_indexBuffer;
#endif

        #endregion Private Members

        #region Consctructors

        public Strip(int sections, Texture texture) {
            Texture = texture;
            SetupSections(sections);
        }

        public Strip(int sections, string filename) : this(sections, new Texture(filename)) {
        }

        #endregion Consctructors

        #region Public Properties

        public Vector2 Alignment { get; set; } = new Vector2(0f, .5f);
        public Texture Texture { get; set; }

        #endregion Public Properties

        #region Protected Properties

        protected Vector2[] SectionsPoints { get; private set; } = new Vector2[0];
        protected int Sections { get { return System.Math.Max(0, SectionsPoints.Length - 1); } }

        #endregion Protected Properties

        #region Public Methods

        public override void DebugRender(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll) {
#if DEBUG
            if (Sections == 0) {
                return;
            }

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X, Position.Y + position.Y, 0f)
                * Renderer.World;

            //bs.View = Microsoft.Xna.Framework.Matrix.Invert(scrollMatrix) * Renderer.View * scrollMatrix;
            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.SetMaterial(color, 1f);

            GraphicsDevice device = Game.Instance.GraphicsDevice;
            foreach (object pass in bs) {
                device.Indices = _debug_indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.LineStrip, 0, 0, _vertexBuffer.VertexCount, 0, Sections * 6 - 1);
            }

            bs.ResetParameters();
#endif
        }

        public void SetSections(IList<Vector2> sectionsPoints, float width = 1f) {
            if (sectionsPoints.Count != SectionsPoints.Length) {
                throw new System.ArgumentException($"Expected {SectionsPoints.Length} points, got {sectionsPoints.Count}", "sectionsPoints");
            }

            sectionsPoints.CopyTo(SectionsPoints, 0);

            // update vertices
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[(sectionsPoints.Count - 1) * 4];
            float upWidth = width * Alignment.Y,
                  downWidth = width - upWidth,
                  leftWidth = upWidth * Alignment.X,
                  rightWidth = width - leftWidth;

            for (int s = 0; s < Sections; s++) {
                Vector2 startPoint = SectionsPoints[s],
                        endPoint = SectionsPoints[s + 1];

                Vector2 direction = Util.Math.PolarToCartesian(1f, Util.Math.Angle(startPoint, endPoint));
                Vector2 up = direction.PerpendicularCCW() * upWidth,
                        down = direction.PerpendicularCW() * downWidth;

                //
                // Vertices layout:
                //
                //  1--3 5--7
                //  |\ | |\ |
                //  | \| | \|
                //  0--2 4--6
                //

                int vertexStart = s * 4;

                Microsoft.Xna.Framework.Vector3 topStart = Microsoft.Xna.Framework.Vector3.Zero,
                                                bottomStart = Microsoft.Xna.Framework.Vector3.Zero;

                if (s == 0) {
                    Vector2 correction = -direction * upWidth; // subtle start points correction (intention is to align correctly when strip is cyclic)

                    bottomStart = new Microsoft.Xna.Framework.Vector3(startPoint.X + down.X + correction.X, startPoint.Y + down.Y + correction.Y, 0f);
                    topStart = new Microsoft.Xna.Framework.Vector3(startPoint.X + up.X + correction.X, startPoint.Y + up.Y + correction.Y, 0f);
                } else {
                    // test if previous point, current point and next point constitute a closed angle
                    // and apply a correction, if needed
                    // the goal is to smooth hard turns

                    Vector2 previousPoint = SectionsPoints[s - 1];
                    Microsoft.Xna.Framework.Vector3 previousBottomPoint = vertices[vertexStart - 2].Position,
                                                    previousTopPoint = vertices[vertexStart - 1].Position;

                    float angle = Util.Math.Angle(previousPoint, startPoint, endPoint);
                    bool isClosedAngle = !Util.Helper.InRange(angle, 180 - 45, 180 + 45);

                    if (isClosedAngle) {
                        Vector2 pivot, rotatedPoint;

                        if (Util.Math.IsRight(previousPoint, endPoint, startPoint)) { // NOTE: ideally should be "IsLeft"
                            // right turn
                            pivot = new Vector2(previousTopPoint.X, previousTopPoint.Y);
                            rotatedPoint = Util.Math.RotateAround(new Vector2(previousBottomPoint.X, previousBottomPoint.Y), pivot, angle);
                            rotatedPoint += up; // align alignment correction
                            bottomStart = new Microsoft.Xna.Framework.Vector3(rotatedPoint.X, rotatedPoint.Y, 0f);

                            // apply alignment correction
                            pivot += up;

                            topStart = new Microsoft.Xna.Framework.Vector3(pivot.X, pivot.Y, 0f);

                            // apply correction on previous pair of points too (for a nice alignment)
                            Microsoft.Xna.Framework.Vector3 upCorrection = new Microsoft.Xna.Framework.Vector3(up.X, up.Y, 0f);
                            vertices[vertexStart - 2].Position = previousBottomPoint + upCorrection;
                            vertices[vertexStart - 1].Position = previousTopPoint + upCorrection;
                        } else {
                            // left turn
                            pivot = new Vector2(previousBottomPoint.X, previousBottomPoint.Y);
                            rotatedPoint = Util.Math.RotateAround(new Vector2(previousTopPoint.X, previousTopPoint.Y), pivot, angle);
                            rotatedPoint += down; // alignment correction
                            topStart = new Microsoft.Xna.Framework.Vector3(rotatedPoint.X, rotatedPoint.Y, 0f);

                            // apply alignment correction
                            pivot += down;

                            bottomStart = new Microsoft.Xna.Framework.Vector3(pivot.X, pivot.Y, 0f);

                            // apply correction on previous pair of points too (for a nice alignment)
                            Microsoft.Xna.Framework.Vector3 downCorrection = new Microsoft.Xna.Framework.Vector3(down.X, down.Y, 0f);
                            vertices[vertexStart - 2].Position = previousBottomPoint + downCorrection;
                            vertices[vertexStart - 1].Position = previousTopPoint + downCorrection;
                        }
                    } else {
                        bottomStart = previousBottomPoint;
                        topStart = previousTopPoint;
                    }
                }

                // start-bottom
                vertices[vertexStart] = new VertexPositionColorTexture(
                    bottomStart,
                    Microsoft.Xna.Framework.Color.White,
                    new Microsoft.Xna.Framework.Vector2(0f, 1f)
                );

                // start-top
                vertices[vertexStart + 1] = new VertexPositionColorTexture(
                    topStart,
                    Microsoft.Xna.Framework.Color.White,
                    new Microsoft.Xna.Framework.Vector2(0f, 0f)
                );

                // end-bottom
                vertices[vertexStart + 2] = new VertexPositionColorTexture(
                    new Microsoft.Xna.Framework.Vector3(endPoint.X + down.X, endPoint.Y + down.Y, 0f),
                    Microsoft.Xna.Framework.Color.White,
                    new Microsoft.Xna.Framework.Vector2(1f, 1f)
                );

                // end-top
                vertices[vertexStart + 3] = new VertexPositionColorTexture(
                    new Microsoft.Xna.Framework.Vector3(endPoint.X + up.X, endPoint.Y + up.Y, 0f),
                    Microsoft.Xna.Framework.Color.White,
                    new Microsoft.Xna.Framework.Vector2(1f, 0f)
                );
            }

            _vertexBuffer.SetData(vertices, 0, vertices.Length, SetDataOptions.Discard);
        }

        public override void Dispose() {
            if (Texture == null) {
                return;
            }

            Texture.Dispose();
        }

        public override string ToString() {
            return $"[Strip | Position: {Position}, Texture: {Texture}]";
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (Sections == 0) {
                return;
            }

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X, Position.Y + position.Y, 0f) * Renderer.World;
            //bs.View = Microsoft.Xna.Framework.Matrix.Invert(scrollMatrix) * Renderer.View * scrollMatrix;
            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.SetMaterial(color * Color, Opacity);

            // texture
            bs.TextureEnabled = true;
            bs.Texture = Texture;

            GraphicsDevice device = Game.Instance.GraphicsDevice;
            foreach (object pass in bs) {
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount, 0, Sections * 2);
            }

            bs.ResetParameters();
        }

        #endregion Protected Methods

        #region Private Methods

        private void SetupSections(int sections) {
            SectionsPoints = new Vector2[sections + 1];

            int[] indices = new int[sections * 2 * 3];
#if DEBUG
            int[] debug_indices = new int[indices.Length];
#endif

            for (int s = 0, i = 0; s < sections; s++, i += 4) {
                //
                // Vertices layout:
                //
                //  1--3/5--7
                //  |\  | \ |
                //  | \ |  \|
                //  0--2/4--6
                //

                int indexStart = s * 6;
                indices[indexStart] = i;
                indices[indexStart + 1] = i + 1;
                indices[indexStart + 2] = i + 2;
                indices[indexStart + 3] = i + 2;
                indices[indexStart + 4] = i + 1;
                indices[indexStart + 5] = i + 3;

#if DEBUG
                int debugIndexStart = s * 6;
                debug_indices[debugIndexStart] = i + 1;
                debug_indices[debugIndexStart + 1] = i;
                debug_indices[debugIndexStart + 2] = i + 2;
                debug_indices[debugIndexStart + 3] = i + 1;
                debug_indices[debugIndexStart + 4] = i + 3;
                debug_indices[debugIndexStart + 5] = i + 2;
#endif
            }

            if (_vertexBuffer == null || sections * 4 > _vertexBuffer.VertexCount) {
                _vertexBuffer = new DynamicVertexBuffer(Game.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, sections * 4, BufferUsage.WriteOnly);
            }

            if (_indexBuffer == null || indices.Length > _indexBuffer.IndexCount) {
                _indexBuffer = new IndexBuffer(Game.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            }

            _indexBuffer.SetData(indices);

#if DEBUG
            if (_debug_indexBuffer == null || debug_indices.Length > _debug_indexBuffer.IndexCount) {
                _debug_indexBuffer = new IndexBuffer(Game.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, debug_indices.Length, BufferUsage.WriteOnly);
            }

            _debug_indexBuffer.SetData(debug_indices);
#endif
        }

        #endregion Private Methods
    }
}
