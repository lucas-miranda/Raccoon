using System.Collections.Generic;
using System.Text;

namespace Raccoon.FileSystem {
    public class PathBuf {
        #region Private Members

        private static StringBuilder Builder = new StringBuilder();

        private List<string> _parts = new List<string>();
        private string _result;

        #endregion Private Members

        #region Constructors

        public PathBuf() {
            _parts = new List<string>();
        }

        public PathBuf(string path) : this() {
            if (path == null) {
                throw new System.ArgumentNullException(nameof(path));
            }

            Push(path);
        }

        public PathBuf(string pathA, string pathB) : this() {
            if (pathA == null) {
                throw new System.ArgumentNullException(nameof(pathA));
            }

            if (pathB == null) {
                throw new System.ArgumentNullException(nameof(pathB));
            }

            Push(pathA);
            Push(pathB);
        }

        public PathBuf(string pathA, string pathB, string pathC) : this() {
            if (pathA == null) {
                throw new System.ArgumentNullException(nameof(pathA));
            }

            if (pathB == null) {
                throw new System.ArgumentNullException(nameof(pathB));
            }

            if (pathC == null) {
                throw new System.ArgumentNullException(nameof(pathC));
            }

            Push(pathA);
            Push(pathB);
            Push(pathC);
        }

        public PathBuf(string pathA, string pathB, string pathC, string pathD) : this() {
            if (pathA == null) {
                throw new System.ArgumentNullException(nameof(pathA));
            }

            if (pathB == null) {
                throw new System.ArgumentNullException(nameof(pathB));
            }

            if (pathC == null) {
                throw new System.ArgumentNullException(nameof(pathC));
            }

            if (pathD == null) {
                throw new System.ArgumentNullException(nameof(pathD));
            }

            Push(pathA);
            Push(pathB);
            Push(pathC);
            Push(pathD);
        }

        public PathBuf(PathBuf path) {
            if (path == null) {
                throw new System.ArgumentNullException(nameof(path));
            }

            _parts = new List<string>(path._parts);
            _result = path._result;
        }

        #endregion Constructors

        #region Public Properties

        public int PartsCount { get { return _parts.Count; } }

        public string this[int index] {
            get {
                return _parts[index];
            }
        }

        #endregion Public Properties

        #region Public Methods

        public PathBuf Push(string path) {
            if (path == null) {
                throw new System.ArgumentNullException(nameof(path));
            }

            if (path.Length == 0) {
                return this;
            }

            _result = null;
            foreach (string part in Split(path)) {
                _parts.Add(part);
            }

            return this;
        }

        public PathBuf Push(PathBuf path) {
            if (path == null) {
                throw new System.ArgumentNullException(nameof(path));
            }

            if (path._parts.Count == 0) {
                return this;
            }

            _result = null;
            _parts.AddRange(path._parts);
            return this;
        }

        public string Pop() {
            if (_parts.Count == 0) {
                throw new System.InvalidOperationException("Path is already empty.");
            }

            string lastPart = _parts[_parts.Count - 1];
            _parts.RemoveAt(_parts.Count - 1);
            return lastPart;
        }

        public string Name() {
            return _parts.Count == 0 ? null : _parts[_parts.Count - 1];
        }

        public string ParentName() {
            return !HasParent() ? null : _parts[_parts.Count - 2];
        }

        public PathBuf Parent() {
            if (!HasParent()) {
                return null;
            }

            PathBuf parentPath = new PathBuf(this);
            parentPath.Pop();
            return parentPath;
        }

        public bool HasParent() {
            return _parts.Count > 1;
        }

        public bool ExistsFile() {
            return !IsEmpty() && System.IO.File.Exists(ToString());
        }

        public bool ExistsDirectory() {
            return !IsEmpty() && System.IO.Directory.Exists(ToString());
        }

        public bool IsEmpty() {
            return _parts.Count == 0;
        }

        public bool Contains(PathBuf path) {
            if (path == null) {
                throw new System.ArgumentNullException(nameof(path));
            }

            bool contains = false;

            for (int i = 0; i < PartsCount && i + path.PartsCount <= PartsCount; i++) {
                contains = true;

                for (int j = 0; j < path.PartsCount; j++) {
                    if (path[j] != _parts[i + j]) {
                        contains = false;
                        break;
                    }
                }

                if (contains) {
                    break;
                }
            }

            return contains;
        }

        public bool StartsWith(PathBuf path) {
            if (path == null) {
                throw new System.ArgumentNullException(nameof(path));
            }

            if (path.PartsCount > PartsCount) {
                return false;
            }

            for (int i = 0; i < path.PartsCount; i++) {
                if (_parts[i] != path[i]) {
                    return false;
                }
            }

            return true;
        }

        public bool Remove(PathBuf path) {
            if (path == null) {
                throw new System.ArgumentNullException(nameof(path));
            }

            if (path.PartsCount > PartsCount) {
                return false;
            }

            // find start index
            int startIndex = -1;
            for (int i = 0; i < PartsCount; i++) {
                if (_parts[i] == path[0]) {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex < 0) {
                return false;
            }

            // verify all parts from path matches
            for (int i = 0; i < path.PartsCount; i++) {
                if (_parts[startIndex + i] != path[i]) {
                    return false;
                }
            }

            _parts.RemoveRange(startIndex, path.PartsCount);
            return true;
        }

        public System.IO.FileInfo FileInfo() {
            return new System.IO.FileInfo(ToString());
        }

        public System.IO.DirectoryInfo DirectoryInfo() {
            return new System.IO.DirectoryInfo(ToString());
        }

        public override string ToString() {
            if (_parts.Count == 0) {
                return string.Empty;
            } else if (_result != null) {
                return _result;
            }

            Builder.Clear();

            for (int i = 0; i < _parts.Count - 1; i++) {
                string part = _parts[i];

                try {
                    SafetyCheck(part);
                } catch (System.Exception e) {
                    throw new System.InvalidOperationException($"Path formation failed. (at part '{part}')", e);
                }

                Builder.Append(part);
                Builder.Append(Path.Separator);
            }

            Builder.Append(_parts[_parts.Count - 1]);

            _result = Builder.ToString();
            return _result;
        }

        public override bool Equals(object obj) {
            return obj is PathBuf pathBuf
                && this == pathBuf;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        #endregion Public Methods

        #region Private Methods

        private static IEnumerable<string> Split(string value) {
            if (value == null) {
                yield break;
            }

            string root = System.IO.Path.GetPathRoot(value);
            if (!string.IsNullOrEmpty(root)) {
                yield return root.TrimEnd(Path.Separators);

                if (value.Length <= root.Length) {
                    yield break;
                }

                value = value.Substring(root.Length);
            }

            int start = 0;
            for (int i = 0; i < value.Length; i++) {
                if (Path.IsSeparator(value[i])) {
                    if (i - start <= 0) {
                        start = i + 1;
                        continue;
                    }

                    yield return value.Substring(start, i - start);
                    start = i + 1;
                }
            }

            if (start < value.Length) {
                yield return value.Substring(start, (value.Length - 1) - start + 1);
            }
        }

        private void SafetyCheck(string part) {
            if (!Path.IsValid(part)) {
                throw new System.InvalidOperationException($"Provided part '{part}' has invalid path chars.");
            }
        }

        #endregion Private Methods

        #region Operators

        public static implicit operator PathBuf(string path) {
            return new PathBuf(path);
        }

        public static bool operator ==(PathBuf l, PathBuf r) {
            if (PathBuf.ReferenceEquals(l, null) || PathBuf.ReferenceEquals(r, null)) {
                return PathBuf.ReferenceEquals(l, null) == PathBuf.ReferenceEquals(r, null);
            }

            if (l._parts.Count != r._parts.Count) {
                return false;
            }

            for (int i = 0; i < l._parts.Count; i++) {
                if (l._parts[i] != r._parts[i]) {
                    return false;
                }
            }

            return true;
        }

        public static bool operator !=(PathBuf l, PathBuf r) {
            if (PathBuf.ReferenceEquals(l, null) || PathBuf.ReferenceEquals(r, null)) {
                return PathBuf.ReferenceEquals(l, null) != PathBuf.ReferenceEquals(r, null);
            }

            if (l._parts.Count != r._parts.Count) {
                return true;
            }

            for (int i = 0; i < l._parts.Count; i++) {
                if (l._parts[i] != r._parts[i]) {
                    return true;
                }
            }

            return false;
        }

        public static PathBuf operator +(PathBuf l, PathBuf r) {
            return new PathBuf(l).Push(r);
        }

        public static PathBuf operator +(PathBuf l, string r) {
            return new PathBuf(l).Push(r);
        }

        public static PathBuf operator -(PathBuf l, PathBuf r) {
            PathBuf path = new PathBuf(l);
            path.Remove(r);
            return path;
        }

        public static PathBuf operator -(PathBuf l, string r) {
            PathBuf path = new PathBuf(l);
            path.Remove(r);
            return path;
        }

        #endregion Operators
    }
}
