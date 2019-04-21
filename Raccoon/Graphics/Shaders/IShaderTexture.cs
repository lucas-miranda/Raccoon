using Microsoft.Xna.Framework;

namespace Raccoon.Graphics {
    public interface IShaderTexture {
        Texture Texture { get; set; }
        bool TextureEnabled { get; set; }
    }
}
