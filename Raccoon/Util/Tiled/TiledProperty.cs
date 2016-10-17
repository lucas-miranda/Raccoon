using Raccoon.Graphics;
using System;
using System.Xml;

namespace Raccoon.Tiled {
    public enum TiledPropertyType {
        Bool = 0,
        Color,
        Float,
        File,
        Int,
        String
    }

    public class TiledProperty {
        public TiledProperty(XmlElement e) {
            Name = e.GetAttribute("name");
            Type = e.HasAttribute("type") ? (TiledPropertyType) Enum.Parse(typeof(TiledPropertyType), e.GetAttribute("type"), true) : TiledPropertyType.String;
            string strValue = e.HasAttribute("value") ? e.GetAttribute("value") : e.InnerText;
            switch (Type) {
                case TiledPropertyType.Bool:
                    Value = bool.Parse(strValue);
                    break;

                case TiledPropertyType.Color:
                    Value = new Color(strValue);
                    break;

                case TiledPropertyType.Float:
                    Value = float.Parse(strValue);
                    break;

                case TiledPropertyType.Int:
                    Value = int.Parse(strValue);
                    break;

                case TiledPropertyType.String:
                case TiledPropertyType.File:
                default:
                    Value = strValue;
                    break;
            }
        }

        public string Name { get; private set; }
        public TiledPropertyType Type { get; private set; }
        public object Value { get; private set; }

        public bool GetBool() {
            return (bool) Value;
        }

        public Color GetColor() {
            return (Color) Value;
        }

        public float GetFloat() {
            return (float) Value;
        }

        public string GetFilename() {
            return GetString();
        }

        public int GetInt() {
            return (int) Value;
        }

        public string GetString() {
            return (string) Value;
        }
    }
}
