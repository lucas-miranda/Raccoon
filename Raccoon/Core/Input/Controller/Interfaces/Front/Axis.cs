using System.Text;
using System.Collections.Generic;

using Raccoon.Util;

namespace Raccoon.Input {
    public class Axis : InputInterface, IInputAxis {
        #region Private Members

        private HashSet<IBackInterfaceAxis> _sources = new HashSet<IBackInterfaceAxis>();

        #endregion Private Members

        #region Constructors

        public Axis() : base() {
        }

        #endregion Constructors

        #region Public Properties

        public float X { get; private set; }
        public float Y { get; private set; }
        public Vector2 Value { get { return new Vector2(X, Y); } }

        #endregion Public Properties

        public string SourcesInfo() {
            StringBuilder sb = new StringBuilder();

            foreach (IBackInterfaceAxis source in _sources) {
                sb.AppendLine($"{source.ToString()}: {source.Value}");
            }

            return sb.ToString();
        }

        #region Internal Methods

        internal override void Update(int delta) {
            X = Y = 0f;
            foreach (IBackInterfaceAxis source in _sources) {
                X = Math.Clamp(X + source.X, -1f, 1f);
                Y = Math.Clamp(Y + source.Y, -1f, 1f);
            }
        }

        internal void RegisterSource(IBackInterfaceAxis source) {
            _sources.Add(source);
        }

        internal void DeregisterSource(IBackInterfaceAxis source) {
            _sources.Remove(source);
        }

        #endregion Internal Methods
    }
}
