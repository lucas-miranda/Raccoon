using System.Text;

namespace Raccoon.Data.Parsers {
    public class SaverState {
        private SaveSettings _settings;

        public SaverState() {
        }

        public StringBuilder StringBuilder { get; } = new StringBuilder();
        public int Level { get; private set; }

        public SaveSettings Settings {
            get {
                return _settings;
            }

            set {
                if (value.SpacePerLevel <= 0) {
                    throw new System.ArgumentException($"Space per level must be a positive non-zero number, but '{value.SpacePerLevel}' was supplied.");
                }

                _settings = value;
            }
        }

        public void PushLevel() {
            Level += 1;
        }

        public void PopLevel() {
            Level -= 1;
        }

        public void Reset() {
            Level = 0;
            StringBuilder.Clear();
            _settings = SaveSettings.Default;
        }
    }
}
