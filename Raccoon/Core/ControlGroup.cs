using System.Collections.Generic;

using Raccoon.Util.Collections;

namespace Raccoon {
    public class ControlGroup {
        private HashSet<IPausable> _registry = new HashSet<IPausable>();
        private Locker<IPausable> _entries = new Locker<IPausable>();

        public ControlGroup() {
        }

        public bool IsPaused { get; private set; }

        public void Register(IPausable pausable) {
            if (pausable == null) {
                throw new System.ArgumentNullException(nameof(pausable));
            }

            if (pausable.ControlGroup != null) {
                if (pausable.ControlGroup == this && _registry.Contains(pausable)) {
                    return;
                } else {
                    pausable.ControlGroup.Unregister(pausable);
                }
            }

            if (!_registry.Add(pausable)) {
                return;
            }

            _entries.Add(pausable);
            pausable.ControlGroupRegistered(this);
        }

        public void Unregister(IPausable pausable) {
            if (pausable == null) {
                throw new System.ArgumentNullException(nameof(pausable));
            }

            if (!_registry.Remove(pausable)) {
                return;
            }

            _entries.Remove(pausable);
            pausable.ControlGroupUnregistered();
        }

        public void Pause() {
            if (IsPaused) {
                return;
            }

            IsPaused = true;

            _entries.Lock();
            foreach (IPausable pausable in _entries) {
                pausable.Paused();
            }
            _entries.Unlock();
        }

        public void Resume() {
            if (!IsPaused) {
                return;
            }

            IsPaused = false;

            _entries.Lock();
            foreach (IPausable pausable in _entries) {
                pausable.Resumed();
            }
            _entries.Unlock();
        }

        public void Clear() {
            _entries.Clear();
            _registry.Clear();
        }
    }
}
