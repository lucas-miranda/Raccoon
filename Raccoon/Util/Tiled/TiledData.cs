using System;
using System.Xml;

namespace Raccoon.Tiled {
    public enum TiledEncoding {
        XML = 0,
        CSV,
        Base64
    }

    public enum TiledCompression {
        None = 0,
        Gzip,
        Zlib
    }

    public class TiledData {
        public TiledData(XmlElement dataElement) {
            Encoding = dataElement.HasAttribute("encoding") ? (TiledEncoding) Enum.Parse(typeof(TiledEncoding), dataElement.GetAttribute("encoding"), true) : TiledEncoding.XML;
            Compression = dataElement.HasAttribute("compression") ? (TiledCompression) Enum.Parse(typeof(TiledCompression), dataElement.GetAttribute("compression"), true) : TiledCompression.None;
            if (Encoding == TiledEncoding.XML) {
                string csv = "";
                XmlNode tileElement = dataElement.FirstChild;
                while (tileElement != null) {
                    csv += tileElement.Attributes["id"].Value + (tileElement.NextSibling != null ? ", " : "");
                    tileElement = tileElement.NextSibling;
                }

                Content = csv;
            } else {
                Content = dataElement.InnerText;
            }
        }

        public TiledEncoding Encoding { get; private set; }
        public TiledCompression Compression { get; private set; }
        public string Content { get; private set; }
    }
}
