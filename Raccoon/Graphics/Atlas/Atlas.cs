using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using System.IO;
using System.Text.RegularExpressions;

namespace Raccoon.Graphics {
    public class Atlas {
        #region Private Static Members

        private static readonly Regex FrameNameRegex = new Regex(@"(.+?) (\d+)? (.*)");

        private static Dictionary<string, Atlas> _bank = new Dictionary<string, Atlas>();

        #endregion Private Static Members

        #region Private Members

        private Dictionary<string, AtlasSubTexture> _subTextures;

        #endregion Private Members

        #region Constructors

        public Atlas(string imageFilename, string jsonFilename) {
            _subTextures = new Dictionary<string, AtlasSubTexture>();
            Texture = new Texture(imageFilename);


            // all frames organized as sprite/tag/frame
            Dictionary<string, Dictionary<string, List<JObject>>> animationsData = new Dictionary<string, Dictionary<string, List<JObject>>>();
            JObject json = JObject.Parse(File.ReadAllText(jsonFilename));

            JToken sizeToken = json.SelectToken("meta.size");

            Rectangle sourceRegion = new Rectangle(
                                         sizeToken.Value<int>("w"),
                                         sizeToken.Value<int>("h")
                                     );

            foreach (JProperty prop in json.SelectToken("frames").Children<JProperty>()) {
                Match m = FrameNameRegex.Match(prop.Name);
                if (!m.Success) {
                    continue;
                }

                string spriteName = m.Groups[1].Value, 
                       tag = m.Groups[3].Length == 0 ? "none" : m.Groups[3].Value;

                int frameId = m.Groups[2].Length == 0 ? 0 : int.Parse(m.Groups[2].Value);
                
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

                    _subTextures.Add(key, new AtlasSubTexture(Texture, sourceRegion) { ClippingRegion = clippingRegion });
                } else {
                    AtlasAnimation animation = new AtlasAnimation(Texture, sourceRegion);

                    foreach (KeyValuePair<string, List<JObject>> track in animationData.Value) {
                        foreach (JObject frameData in track.Value) {
                            JToken frameRegion = frameData["frame"];
                            Rectangle clippingRegion = new Rectangle(
                                                           frameRegion.Value<int>("x"), 
                                                           frameRegion.Value<int>("y"), 
                                                           frameRegion.Value<int>("w"), 
                                                           frameRegion.Value<int>("h")
                                                       );

                            animation.Add(clippingRegion, frameData["duration"].ToObject<int>(), track.Key);
                        }
                    }

                    _subTextures.Add(key, animation);
                }
            }
        }

        #endregion Constructors

        #region Public Properties

        public Texture Texture { get; private set; }
        public Size Size { get { return Texture.Size; } }

        public AtlasSubTexture this[string name] {
            get {
                return _subTextures[name.ToLowerInvariant()];
            }
        }

        public AtlasSubTexture this[int x, int y, int width, int height] {
            get {
                return new AtlasSubTexture(Texture, new Rectangle(x, y, width, height));
            }
        }

        public AtlasSubTexture this[Rectangle region] {
            get {
                return new AtlasSubTexture(Texture, region);
            }
        }

        #endregion Public Properties

        #region Public Static Methods

        public static void Register(string name, string imageFilename, string jsonFilename) {
            _bank.Add(name, new Atlas(imageFilename, jsonFilename));
        }

        public static Atlas Retrieve(string name) {
            return _bank[name];
        }

        public static AtlasSubTexture Retrieve(string name, string subName) {
            return _bank[name][subName.ToLowerInvariant()];
        }

        public static AtlasAnimation RetrieveAnimation(string name, string subName) {
            return _bank[name][subName.ToLowerInvariant()] as AtlasAnimation;
        }

        #endregion Public Static Methods
    }
}
