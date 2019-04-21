using Microsoft.Xna.Framework;

namespace Raccoon.Graphics {
    public interface IShaderTransform {
        Matrix World { get; set; }
        Matrix View { get; set; }
        Matrix Projection { get; set; }
    }
}
