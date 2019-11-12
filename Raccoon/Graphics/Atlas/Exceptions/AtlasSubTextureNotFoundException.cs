namespace Raccoon.Graphics {
    public class AtlasSubTextureNotFoundException : System.Exception {
        public AtlasSubTextureNotFoundException(string subTextureName) : base($"SubTexture with name '{subTextureName}'") {
        }
    }
}
