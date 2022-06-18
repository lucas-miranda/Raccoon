
namespace Raccoon.FileSystem {
    public static class Path {
        public static readonly char Separator = System.IO.Path.DirectorySeparatorChar;
        public static readonly char[] Separators, InvalidPathChars, InvalidFileNameChars;

        static Path() {
            char universalPathSeparator = '/';

            if (!(System.IO.Path.DirectorySeparatorChar == universalPathSeparator
             || System.IO.Path.AltDirectorySeparatorChar == universalPathSeparator
            )) {
                Separators = new char[] {
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar,
                    '/',
                };
            } else {
                Separators = new char[] {
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar,
                };
            }

            InvalidPathChars = System.IO.Path.GetInvalidPathChars();
            InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
        }

        public static bool IsSeparator(char c) {
            for (int i = 0; i < Separators.Length; i++) {
                if (c == Separators[i]) {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidChar(char c) {
            foreach (char invalidChar in InvalidPathChars) {
                if (c == invalidChar) {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValid(string path) {
            foreach (char c in path) {
                if (!IsValidChar(c)) {
                    return false;
                }
            }

            return true;
        }
    }
}
