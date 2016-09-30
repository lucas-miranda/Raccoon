using System;
using System.Xml;

namespace Raccoon.Tiled {
    public enum TiledPropertyType {
        String = 0,
        Int,
        Float,
        Bool
    }

    public class TiledProperty {
        public TiledProperty(XmlElement e) {
            Name = e.GetAttribute("name");
            Type = e.HasAttribute("type") ? (TiledPropertyType) Enum.Parse(typeof(TiledPropertyType), e.GetAttribute("type"), true) : TiledPropertyType.String;
            string value = e.HasAttribute("value") ? e.GetAttribute("value") : e.InnerText;
            switch (Type) {
                case TiledPropertyType.Int:
                    Value = int.Parse(value);
                    break;

                case TiledPropertyType.Float:
                    Value = float.Parse(value);
                    break;

                case TiledPropertyType.Bool:
                    Value = bool.Parse(value);
                    break;

                case TiledPropertyType.String:
                default:
                    Value = value;
                    break;
            }
        }

        public string Name { get; private set; }
        public TiledPropertyType Type { get; private set; }
        public object Value { get; private set; }

        public string GetString() {
            return (string) Value;
        }

        public void SetString(string value) {
            Value = value;
        }

        public int GetInt() {
            return (int) Value;
        }

        public void SetInt(int value) {
            Value = value;
        }

        public float GetFloat() {
            return (float) Value;
        }

        public void SetFloat(float value) {
            Value = value;
        }

        public bool GetBool() {
            return (bool) Value;
        }

        public void SetBool(bool value) {
            Value = value;
        }
    }
}
