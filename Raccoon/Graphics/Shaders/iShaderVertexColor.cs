using Microsoft.Xna.Framework;

namespace Raccoon.Graphics {
    public interface IShaderVertexColor {
        Color DiffuseColor { get; set; }
        float Alpha { get; set; }
    }
}
