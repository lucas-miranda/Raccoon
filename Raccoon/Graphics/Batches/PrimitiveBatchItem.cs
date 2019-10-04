using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class PrimitiveBatchItem : IBatchItem {
        public PrimitiveBatchItem() {
        }

        public Shader Shader { get; private set; }
        public IShaderParameters ShaderParameters { get; private set; }
        public VertexPositionColorTexture[] VertexData { get; private set; }
        public Texture Texture { get; private set; }
        public int[] IndexData { get; private set; }
        public bool IsHollow { get; private set; }

        public void Set(VertexPositionColorTexture[] vertexData, int[] indexData, bool isHollow, Shader shader, IShaderParameters shaderParameters, Texture texture) {
            VertexData = vertexData;
            IndexData = indexData;
            IsHollow = isHollow;
            Shader = shader;
            ShaderParameters = shaderParameters;
            Texture = texture;
        }

        public void Clear() {
            Shader = null;
            ShaderParameters = null;
            VertexData = null;
            Texture = null;
            IndexData = null;
        }
    }
}
