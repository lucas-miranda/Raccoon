using Raccoon.Graphics;
using System;
using System.Xml;

namespace Raccoon.Tiled {
    public enum TiledImageFormat {
        PNG = 0,
        BMP,
        JPG,
        GIF
    }

    public class TiledImage {
        public TiledImage(string filepath, XmlElement imageElement) {
            Format = (TiledImageFormat) Enum.Parse(typeof(TiledImageFormat), imageElement.HasAttribute("format") ? imageElement.GetAttribute("format") : imageElement.GetAttribute("source").Substring(imageElement.GetAttribute("source").Length - 3), true);
            Source = filepath + imageElement.GetAttribute("source");
            Size = new Size(float.Parse(imageElement.GetAttribute("width"), System.Globalization.CultureInfo.InvariantCulture), float.Parse(imageElement.GetAttribute("height"), System.Globalization.CultureInfo.InvariantCulture));
            if (imageElement.HasAttribute("trans")) {
                Transparent = new Color(imageElement.GetAttribute("trans"));
            }

            XmlElement e = imageElement["data"];
            if (e != null) {
                TiledData data = new TiledData(e);
                // TODO: create image data (if exists)
            }
        }

        public TiledImageFormat Format { get; private set; }
        public string Source { get; private set; }
        public Color Transparent { get; private set; }
        public Size Size { get; private set; }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
    }
}
