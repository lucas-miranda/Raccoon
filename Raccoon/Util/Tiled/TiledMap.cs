using Raccoon.Graphics;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Raccoon.Tiled {
    public enum TiledMapOrientation {
        Orthogonal = 0,
        Isometric,
        Staggered,
        Hexagonal
    }

    public enum TiledRenderOrder {
        RightDown = 0,
        RightUp,
        LeftDown,
        LeftUp
    }

    public enum TiledStaggerAxis {
        X = 0,
        Y
    }

    public enum TiledStaggerIndex {
        Even = 0,
        Odd
    }

    public class TiledMap {
        private XmlDocument _xmlDoc;
        private XmlElement _rootElement;

        public TiledMap(string filename) {
            Properties = new Dictionary<string, TiledProperty>();
            Tilesets = new List<TiledTileset>();
            Layers = new List<ITiledLayer>();
            _xmlDoc = new XmlDocument();
            FullFilename = filename;
            string[] filenameSections = FullFilename.Split('/');
            Filename = filenameSections[filenameSections.Length - 1];
            Path = string.Join("/", filenameSections, 0, filenameSections.Length - 1) + "/";
            Load();
        }

        public string Filename { get; private set; }
        public string Path { get; private set; }
        public string FullFilename { get; private set; }
        public string Version { get; private set; }
        public TiledMapOrientation Orientation { get; private set; }
        public TiledRenderOrder RenderOrder { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public Size TileSize { get; private set; }
        public Color BackgroundColor { get; private set; }
        public int NextObjectId { get; private set; }
        public int HexSideLength { get; private set; }
        public TiledStaggerAxis StaggerAxis { get; private set; }
        public TiledStaggerIndex StaggerIndex { get; private set; }
        public Dictionary<string, TiledProperty> Properties { get; private set; }
        public List<TiledTileset> Tilesets { get; private set; }
        public List<ITiledLayer> Layers { get; private set; }

        private void Load() {
            _xmlDoc.Load(FullFilename);
            _rootElement = _xmlDoc["map"];

            // map header info
            Version = _rootElement.GetAttribute("version");
            Orientation = (TiledMapOrientation) Enum.Parse(typeof(TiledMapOrientation), _rootElement.GetAttribute("orientation"), true);
            RenderOrder = (TiledRenderOrder) Enum.Parse(typeof(TiledRenderOrder), _rootElement.GetAttribute("renderorder").Replace("-", ""), true);
            Columns = int.Parse(_rootElement.GetAttribute("width"));
            Rows = int.Parse(_rootElement.GetAttribute("height"));
            TileSize = new Size(int.Parse(_rootElement.GetAttribute("tilewidth")), int.Parse(_rootElement.GetAttribute("tileheight")));
            NextObjectId = int.Parse(_rootElement.GetAttribute("nextobjectid"));

            if (_rootElement.HasAttribute("backgroundcolor")) {
                BackgroundColor = new Color(_rootElement.GetAttribute("backgroundcolor"));
            }

            if (Orientation == TiledMapOrientation.Hexagonal) {
                HexSideLength = int.Parse(_rootElement.GetAttribute("hexsidelength"));
            }

            if (Orientation == TiledMapOrientation.Hexagonal || Orientation == TiledMapOrientation.Staggered) {
                StaggerAxis = (TiledStaggerAxis) Enum.Parse(typeof(TiledStaggerAxis), _rootElement.GetAttribute("staggeraxis"), true);
                StaggerIndex = (TiledStaggerIndex) Enum.Parse(typeof(TiledStaggerIndex), _rootElement.GetAttribute("staggerindex"), true);
            }

            // properties
            XmlElement element = _rootElement["properties"];
            if (element != null) {
                foreach (XmlElement propertyElement in element) {
                    Properties.Add(propertyElement.GetAttribute("name"), new TiledProperty(propertyElement));
                }
            }

            // tilesets
            foreach (XmlElement tilesetElement in _rootElement.GetElementsByTagName("tileset")) {
                TiledTileset tileset = !tilesetElement.HasAttribute("source") ? new TiledTileset(Path, tilesetElement) : new TiledTileset(Path + tilesetElement.GetAttribute("source"), int.Parse(tilesetElement.GetAttribute("firstgid")));
                Tilesets.Add(tileset);
            }

            // layers
            foreach (XmlElement layerElement in _rootElement) {
                ITiledLayer layer = null;
                switch (layerElement.Name) {
                    case "layer":
                        layer = new TiledTileLayer(layerElement);
                        break;

                    case "objectgroup":
                        layer = new TiledObjectLayer(layerElement);
                        break;

                    case "imagelayer":
                        layer = new TiledImageLayer(Path, layerElement);
                        break;

                    default:
                        break;
                }

                if (layer != null) {
                    Layers.Add(layer);
                }
            }
        }
    }
}
