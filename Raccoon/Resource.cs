using System.IO;

namespace Raccoon {
    public class Resource {
        private static byte[] Cache_04b03,
                              CacheBasicShader,
                              CacheRepeatShader,
                              CacheFontMTSDFShader;

        public static byte[] _04b03 {
            get {
				if (Cache_04b03 == null) {
					Cache_04b03 = GetResource("_04b03");
				}

				return Cache_04b03;
            }
        }

        public static byte[] BasicShader {
            get {
				if (CacheBasicShader == null) {
					CacheBasicShader = GetResource("BasicShader");
				}

				return CacheBasicShader;
            }
        }

        public static byte[] RepeatShader {
            get {
				if (CacheRepeatShader == null) {
					CacheRepeatShader = GetResource("RepeatShader");
				}

				return CacheRepeatShader;
            }
        }

        public static byte[] FontMTSDFShader {
            get {
				if (CacheFontMTSDFShader == null) {
					CacheFontMTSDFShader = GetResource("FontMTSDFShader");
				}

				return CacheFontMTSDFShader;
            }
        }

		private static byte[] GetResource(string name)
		{
			Stream stream = typeof(Resource).Assembly.GetManifestResourceStream("Raccoon.Resource." + name);
			using (MemoryStream ms = new MemoryStream()) {
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}
    }
}
