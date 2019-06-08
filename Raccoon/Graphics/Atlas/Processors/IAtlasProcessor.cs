using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Raccoon.Graphics.AtlasProcessors {
    public interface IAtlasProcessor {
        bool VerifyJsonMeta(JToken metaToken);
        bool ProcessJson(JObject json, in Texture texture, ref Dictionary<string, AtlasSubTexture> subTextures);
    }
}
