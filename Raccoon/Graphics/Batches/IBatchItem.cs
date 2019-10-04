using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public interface IBatchItem {
        Shader Shader { get; }
        IShaderParameters ShaderParameters { get; }
        VertexPositionColorTexture[] VertexData { get; }
        int[] IndexData { get; }
        Texture Texture { get; }

        void Clear();
    }
}
