using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class PrimitiveBatchItem {
        public PrimitiveBatchItem(VertexPositionColor[] vertexData, int[] indexData) {
            VertexData = vertexData;
            IndexData = indexData;
        }

        public VertexPositionColor[] VertexData { get; private set; }
        public int[] IndexData { get; private set; }
    }
}
