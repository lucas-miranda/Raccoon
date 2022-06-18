using System.IO;

namespace Raccoon.FileSystem {
    /// <summary>
    /// Works as a wrapper to <see cref="System.IO.File"/>, but with normalized paths and trying to be safer.
    /// </summary>
    public static class SafeFile {
        private const char ForwardSlash = '/',
                           BackwardSlash = '\\';

        public static FileStream OpenRead(string filepath) {
            filepath = NormalizePath(filepath);

            if (System.IO.Path.IsPathRooted(filepath)) {
                return File.OpenRead(filepath);
            }

            return File.OpenRead(System.IO.Path.Combine(Directories.Base, filepath));
        }

        public static FileStream OpenWrite(string filepath) {
            filepath = NormalizePath(filepath);

            if (System.IO.Path.IsPathRooted(filepath)) {
                return File.OpenWrite(filepath);
            }

            return File.OpenWrite(System.IO.Path.Combine(Directories.Base, filepath));
        }

        public static string NormalizePath(string filepath) {
            if (System.IO.Path.DirectorySeparatorChar == ForwardSlash) {
                return filepath.Replace(BackwardSlash, ForwardSlash);
            }

            return filepath.Replace(ForwardSlash, BackwardSlash);
        }

    }
}
