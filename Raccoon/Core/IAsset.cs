using System.IO;

namespace Raccoon {
    public interface IAsset : System.IDisposable {
        string Name { get; }
        //string Filename { get; }
        string[] Filenames { get; }
        bool IsDisposed { get; }

        void Reload();
        void Reload(Stream stream);
    }
}
