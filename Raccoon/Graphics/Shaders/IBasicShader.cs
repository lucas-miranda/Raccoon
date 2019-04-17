using Microsoft.Xna.Framework;

namespace Raccoon.Graphics {
    public interface IBasicShader {
        Matrix World { get; set; }
        Matrix View { get; set; }
        Matrix Projection { get; set; }
        Color DiffuseColor { get; set; }
        float Alpha { get; set; }
        Texture Texture { get; set; }
    }
}
