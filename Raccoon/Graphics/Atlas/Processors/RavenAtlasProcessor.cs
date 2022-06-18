using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Raccoon.Graphics.AtlasProcessors {
    [System.Obsolete(
        "Raven is completely deprecated. Use clymene and ClymeneAtlasProcessor instead.",
        false
    )]
    public class RavenAtlasProcessor : IAtlasProcessor {
        public bool VerifyJsonMeta(JToken metaToken) {
            JToken ravenAppToken = metaToken.SelectToken("raven.app");

            if (ravenAppToken == null) {
                return false;
            }

            string app = ravenAppToken.Value<string>().ToLowerInvariant();
            return app.Equals("https://github.com/lucas-miranda/raven");
        }

        public bool ProcessJson(JObject json, Texture texture, ref Dictionary<string, AtlasSubTexture> subTextures) {
            //JToken texturesToken = json.SelectToken("textures");
            //JToken metaToken = json.SelectToken("meta");

            JToken animationsToken = json.SelectToken("animations");
            IEnumerable<JProperty> animationProperties = animationsToken.Children<JProperty>();
            foreach (JProperty animationProperty in animationProperties) {
                string subTextureName = animationProperty.Name.ToLowerInvariant();

                JToken animationToken = animationProperty.Value;

                // process frames, to easily lookup at tracks processing step
                List<(Rectangle Source, uint Duration, Rectangle OriginalFrame)> frames = new List<(Rectangle, uint, Rectangle)>();

                IJEnumerable<JToken> framesTokens = animationToken["frames"].Children();
                foreach (JToken frameToken in framesTokens) {
                    JToken sourceToken = frameToken["source"];
                    Rectangle source = new Rectangle(
                        sourceToken.Value<float>("x"),
                        sourceToken.Value<float>("y"),
                        sourceToken.Value<float>("w"),
                        sourceToken.Value<float>("h")
                    );

                    uint duration = frameToken.Value<uint>("duration");

                    JToken orignalFrameToken = frameToken["original_frame"];
                    Rectangle originalFrame = new Rectangle(
                        orignalFrameToken.Value<float>("x"),
                        orignalFrameToken.Value<float>("y"),
                        orignalFrameToken.Value<float>("w"),
                        orignalFrameToken.Value<float>("h")
                    );

                    frames.Add((source, duration, originalFrame));
                }

                // process tracks
                if (frames.Count == 1) {
                    (Rectangle Source, uint Duration, Rectangle OriginalFrame) frame = frames[0];
                    AtlasSubTexture subTexture = new AtlasSubTexture(
                        texture,
                        frame.Source,
                        new Rectangle(Vector2.Zero, frame.Source.Size)
                    ) {
                        OriginalFrame = frame.OriginalFrame
                    };

                    subTextures.Add(subTextureName, subTexture);
                    continue;
                }

                AtlasAnimation animation = new AtlasAnimation(texture);

                // register all frames to "all" track
                for (uint i = 0U; i < frames.Count; i++) {
                    (Rectangle Source, uint Duration, Rectangle OriginalFrame) frame = frames[(int) i];
                    animation.AddFrame(frame.Source, (int) frame.Duration, frame.OriginalFrame, "all");
                }

                // process other tracks
                IJEnumerable<JToken> tracksTokens = animationToken["tracks"].Children();
                foreach (JToken trackToken in tracksTokens) {
                    string name = trackToken.Value<string>("name");
                    uint from = trackToken.Value<uint>("from");
                    uint to = trackToken.Value<uint>("to");
                    //string direction = trackToken.Value<string>("direction"); //! unused yet

                    for (uint i = from; i <= to; i++) {
                        (Rectangle Source, uint Duration, Rectangle OriginalFrame) frame = frames[(int) i];
                        animation.AddFrame(frame.Source, (int) frame.Duration, frame.OriginalFrame, name);
                    }
                }

                subTextures.Add(subTextureName, animation);
            }

            return true;
        }
    }
}
