using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Raccoon.Graphics {
    public class Atlas {
        #region Private Static Members

        private static readonly Regex FrameNameRegex = new Regex(@"(.+) (\d+)? (.*)");

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
            foreach (JProperty prop in json.SelectToken("frames").Children<JProperty>()) {
                Match m = FrameNameRegex.Match(prop.Name);
                if (!m.Success) {
                    continue;
                }

                string spriteName = m.Groups[1].Value, tag = m.Groups[3].Length == 0 ? "default" : m.Groups[3].Value;
                int frameId = m.Groups[2].Length == 0 ? 0 : int.Parse(m.Groups[2].Value);
                
                if (!animationsData.ContainsKey(spriteName)) {
                    animationsData.Add(spriteName, new Dictionary<string, List<JObject>>());
                    animationsData[spriteName].Add("Default", new List<JObject>());
                }

                Dictionary<string, List<JObject>> animData = animationsData[spriteName];
                if (!animData.ContainsKey(tag)) {
                    animData.Add(tag, new List<JObject>());
                }

                JObject frameObj = prop.Value.ToObject<JObject>();
                frameObj.Add("frameId", new JValue(frameId));
                animData[tag].Add(frameObj);
                if (tag != "Default") {
                    animData["Default"].Add(frameObj);
                }
            }

            // create AtlasSubTextures
            foreach (KeyValuePair<string, Dictionary<string, List<JObject>>> animationData in animationsData) {
                JObject defaultSpriteData = animationData.Value["Default"][0];
                JToken frameRegion = defaultSpriteData["frame"];
                Rectangle region = new Rectangle(frameRegion.Value<int>("x"), frameRegion.Value<int>("y"), frameRegion.Value<int>("w"), frameRegion.Value<int>("h"));
                if (animationData.Value["Default"].Count == 1) {
                    _subTextures.Add(animationData.Key, new AtlasSubTexture(Texture, region));
                } else {
                    // expand region to fit all frames
                    foreach (JObject frameData in animationData.Value["Default"]) {
                        frameRegion = frameData["frame"];
                        region |= new Rectangle(frameRegion.Value<int>("x"), frameRegion.Value<int>("y"), frameRegion.Value<int>("w"), frameRegion.Value<int>("h"));
                    }

                    JToken frameSize = defaultSpriteData["sourceSize"];
                    AtlasAnimation animation = new AtlasAnimation(new Size(frameSize.Value<int>("w"), frameSize.Value<int>("h")), Texture, region);
                    foreach (KeyValuePair<string, List<JObject>> track in animationData.Value) {
                        foreach (JObject frameData in track.Value) {
                            animation.Add(frameData["frameId"].ToObject<int>(), frameData["duration"].ToObject<int>(), track.Key);
                        }
                    }

                    _subTextures.Add(animationData.Key, animation);
                }
            }
        }

        #endregion Constructors

        #region Public Properties

        public Texture Texture { get; private set; }
        public Size Size { get { return Texture.Size; } }

        public AtlasSubTexture this[string name] {
            get {
                return _subTextures[name];
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
    }
}
