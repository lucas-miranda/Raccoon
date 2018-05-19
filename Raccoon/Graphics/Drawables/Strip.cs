﻿using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Strip : Image {
        #region Private Members

        private DynamicVertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

#if DEBUG
        private IndexBuffer _debug_indexBuffer;
#endif

        #endregion Private Members

        #region Consctructors

        public Strip(int sections, Texture texture) : base(texture) {
            SetupSections(sections);
        }

        public Strip(int sections, string filename) : base(filename) {
            SetupSections(sections);
        }

        public Strip(int sections, AtlasSubTexture subTexture) : base(subTexture) {
            SetupSections(sections);
        }

        public Strip(int sections, Image image) :  base(image) {
            SetupSections(sections);
        }

        #endregion Consctructors

        #region Protected Properties

        protected Vector2[] SectionsPoints { get; private set; } = new Vector2[0];
        protected int Sections { get { return System.Math.Max(0, SectionsPoints.Length - 1); } }

        #endregion Protected Properties

        #region Public Methods

        public override void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null) {
            if (Sections == 0) {
                return;
            }

            scroll += Scroll;
            scroll = scroll.LengthSquared() == 0f ? new Vector2(Util.Math.Epsilon) : scroll;
            Microsoft.Xna.Framework.Matrix scrollMatrix = Microsoft.Xna.Framework.Matrix.CreateScale(scroll.X, scroll.Y, 1f);

            BasicEffect effect = Game.Instance.Core.BasicEffect;
            effect.TextureEnabled = true;
            effect.Texture = Texture.XNATexture;
            float[] colorNormalized = (color * Color).Normalized;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(colorNormalized[0], colorNormalized[1], colorNormalized[2]);
            effect.Alpha = Opacity;
            effect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X, Position.Y + position.Y, 0f) * Surface.World;
            effect.View = Microsoft.Xna.Framework.Matrix.Invert(scrollMatrix) * Surface.View * scrollMatrix;
            effect.Projection = Surface.Projection;

            GraphicsDevice device = Game.Instance.Core.GraphicsDevice;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Sections * 2);
            }

            effect.Alpha = 1f;
            effect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
            effect.Texture = null;
            effect.TextureEnabled = false;
        }

        public override void DebugRender(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll) {
#if DEBUG
            if (Sections == 0) {
                return;
            }

            scroll = scroll.LengthSquared() == 0f ? new Vector2(Util.Math.Epsilon) : scroll;
            Microsoft.Xna.Framework.Matrix scrollMatrix = Microsoft.Xna.Framework.Matrix.CreateScale(scroll.X, scroll.Y, 1f);

            BasicEffect effect = Game.Instance.Core.BasicEffect;
            effect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X, Position.Y + position.Y, 0f) * Surface.World;
            effect.View = Microsoft.Xna.Framework.Matrix.Invert(scrollMatrix) * Surface.View * scrollMatrix;
            effect.Projection = Surface.Projection;

            GraphicsDevice device = Game.Instance.Core.GraphicsDevice;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.Indices = _debug_indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.LineStrip, 0, 0, Sections * 6 - 1);
            }
#endif
        }

        public void SetSections(Vector2[] sectionsPoints, float width = 1f) {
            if (sectionsPoints.Length != SectionsPoints.Length) {
                throw new System.ArgumentException($"Expected {SectionsPoints.Length} points, got {sectionsPoints.Length}", "sectionsPoints");
            }

            sectionsPoints.CopyTo(SectionsPoints, 0);

            // update vertices
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[(sectionsPoints.Length - 1) * 4];
            float halfWidth = width / 2f;

            for (int s = 0; s < Sections; s++) {
                Vector2 startPoint = SectionsPoints[s],
                        endPoint = SectionsPoints[s + 1];

                Vector2 direction = Util.Math.PolarToCartesian(1f, Util.Math.Angle(startPoint, endPoint));
                Vector2 up = direction.PerpendicularCCW() * halfWidth,
                        down = direction.PerpendicularCW() * halfWidth;

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
                    bottomStart = new Microsoft.Xna.Framework.Vector3(startPoint.X + down.X, startPoint.Y + down.Y, 0f);
                    topStart = new Microsoft.Xna.Framework.Vector3(startPoint.X + up.X, startPoint.Y + up.Y, 0f);
                } else {
                    bottomStart = vertices[vertexStart - 2].Position;
                    topStart = vertices[vertexStart - 1].Position;
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

        public override string ToString() {
            return $"[Strip | Position: {Position}, Texture: {Texture}]";
        }

        #endregion Public Methods

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
                _vertexBuffer = new DynamicVertexBuffer(Game.Instance.Core.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, sections * 4, BufferUsage.WriteOnly);
            }

            if (_indexBuffer == null || indices.Length > _indexBuffer.IndexCount) {
                _indexBuffer = new IndexBuffer(Game.Instance.Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            }

            _indexBuffer.SetData(indices);

#if DEBUG
            if (_debug_indexBuffer == null || debug_indices.Length > _debug_indexBuffer.IndexCount) {
                _debug_indexBuffer = new IndexBuffer(Game.Instance.Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, debug_indices.Length, BufferUsage.WriteOnly);
            }

            _debug_indexBuffer.SetData(debug_indices);
#endif
        }

        #endregion Private Methods
    }
}