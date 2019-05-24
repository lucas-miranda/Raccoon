using System.Collections.Generic;
using System.Xml;

namespace Raccoon.Tiled {
    public class TiledTerrainType {
        public TiledTerrainType(XmlElement terrainTypeElement) {
            Name = terrainTypeElement.GetAttribute("name");
            int tileId = int.Parse(terrainTypeElement.GetAttribute("tile"));
            if (tileId < 0) {
                TileId = null;
            } else {
                TileId = tileId;
            }

            Properties = new Dictionary<string, TiledProperty>();
            XmlElement propertiesElement = terrainTypeElement["properties"];
            if (propertiesElement != null) {
                foreach (XmlElement propertyElement in propertiesElement) {
                    Properties.Add(propertyElement.GetAttribute("name"), new TiledProperty(propertyElement));
                }
            }
        }

        public string Name { get; private set; }
        public int? TileId { get; private set; }
        public Dictionary<string, TiledProperty> Properties { get; private set; }
    }
}
