using Raccoon.Graphics;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using System;

namespace Raccoon.Tiled {
    public enum TiledDrawOrder {
        TopDown = 0,
        Index
    }

    public class TiledObjectGroup : IEnumerable<TiledObject> {
        private List<TiledObject> _objects;

        public TiledObjectGroup(XmlElement objectGroupElement) {
            Name = objectGroupElement.GetAttribute("name");
            Color = objectGroupElement.HasAttribute("color") ? new Color(objectGroupElement.GetAttribute("color")) : Color.White;
            Opacity = objectGroupElement.HasAttribute("opacity") ? float.Parse(objectGroupElement.GetAttribute("opacity"), System.Globalization.CultureInfo.InvariantCulture) : 1f;
            Visible = objectGroupElement.HasAttribute("visible") ? (int.Parse(objectGroupElement.GetAttribute("visible")) == 1) : true;
            Offset = new Vector2(
                objectGroupElement.HasAttribute("offsetx") ? float.Parse(objectGroupElement.GetAttribute("offsetx"), System.Globalization.CultureInfo.InvariantCulture) : 0f,
                objectGroupElement.HasAttribute("offsety") ? float.Parse(objectGroupElement.GetAttribute("offsety"), System.Globalization.CultureInfo.InvariantCulture) : 0f
            );
            DrawOrder = objectGroupElement.HasAttribute("draworder") ? (TiledDrawOrder) Enum.Parse(typeof(TiledDrawOrder), objectGroupElement.GetAttribute("draworder"), true) : TiledDrawOrder.TopDown;

            _objects = new List<TiledObject>();
            foreach (XmlElement objectElement in objectGroupElement.GetElementsByTagName("object")) {
                _objects.Add(new TiledObject(objectElement));
            }

            Properties = new Dictionary<string, TiledProperty>();
            if (objectGroupElement["properties"] != null) {
                foreach (XmlElement propertyElement in objectGroupElement["properties"]) {
                    Properties.Add(propertyElement.GetAttribute("name"), new TiledProperty(propertyElement));
                }
            }
        }

        public string Name { get; protected set; }
        public Color Color { get; private set; }
        public float Opacity { get; protected set; }
        public bool Visible { get; protected set; }
        public Vector2 Offset { get; protected set; }
        public TiledDrawOrder DrawOrder { get; private set; }
        public int Count { get { return _objects.Count; } }
        public Dictionary<string, TiledProperty> Properties { get; protected set; }

        public TiledObject this[int i] {
            get {
                return _objects[i];
            }
        }

        public IEnumerator<TiledObject> GetEnumerator() {
            return _objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
