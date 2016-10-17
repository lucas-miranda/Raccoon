using System.Collections.Generic;
using System.Xml;

namespace Raccoon.Tiled {
    public class TiledTilesetTile {
        public TiledTilesetTile(string tsxPath, XmlElement tileElement) {
            Properties = new Dictionary<string, TiledProperty>();
            Id = int.Parse(tileElement.GetAttribute("id"));
            Probability = tileElement.HasAttribute("probability") ? float.Parse(tileElement.GetAttribute("probability"), System.Globalization.CultureInfo.InvariantCulture) : 1f;

            TerrainCorners = new int?[4] { null, null, null, null };
            if (tileElement.HasAttribute("terrain")) {
                string[] terrainCorners = tileElement.GetAttribute("terrain").Split(',');
                if (terrainCorners[0].Length > 0) {
                    TerrainTopLeftCorner = int.Parse(terrainCorners[0]);
                }

                if (terrainCorners[1].Length > 0) {
                    TerrainTopRightCorner = int.Parse(terrainCorners[1]);
                }

                if (terrainCorners[2].Length > 0) {
                    TerrainBottomLeftCorner = int.Parse(terrainCorners[2]);
                }

                if (terrainCorners[3].Length > 0) {
                    TerrainBottomRightCorner = int.Parse(terrainCorners[3]);
                }
            }

            XmlElement e = tileElement["animation"];
            if (e != null) {
                Animation = new TiledAnimation(e);
            }

            e = tileElement["objectgroup"];
            if (e != null) {
                ObjectGroup = new TiledObjectGroup(e);
            }

            e = tileElement["image"];
            if (e != null) {
                Image = new TiledImage(tsxPath, e);
            }

            e = tileElement["properties"];
            if (e != null) {
                foreach (XmlElement propertyElement in e) {
                    Properties.Add(propertyElement.GetAttribute("name"), new TiledProperty(propertyElement));
                }
            }
        }

        public int Id { get; private set; }
        public int?[] TerrainCorners { get; private set; }
        public int? TerrainTopLeftCorner { get { return TerrainCorners[0]; } private set { TerrainCorners[0] = value; } }
        public int? TerrainTopRightCorner { get { return TerrainCorners[1]; } private set { TerrainCorners[1] = value; } }
        public int? TerrainBottomLeftCorner { get { return TerrainCorners[2]; } private set { TerrainCorners[2] = value; } }
        public int? TerrainBottomRightCorner { get { return TerrainCorners[3]; } private set { TerrainCorners[3] = value; } }
        public float Probability { get; private set; }
        public Dictionary<string, TiledProperty> Properties { get; private set; }
        public TiledImage Image { get; private set; }
        public TiledObjectGroup ObjectGroup { get; private set; }
        public TiledAnimation Animation { get; private set; }
    }
}
