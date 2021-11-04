using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;

namespace Raccoon.Fonts {
    public class FontTextureData {
        public FontTextureData(string dataFilepath) {
            dataFilepath = Path.Combine(Game.Instance.ContentDirectory, dataFilepath);

            if (!File.Exists(dataFilepath)) {
                throw new System.ArgumentException($"Font data file at provided path '{dataFilepath}' doesn't exists.");
            }

            string dataFileText = File.ReadAllText(dataFilepath);
            JObject json = JObject.Parse(dataFileText);
            JToken atlasToken = json.SelectToken("atlas", errorWhenNoMatch: true);

            if (!System.Enum.TryParse<FontTextureAtlasKind>(
                atlasToken["type"].Value<string>(), true, out FontTextureAtlasKind atlasKind
            )) {
                throw new System.NotImplementedException($"Atlas type '{atlasToken["type"].Value<string>()}' isn't implemented.");
            }

            switch (atlasKind) {
                case FontTextureAtlasKind.MTSDF:
                    DeserializeSignedDistanceField(atlasKind, json);
                    break;

                default:
                    throw new System.NotImplementedException($"Atlas kind '{atlasKind}' data deserialization isn't implemented.");
            }
        }

        public FontTextureDataAtlas Atlas { get; private set; }
        public FontTextureDataMetrics Metrics { get; private set; }
        public Dictionary<uint, FontTextureDataGlyph> Glyphs { get; } = new Dictionary<uint, FontTextureDataGlyph>();

        private void DeserializeSignedDistanceField(FontTextureAtlasKind kind, JObject json) {
            JToken token = json["atlas"];

            if (!System.Enum.TryParse<FontTextureYOriginKind>(
                token["yOrigin"].Value<string>(), true, out FontTextureYOriginKind yOriginKind
            )) {
                throw new System.NotImplementedException(
                    $"atlas.yOrigin value '{token["yOrigin"].Value<string>()}' isn't implemented."
                );
            }

            Atlas = new FontTextureDataAtlas(
                kind,
                token["distanceRange"].Value<float>(),
                token["size"].Value<float>(),
                token["width"].Value<float>(),
                token["height"].Value<float>(),
                yOriginKind
            );

            token = json["metrics"];

            Metrics = new FontTextureDataMetrics(
                token["emSize"].Value<float>(),
                token["lineHeight"].Value<float>(),
                token["ascender"].Value<float>(),
                token["descender"].Value<float>(),
                token["underlineY"].Value<float>(),
                token["underlineThickness"].Value<float>()
            );

            foreach (JToken glyphToken in json["glyphs"].Children()) {
                FontTextureDataGlyph glyph;

                JToken planeBounds = glyphToken["planeBounds"],
                       atlasBounds = glyphToken["atlasBounds"];

                uint unicode = glyphToken["unicode"].Value<uint>();
                if (planeBounds == null || atlasBounds == null) {
                    glyph = new FontTextureDataGlyph(
                        unicode,
                        glyphToken["advance"].Value<double>()
                    );
                } else {
                    glyph = new FontTextureDataGlyph(
                        unicode,
                        glyphToken["advance"].Value<double>(),
                        DeserializeBounds(planeBounds),
                        DeserializeBounds(atlasBounds)
                    );
                }

                Glyphs.Add(unicode, glyph);
            }
        }

        private FontTextureDataGlyphBounds DeserializeBounds(JToken obj) {
            return new FontTextureDataGlyphBounds(
                obj["top"].Value<double>(),
                obj["right"].Value<double>(),
                obj["bottom"].Value<double>(),
                obj["left"].Value<double>()
            );
        }
    }
}
