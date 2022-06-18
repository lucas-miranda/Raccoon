
namespace Raccoon.FileSystem {
    public static class Directories {
        private static string _base;

        /// <summary>
        /// Game base directory.
        /// </summary>
        public static string Base {
            get {
                if (_base != null) {
                    return _base;
                }

                // it's the same code as Microsoft.Xna.Framework.SDL2_FNAPlatform.GetBaseDirectory()
                // with some minor adaptations
                string result;

                try {
                    string platform = SDL2.SDL.SDL_GetPlatform();

                    if (System.Environment.GetEnvironmentVariable("FNA_SDL2_FORCE_BASE_PATH") != "1") {
                        if (platform.Equals("Windows")
                          || platform.Equals("Mac OS X")
                          || platform.Equals("Linux")
                          || platform.Equals("FreeBSD")
                          || platform.Equals("OpenBSD")
                          || platform.Equals("NetBSD")
                        ) {
                            _base = System.AppDomain.CurrentDomain.BaseDirectory;
                            return _base;
                        }
                    }

                    result = SDL2.SDL.SDL_GetBasePath();

                    if (string.IsNullOrEmpty(result)) {
                        result = System.AppDomain.CurrentDomain.BaseDirectory;
                    }

                    if (string.IsNullOrEmpty(result)) {
                        result = System.Environment.CurrentDirectory;
                    }

                    _base = result;
                } catch (System.DllNotFoundException) {
                    // SDL2 isn't available yet
                    result = System.AppDomain.CurrentDomain.BaseDirectory;
                }

                return result;
            }
        }

        public static string Cache {
            get {
                throw new System.NotImplementedException();
            }
        }

        public static string Config {
            get {
                throw new System.NotImplementedException();
            }
        }
    }
}
