using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public class ContactList : IEnumerable<Contact>, IEnumerable {
        private readonly Contact[] _entries;

        public ContactList(int capacity) {
            _entries = new Contact[capacity];
        }

        public ContactList(IList<Contact> contacts) {
            if (contacts == null || contacts.Count == 0) {
                _entries = new Contact[0];
            } else {
                _entries = new Contact[contacts.Count];
                contacts.CopyTo(_entries, 0);
            }
        }

        public ContactList(params Contact[] contacts) : this((IList<Contact>) contacts) {
        }

        public ContactList() : this(0) {
        }

        public int Count { get { return _entries.Length; } }

        public Contact this[int index] { get { return _entries[index]; } }

        public bool Contains(System.Predicate<Contact> predicate) {
            return System.Array.FindIndex(_entries, predicate) > -1;
        }

        public int FindIndex(System.Predicate<Contact> predicate) {
            return System.Array.FindIndex(_entries, predicate);
        }

        public bool Collides(bool touchIsCollision) {
            if (touchIsCollision) {
                foreach (Contact c in _entries) {
                    if (c.PenetrationDepth >= 0f) {
                        return true;
                    }
                }
            } else {
                foreach (Contact c in _entries) {
                    if (c.PenetrationDepth > 0.1f) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Collides(Vector2 collisionDirection, float equalityFactor, bool touchIsCollision) {
            if (touchIsCollision) {
                foreach (Contact c in _entries) {
                    if (c.PenetrationDepth >= 0f && collisionDirection.Projection(c.Normal) >= equalityFactor) {
                        return true;
                    }
                }
            } else {
                foreach (Contact c in _entries) {
                    if (c.PenetrationDepth > 0.1f && collisionDirection.Projection(c.Normal) >= equalityFactor) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TouchesSurfaceOnly() {
            if (_entries.Length == 0) {
                return false;
            }

            foreach (Contact c in _entries) {
                if (c.PenetrationDepth > 0f) {
                    return false;
                }
            }

            return true;
        }

        public void CopyTo(Contact[] contacts, int index) {
            _entries.CopyTo(contacts, index);
        }

        public IEnumerator<Contact> GetEnumerator() {
            foreach (Contact c in _entries) {
                yield return c;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _entries.GetEnumerator();
        }

        public override string ToString() {
            return $"[{string.Join(", ", _entries)}]";
        }
    }
}
