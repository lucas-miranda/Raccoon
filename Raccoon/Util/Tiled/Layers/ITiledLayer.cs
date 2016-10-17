using System.Collections.Generic;

namespace Raccoon.Tiled {
    public enum TiledLayerType {
        Tile = 0,
        Object,
        Image
    }

    public interface ITiledLayer {
        TiledLayerType Type { get; }
        string Name { get; }
        float Opacity { get; }
        bool Visible { get; }
        Vector2 Offset { get; }
        Dictionary<string, TiledProperty> Properties { get; }
    }
}
