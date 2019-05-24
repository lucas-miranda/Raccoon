using System.Collections.Generic;
using System.Xml;

namespace Raccoon.Tiled {
    public class TiledTileset {
        private TiledTileset() {
            Properties = new Dictionary<string, TiledProperty>();
            TerrainTypes = new List<TiledTerrainType>();
            Tiles = new Dictionary<uint, TiledTilesetTile>();
        }

        public TiledTileset(string tsxFilename, int firstGid) : this() {
            FirstGid = firstGid;
            FullFilename = tsxFilename;
            string[] tsxFilenameSections = FullFilename.Split('/');
            Filename = tsxFilenameSections[tsxFilenameSections.Length - 1];
            Path = string.Join("/", tsxFilenameSections, 0, tsxFilenameSections.Length - 1) + "/";
            XmlDocument _xmlDoc = new XmlDocument();
            _xmlDoc.Load(FullFilename);
            Load(_xmlDoc["tileset"]);
        }

        public TiledTileset(string tmxFilepath, XmlElement tilesetElement) : this() {
            Path = tmxFilepath;
            Load(tilesetElement);
        }

        public string Name { get; private set; }
        public string Filename { get; private set; }
        public string Path { get; private set; }
        public string FullFilename { get; private set; }
        public int FirstGid { get; private set; }
        public Size TileSize { get; private set; }
        public float Spacing { get; private set; }
        public float Margin { get; private set; }
        public int Count { get; private set; }
        public int Columns { get; private set; }
        public Vector2 Offset { get; private set; }
        public TiledImage SpriteSheet { get; private set; }
        public Dictionary<string, TiledProperty> Properties { get; private set; }
        public Dictionary<uint, TiledTilesetTile> Tiles { get; private set; }
        public List<TiledTerrainType> TerrainTypes { get; private set; }

        private void Load(XmlElement tilesetElement) {
            Name = tilesetElement.GetAttribute("name");
            TileSize = new Size(int.Parse(tilesetElement.GetAttribute("tilewidth")), int.Parse(tilesetElement.GetAttribute("tileheight")));
            Spacing = tilesetElement.HasAttribute("spacing") ? float.Parse(tilesetElement.GetAttribute("spacing"), System.Globalization.CultureInfo.InvariantCulture) : 0f;
            Margin = tilesetElement.HasAttribute("margin") ? float.Parse(tilesetElement.GetAttribute("margin"), System.Globalization.CultureInfo.InvariantCulture) : 0f;
            Count = int.Parse(tilesetElement.GetAttribute("tilecount"));
            Columns = int.Parse(tilesetElement.GetAttribute("columns"));

            if (tilesetElement.HasAttribute("firstgid")) {
                FirstGid = int.Parse(tilesetElement.GetAttribute("firstgid"));
            }

            XmlElement e = tilesetElement["tileoffset"];
            if (e != null) {
                Offset = new Vector2(float.Parse(e.GetAttribute("x"), System.Globalization.CultureInfo.InvariantCulture), float.Parse(e.GetAttribute("y"), System.Globalization.CultureInfo.InvariantCulture));
            }

            e = tilesetElement["properties"];
            if (e != null) {
                foreach (XmlElement propertyElement in e) {
                    Properties.Add(propertyElement.GetAttribute("name"), new TiledProperty(propertyElement));
                }
            }

            e = tilesetElement["image"];
            if (e != null) {
                SpriteSheet = new TiledImage(Path, e);
            }

            e = tilesetElement["terraintypes"];
            if (e != null) {
                foreach (XmlElement terrainTypeElement in e) {
                    TerrainTypes.Add(new TiledTerrainType(terrainTypeElement));
                }
            }

            foreach (XmlElement tileElement in tilesetElement.GetElementsByTagName("tile")) {
                Tiles.Add(uint.Parse(tileElement.GetAttribute("id")), new TiledTilesetTile(Path, tileElement));
            }
        }
    }
}
