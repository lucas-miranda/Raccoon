using System.Xml;

namespace Raccoon.Tiled {
    public class TiledObjectLayer : TiledObjectGroup, ITiledLayer {
        public TiledObjectLayer(XmlElement layerElement) : base(layerElement) {
        }

        public TiledLayerType Type { get { return TiledLayerType.Object; } }
    }
}
