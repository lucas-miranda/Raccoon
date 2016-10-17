using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Raccoon.Tiled {
    public enum TiledObjectShape {
        Box = 0,
        Ellipse,
        Polygon,
        Polyline
    }

    public class TiledObject {
        private readonly Regex PointRegex = new Regex(@"(\-?\d+\.?\d*)\,(\-?\d+\.?\d*)");

        public TiledObject(XmlElement objectElement) {
            Id = int.Parse(objectElement.GetAttribute("id"));
            Name = objectElement.GetAttribute("name");
            Type = objectElement.GetAttribute("type");
            Visible = objectElement.HasAttribute("visible") ? (int.Parse(objectElement.GetAttribute("visible")) == 1) : true;
            Position = new Vector2(float.Parse(objectElement.GetAttribute("x"), System.Globalization.CultureInfo.InvariantCulture), float.Parse(objectElement.GetAttribute("y"), System.Globalization.CultureInfo.InvariantCulture));
            Rotation = objectElement.HasAttribute("rotation") ? float.Parse(objectElement.GetAttribute("rotation"), System.Globalization.CultureInfo.InvariantCulture) : 0f;

            if (objectElement.HasAttribute("width") && objectElement.HasAttribute("height")) {
                Size = new Size(float.Parse(objectElement.GetAttribute("width"), System.Globalization.CultureInfo.InvariantCulture), float.Parse(objectElement.GetAttribute("height"), System.Globalization.CultureInfo.InvariantCulture));
            }

            if (objectElement.HasAttribute("gid")) {
                Gid = int.Parse(objectElement.GetAttribute("gid"));
            }

            Properties = new Dictionary<string, TiledProperty>();
            XmlElement e = objectElement["properties"];
            if (e != null) {
                foreach (XmlElement propertyElement in e) {
                    Properties.Add(propertyElement.GetAttribute("name"), new TiledProperty(propertyElement));
                }
            }

            Points = new List<Vector2>();
            if (objectElement.HasChildNodes) {
                XmlNode child = objectElement.FirstChild;
                switch (child.Name) {
                    case "ellipse":
                        Shape = TiledObjectShape.Ellipse;
                        break;

                    case "polygon":
                        Shape = TiledObjectShape.Polygon;
                        foreach (Match m in PointRegex.Matches(child.Attributes["points"].Value)) {
                            Points.Add(new Vector2(float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)));
                        }
                        break;

                    case "polyline":
                        Shape = TiledObjectShape.Polyline;
                        foreach (Match m in PointRegex.Matches(child.Attributes["points"].Value)) {
                            Points.Add(new Vector2(float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)));
                        }
                        break;

                    default:
                        break;
                }
            } else {
                Shape = TiledObjectShape.Box;
                Points.Add(new Vector2(0, 0));
                Points.Add(new Vector2(Width, 0));
                Points.Add(new Vector2(Width, Height));
                Points.Add(new Vector2(0, Height));
            }
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public bool Visible { get; private set; }
        public Vector2 Position { get; private set; }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public Size Size { get; private set; }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
        public float Rotation { get; private set; }
        public int? Gid { get; private set; }
        public TiledObjectShape Shape { get; private set; }
        public Dictionary<string, TiledProperty> Properties { get; private set; }
        public List<Vector2> Points { get; private set; }
    }
}
