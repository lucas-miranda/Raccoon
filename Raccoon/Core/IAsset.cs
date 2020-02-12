using System.IO;

namespace Raccoon {
    public interface IAsset : System.IDisposable {
        string Name { get; set; }
        string Filename { get; }
        bool IsDisposed { get; }

        void Reload();
        void Reload(Stream stream);
    }
}
