using Raccoon.Graphics;

namespace Raccoon {
    public interface IRenderable {
        bool Visible { get; set; }
        int Layer { get; set; }
        Renderer Renderer { get; set; }

        void Render();
    }
}
