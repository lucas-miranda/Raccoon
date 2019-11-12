namespace Raccoon.Graphics {
    public class AtlasMismatchSubTextureTypeException : System.Exception {
        public AtlasMismatchSubTextureTypeException() {
        }

        public AtlasMismatchSubTextureTypeException(string message) : base(message) {
        }

        public AtlasMismatchSubTextureTypeException(System.Type expectedType, System.Type foundType) : base($"SubTexture mismatch type, expected '{expectedType.Name}', but got '{foundType.Name}'.") {
        }
    }
}
