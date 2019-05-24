using System.Collections.Generic;
using System.Xml;

namespace Raccoon.Tiled {
    public class TiledImageLayer : ITiledLayer {
        public TiledImageLayer(string tmxPath, XmlElement layerElement) {
            Name = layerElement.GetAttribute("name");
            Opacity = layerElement.HasAttribute("opacity") ? float.Parse(layerElement.GetAttribute("opacity"), System.Globalization.CultureInfo.InvariantCulture) : 1f;
            Visible = layerElement.HasAttribute("visible") ? (int.Parse(layerElement.GetAttribute("visible")) != 0) : true;
            Offset = new Vector2(layerElement.HasAttribute("offsetx") ? float.Parse(layerElement.GetAttribute("offsetx"), System.Globalization.CultureInfo.InvariantCulture) : 0f, layerElement.HasAttribute("offsety") ? float.Parse(layerElement.GetAttribute("offsety"), System.Globalization.CultureInfo.InvariantCulture) : 0f);

            Properties = new Dictionary<string, TiledProperty>();
            XmlElement e = layerElement["properties"];
            if (e != null) {
                foreach (XmlElement propertyElement in e) {
                    Properties.Add(propertyElement.GetAttribute("name"), new TiledProperty(propertyElement));
                }
            }

            e = layerElement["image"];
            if (e != null) {
                Image = new TiledImage(tmxPath, e);
            }
        }

        public string Name { get; private set; }
        public float Opacity { get; private set; }
        public bool Visible { get; private set; }
        public Vector2 Offset { get; private set; }
        public Dictionary<string, TiledProperty> Properties { get; private set; }
        public TiledImage Image { get; private set; }
    }
}
