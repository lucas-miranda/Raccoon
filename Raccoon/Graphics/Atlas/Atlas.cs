using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;

using Raccoon.Graphics.AtlasProcessors;

namespace Raccoon.Graphics {
    public class Atlas {
        #region Private Members

        private static readonly IAtlasProcessor[] _processors;

        private Dictionary<string, AtlasSubTexture> _subTextures = new Dictionary<string, AtlasSubTexture>();

        #endregion Private Members

        #region Constructors

        static Atlas() {
            _processors = new IAtlasProcessor[] {
                new AsepriteAtlasProcessor(),
                new RavenAtlasProcessor()
            };
        }

        public Atlas(string imageFilename, string dataFilename) {
            Texture = new Texture(imageFilename);

            string dataFileText = File.ReadAllText(dataFilename);
            JObject json = JObject.Parse(dataFileText);

            JToken metaToken = json.SelectToken("meta", errorWhenNoMatch: true);
            IAtlasProcessor dataProcessor = null;

            foreach (IAtlasProcessor processor in _processors) {
                // choose first processor which accepts meta
                if (processor.VerifyJsonMeta(metaToken)) {
                    dataProcessor = processor;
                    break;
                }
            }

            if (dataProcessor == null) {
                throw new System.InvalidOperationException($"Could not find a valid atlas data processor (to file: '{dataFilename}').");
            }

            dataProcessor.ProcessJson(json, Texture, ref _subTextures);
        }

        #endregion Constructors

        #region Public Properties

        public Texture Texture { get; private set; }
        public Size Size { get { return Texture.Size; } }

        public AtlasSubTexture this[string name] {
            get {
                return _subTextures[name.ToLowerInvariant()];
            }
        }

        public AtlasSubTexture this[int x, int y, int width, int height] {
            get {
                return new AtlasSubTexture(Texture, Texture.Bounds, new Rectangle(x, y, width, height));
            }
        }

        public AtlasSubTexture this[Rectangle region] {
            get {
                return new AtlasSubTexture(Texture, Texture.Bounds, region);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public AtlasSubTexture RetrieveSubTexture(string name) {
            return _subTextures[name.ToLowerInvariant()];
        }

        public AtlasAnimation RetrieveAnimation(string name) {
            if (!(_subTextures[name.ToLowerInvariant()] is AtlasAnimation animation)) {
                return null;
            }

            return animation;
        }

        #endregion Public Methods
    }
}
