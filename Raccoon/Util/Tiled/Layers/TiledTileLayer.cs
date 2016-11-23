using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Raccoon.Tiled {
    public class TiledTileLayer : ITiledLayer {
        private readonly Regex GidRegex = new Regex(@"(\d+)");
        private TiledTile[][] _tiles;

        public TiledTileLayer(XmlElement layerElement) {
            Name = layerElement.GetAttribute("name");
            Columns = int.Parse(layerElement.GetAttribute("width"));
            Rows = int.Parse(layerElement.GetAttribute("height"));
            Opacity = layerElement.HasAttribute("opacity") ? float.Parse(layerElement.GetAttribute("opacity"), System.Globalization.CultureInfo.InvariantCulture) : 1f;
            Visible = layerElement.HasAttribute("visible") ? bool.Parse(layerElement.GetAttribute("visible")) : true;
            Offset = new Vector2(layerElement.HasAttribute("offsetx") ? float.Parse(layerElement.GetAttribute("offsetx"), System.Globalization.CultureInfo.InvariantCulture) : 0f, layerElement.HasAttribute("offsety") ? float.Parse(layerElement.GetAttribute("offsety"), System.Globalization.CultureInfo.InvariantCulture) : 0f);

            Properties = new Dictionary<string, TiledProperty>();
            XmlElement e = layerElement["properties"];
            if (e != null) {
                foreach (XmlElement propertyElement in e) {
                    Properties.Add(propertyElement.GetAttribute("name"), new TiledProperty(propertyElement));
                }
            }

            // start tiles data
            _tiles = new TiledTile[Rows][];
            for (int row = 0; row < Rows; row++) {
                TiledTile[] tilesRow = _tiles[row] = new TiledTile[Columns];
                for (int column = 0; column < Columns; column++) {
                    tilesRow[column] = new TiledTile(0);
                }
            }

            e = layerElement["data"];
            if (e != null) {
                TiledData data = new TiledData(e);
                switch (data.Encoding) {
                    case TiledEncoding.XML:
                    case TiledEncoding.CSV:
                        int x = 0, y = 0;
                        foreach (Match m in GidRegex.Matches(data.Content)) {
                            uint gid = uint.Parse(m.Value);
                            if (gid > 0) {
                                _tiles[y][x] = new TiledTile(gid);
                            }

                            x++;
                            if (x == Columns) {
                                x = 0;
                                y++;
                                if (y == Rows) {
                                    break;
                                }
                            }
                        }
                        break;

                    case TiledEncoding.Base64:
                        // TODO: read layer data using Base64 encoding
                        break;

                    default:
                        break;
                }
            }
        }

        public string Name { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public float Opacity { get; private set; }
        public bool Visible { get; private set; }
        public Vector2 Offset { get; private set; }
        public Dictionary<string, TiledProperty> Properties { get; private set; }

        public TiledTile[] this[int y] {
            get {
                return _tiles[y];
            }
        }

        public TiledTile this[int x, int y] {
            get {
                return _tiles[y][x];
            }
        }
    }
}
