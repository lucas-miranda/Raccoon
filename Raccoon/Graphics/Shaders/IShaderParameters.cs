namespace Raccoon.Graphics {
    /// <summary>
    /// Specifies parameters values to be applied, and how to apply those, to a Shader before rendering.
    /// For example: Draw Graphic A and Graphic B using the same Shader, but with slightly different parameters.
    /// </summary>
    public interface IShaderParameters : System.IEquatable<IShaderParameters> {
        void ApplyParameters(Shader shader);
        IShaderParameters Clone();
    }
}
