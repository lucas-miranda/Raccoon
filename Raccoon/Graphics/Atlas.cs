using System.Collections.Generic;
using System.Xml;

namespace Raccoon.Graphics {
    public class Atlas {
        #region Private Members

        private Dictionary<string, AtlasSubTexture> _subTexturesDict;

        #endregion Private Members

        #region Constructors

        public Atlas(string imageFilename, string dataFilename) {
            _subTexturesDict = new Dictionary<string, AtlasSubTexture>();
            Texture = new Texture(imageFilename);

            // loading data file
            XmlDocument _xmlDoc = new XmlDocument();
            _xmlDoc.Load(dataFilename);
            XmlElement _rootElement = _xmlDoc["TextureAtlas"];

            // populating sub textures
            foreach (XmlElement spriteElement in _rootElement) {
                AtlasSubTexture subTexture = new AtlasSubTexture(Texture, new Rectangle(int.Parse(spriteElement.Attributes["x"].Value), int.Parse(spriteElement.Attributes["y"].Value), int.Parse(spriteElement.Attributes["w"].Value), int.Parse(spriteElement.Attributes["h"].Value)));
                _subTexturesDict.Add(spriteElement.Attributes["n"].Value, subTexture);
            }
        }

        #endregion Constructors

        #region Public Properties

        public Texture Texture { get; private set; }
        public Size Size { get { return Texture.Size; } }

        public AtlasSubTexture this[string name] {
            get {
                return _subTexturesDict[name];
            }
        }

        public AtlasSubTexture this[int x, int y, int width, int height] {
            get {
                return new AtlasSubTexture(Texture, new Rectangle(x, y, width, height));
            }
        }

        #endregion Public Properties
    }

    public struct AtlasSubTexture {
        public Texture Texture;
        public Rectangle Region;

        public AtlasSubTexture(Texture texture, Rectangle region) {
            Texture = texture;
            Region = region;
        }
    }
}
