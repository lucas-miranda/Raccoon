
namespace Raccoon.Data {
    public struct SaveSettings {
        public static readonly SaveSettings Default = new SaveSettings(4, true);

        public int SpacePerLevel;
        public bool UseExplicitTypes;

        public SaveSettings(int spacePerLevel, bool useExplicitTypes) {
            SpacePerLevel = spacePerLevel;
            UseExplicitTypes = useExplicitTypes;
        }
    }
}
