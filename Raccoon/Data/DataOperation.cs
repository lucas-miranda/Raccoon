
namespace Raccoon.Data {
    [System.Flags]
    public enum DataOperation {
        None = 0,
        Load = 1 << 0,
        Save = 1 << 1,
        All = Load | Save
    }
}
