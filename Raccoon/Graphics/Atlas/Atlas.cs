using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;

using Raccoon.Graphics.AtlasProcessors;

namespace Raccoon.Graphics {
    public class Atlas : IAsset {
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

        public Atlas(string textureFilename, string dataFilename) {
            if (string.IsNullOrWhiteSpace(textureFilename)) {
                throw new System.ArgumentException($"Invalid texture filename '{textureFilename}'.");
            } else if (string.IsNullOrWhiteSpace(dataFilename)) {
                throw new System.ArgumentException($"Invalid data filename '{dataFilename}'.");
            }

            Filename = textureFilename;
            if (Filename.Contains(".")) {
                Filename = Path.Combine(Game.Instance.ContentDirectory, Filename);
            }

            DataFilename = dataFilename;
            if (dataFilename.Contains(".")) {
                DataFilename = Path.Combine(Game.Instance.ContentDirectory, DataFilename);
            }

            Load();
        }

        public Atlas(Stream textureStream, string dataFilename) {
            if (string.IsNullOrWhiteSpace(dataFilename)) {
                throw new System.ArgumentException("Invalid data filename.");
            }

            DataFilename = dataFilename;
            Load(textureStream);
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; } = "Atlas";
        public string Filename { get; private set; }
        public string DataFilename { get; private set; }
        public Texture Texture { get; private set; }
        public Size Size { get { return Texture.Size; } }
        public bool IsDisposed { get; private set; }

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
            if (!_subTextures.TryGetValue(name.ToLowerInvariant(), out AtlasSubTexture subTexture)) {
                throw new AtlasSubTextureNotFoundException(name);
            }

            return subTexture;
        }

        public AtlasAnimation RetrieveAnimation(string name) {
            AtlasSubTexture subTexture = RetrieveSubTexture(name);

            if (!(subTexture is AtlasAnimation animation)) {
                throw new AtlasMismatchSubTextureTypeException(expectedType: typeof(AtlasAnimation), foundType: subTexture.GetType());
            }

            return animation;
        }

        public void Reload() {
            foreach (AtlasSubTexture subTexture in _subTextures.Values) {
                subTexture.Dispose();
            }

            _subTextures.Clear();
            Load();
        }

        public void Reload(Stream textureStream) {
            foreach (AtlasSubTexture subTexture in _subTextures.Values) {
                subTexture.Dispose();
            }

            _subTextures.Clear();
            Load(textureStream);
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            foreach (AtlasSubTexture subTexture in _subTextures.Values) {
                subTexture.Dispose();
            }

            Texture.Dispose();

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Private Methods

        private void Load() {
            Texture = new Texture(Filename);
            LoadData();
        }

        private void Load(Stream stream) {
            Texture = new Texture(stream);
            LoadData();
        }

        private void LoadData() {
            string dataFileText = File.ReadAllText(DataFilename);
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
                throw new System.InvalidOperationException($"Could not find a valid atlas data processor (to data file: '{DataFilename}' and texture file: '{Filename}').");
            }

            dataProcessor.ProcessJson(json, Texture, ref _subTextures);
        }

        #endregion Private Methods
    }
}
