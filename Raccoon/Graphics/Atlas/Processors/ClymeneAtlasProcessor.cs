using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Raccoon.Graphics.AtlasProcessors {
    public class ClymeneAtlasProcessor : IAtlasProcessor {
        #region Public Members

        public const string InnerTracksSeparator = "->";

        #endregion Public Members

        #region Constructors

        public ClymeneAtlasProcessor() {
        }

        #endregion Constructors

        #region Public Methods

        public bool VerifyJsonMeta(JToken metaToken) {
            JToken appToken = metaToken["app"];

            if (appToken == null) {
                return false;
            }

            return appToken.Value<string>()
                           .ToLowerInvariant()
                           .Equals("https://github.com/lucas-miranda/clymene");
        }

        public bool ProcessJson(JObject json, Texture texture, ref Dictionary<string, AtlasSubTexture> subTextures) {
            List<FrameData> frames = new List<FrameData>();

            foreach (JProperty graphicProperty
                in json["graphics"].Children<JProperty>()
            ) {
                string subTextureName = graphicProperty.Name.ToLowerInvariant();

                // -> frames
                // to easily lookup at tracks processing step

                frames.Clear();
                ProcessFrames(graphicProperty.Value["frames"], ref frames);

                if (frames.Count == 1) {
                    FrameData frame = frames[0];
                    AtlasSubTexture subTexture = new AtlasSubTexture(
                        texture,
                        frame.AtlasRegion,
                        new Rectangle(Vector2.Zero, frame.AtlasRegion.Size)
                    ) {
                        OriginalFrame = frame.SourceRegion,
                    };

                    subTextures.Add(subTextureName, subTexture);
                    continue;
                }

                AtlasAnimation animation = new AtlasAnimation(texture);

                // register every frame to "all" track
                foreach (FrameData frameData in frames) {
                    RegisterFrame(animation, "all", frameData);
                }

                // -> tracks

                ProcessTracks(graphicProperty.Value["tracks"], animation, ref frames);

                //

                subTextures.Add(subTextureName, animation);
            }

            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private List<FrameData> ProcessFrames(JToken framesToken, ref List<FrameData> frames) {
            foreach (JToken frameToken in framesToken.Children()) {
                if (!frameToken.HasValues) {
                    frames.Add(null);
                    continue;
                }

                JToken atlasToken = frameToken["atlas"];

                Rectangle atlasRegion = new Rectangle(
                    atlasToken.Value<uint>("x"),
                    atlasToken.Value<uint>("y"),
                    atlasToken.Value<uint>("width"),
                    atlasToken.Value<uint>("height")
                );

                JToken sourceToken = frameToken["source"];
                Rectangle sourceRegion = new Rectangle(
                    -sourceToken.Value<uint>("x"),
                    -sourceToken.Value<uint>("y"),
                    sourceToken.Value<uint>("width"),
                    sourceToken.Value<uint>("height")
                );

                uint? duration;
                JToken durationToken = frameToken["duration"];

                if (durationToken != null) {
                    duration = durationToken.Value<uint>();
                } else {
                    duration = null;
                }

                frames.Add(new FrameData {
                    AtlasRegion = atlasRegion,
                    SourceRegion = sourceRegion,
                    Duration = duration,
                });
            }

            return frames;
        }

        private void ProcessTracks(JToken tracksToken, AtlasAnimation atlasAnimation, string prependLabel, ref List<FrameData> frames) {
            if (tracksToken == null) {
                return;
            }

            // process tracks data
            foreach (JToken trackToken in tracksToken.Children<JToken>()) {
                string label = prependLabel + trackToken.Value<string>("label");

                // -> indices

                foreach (JToken indexGroupToken in trackToken["indices"].Children()) {
                    if (indexGroupToken.HasValues) {
                        // index range

                        uint from = indexGroupToken.Value<uint>("from"),
                             to = indexGroupToken.Value<uint>("to");

                        RegisterFrameRange(atlasAnimation, label, from, to, ref frames);
                    } else {
                        // index value

                        uint value = indexGroupToken.Value<uint>();
                        RegisterFrame(atlasAnimation, label, value, ref frames);
                    }
                }

                // -> inner tracks

                JToken innerTracksToken = trackToken["tracks"];

                if (innerTracksToken != null) {
                    ProcessTracks(
                        innerTracksToken,
                        atlasAnimation,
                        label + InnerTracksSeparator,
                        ref frames
                    );
                }
            }
        }

        private void ProcessTracks(JToken tracksToken, AtlasAnimation atlasAnimation, ref List<FrameData> frames) {
            ProcessTracks(tracksToken, atlasAnimation, string.Empty, ref frames);
        }

        private void RegisterFrame(AtlasAnimation atlasAnimation, string targetTag, FrameData frameData) {
            if (frameData == null) {
                atlasAnimation.AddFrame(
                    Rectangle.Empty,
                    (int) 100,
                    Rectangle.Empty,
                    targetTag
                );

                return;
            }

            atlasAnimation.AddFrame(
                frameData.AtlasRegion,
                (int) frameData.Duration,
                frameData.SourceRegion,
                targetTag
            );
        }

        private void RegisterFrame(AtlasAnimation atlasAnimation, string targetTag, uint frameIndex, ref List<FrameData> frames) {
            FrameData frameData = frames[(int) frameIndex];
            RegisterFrame(atlasAnimation, targetTag, frameData);
        }

        private void RegisterFrameRange(AtlasAnimation atlasAnimation, string targetTag, uint fromFrameIndex, uint toFrameIndex, ref List<FrameData> frames) {
            for (uint i = fromFrameIndex; i <= toFrameIndex; i++) {
                RegisterFrame(atlasAnimation, targetTag, i, ref frames);
            }
        }

        #endregion Private Methods

        #region FrameData Class

        private class FrameData {
            public FrameData() {
            }

            public Rectangle AtlasRegion { get; set; }
            public Rectangle SourceRegion { get; set; }
            public uint? Duration { get; set; }
        }

        #endregion FrameData Class
    }
}
