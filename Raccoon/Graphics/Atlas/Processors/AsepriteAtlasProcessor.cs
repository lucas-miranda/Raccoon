using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Raccoon.Graphics.AtlasProcessors {
    public class AsepriteAtlasProcessor : IAtlasProcessor {
        public static readonly Regex FrameNameRegex = new Regex(@"(.+?) (\d+)? (.*)"),
                                     FrameNameWithoutTagsRegex = new Regex(@"(.+?) (\d+)\.ase");

        public bool VerifyJsonMeta(JToken metaToken) {
            JToken appToken = metaToken["app"];

            if (appToken == null) {
                return false;
            }

            string app = appToken.Value<string>().ToLowerInvariant();
            return app.Equals("http://www.aseprite.org/");
        }

        public bool ProcessJson(JObject json, in Texture texture, ref Dictionary<string, AtlasSubTexture> subTextures) {
            // all frames organized as sprite/tag/frame
            Dictionary<string, Dictionary<string, List<JObject>>> animationsData = new Dictionary<string, Dictionary<string, List<JObject>>>();

            JToken sizeToken = json.SelectToken("meta.size");

            Rectangle sourceRegion = new Rectangle(
                                         sizeToken.Value<int>("w"),
                                         sizeToken.Value<int>("h")
                                     );

            foreach (JProperty prop in json.SelectToken("frames").Children<JProperty>()) {
                string spriteName, tag;
                int frameId;

                Match m = FrameNameRegex.Match(prop.Name);

                if (m.Success) {
                    spriteName = m.Groups[1].Value;
                    tag = m.Groups[3].Length == 0 ? "none" : m.Groups[3].Value;
                    frameId = m.Groups[2].Length == 0 ? 0 : int.Parse(m.Groups[2].Value);
                } else {
                    m = FrameNameWithoutTagsRegex.Match(prop.Name);

                    if (m.Success) {
                        spriteName = m.Groups[1].Value;
                        tag = "none";
                        frameId = int.Parse(m.Groups[2].Value);
                    } else {
                        continue;
                    }
                }

                if (!animationsData.ContainsKey(spriteName)) {
                    animationsData.Add(spriteName, new Dictionary<string, List<JObject>>());
                    animationsData[spriteName].Add("all", new List<JObject>());
                }

                Dictionary<string, List<JObject>> animData = animationsData[spriteName];
                if (!animData.ContainsKey(tag)) {
                    animData.Add(tag, new List<JObject>());
                }

                JObject frameObj = prop.Value.ToObject<JObject>();
                frameObj.Add("frameId", new JValue(frameId));
                animData[tag].Add(frameObj);

                if (tag != "all") {
                    animData["all"].Add(frameObj);
                }
            }

            // create AtlasSubTextures
            foreach (KeyValuePair<string, Dictionary<string, List<JObject>>> animationData in animationsData) {
                string key = animationData.Key.ToLowerInvariant();

                if (animationData.Value["all"].Count == 1) {
                    JToken frameRegion = animationData.Value["all"][0]["frame"];

                    Rectangle clippingRegion = new Rectangle(
                                                   frameRegion.Value<int>("x"),
                                                   frameRegion.Value<int>("y"),
                                                   frameRegion.Value<int>("w"),
                                                   frameRegion.Value<int>("h")
                                               );

                    subTextures.Add(
                        key,
                        new AtlasSubTexture(texture, sourceRegion, clippingRegion)
                    );
                } else {
                    AtlasAnimation animation = new AtlasAnimation(texture, sourceRegion);

                    foreach (KeyValuePair<string, List<JObject>> track in animationData.Value) {
                        foreach (JObject frameData in track.Value) {
                            JToken frameRegion = frameData["frame"];
                            Rectangle clippingRegion = new Rectangle(
                                                           frameRegion.Value<int>("x"),
                                                           frameRegion.Value<int>("y"),
                                                           frameRegion.Value<int>("w"),
                                                           frameRegion.Value<int>("h")
                                                       );

                            animation.AddFrame(clippingRegion, frameData["duration"].ToObject<int>(), track.Key);
                        }
                    }

                    subTextures.Add(key, animation);
                }
            }

            return true;
        }
    }
}
