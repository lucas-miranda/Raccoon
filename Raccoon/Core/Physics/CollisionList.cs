using System.Text;
using System.Collections.Generic;

namespace Raccoon {
    public class CollisionList<T> : List<CollisionInfo<T>>  {
        public void Add(T subject, params Contact[] contacts) {
            Add(new CollisionInfo<T>(subject, contacts));
        }

        public void Add(T subject, ContactList contacts) {
            Add(new CollisionInfo<T>(subject, contacts));
        }

        public bool Contains(T subject) {
            foreach (CollisionInfo<T> entry in this) {
                if (entry.Subject.Equals(subject)) {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(object subject) {
            foreach (CollisionInfo<T> entry in this) {
                if (entry.Subject.Equals(subject)) {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(System.Predicate<CollisionInfo<T>> predicate) {
            return FindIndex(predicate) > -1;
        }

        public bool Collides(bool touchIsCollision = false) {
            foreach (CollisionInfo<T> collisionInfo in this) {
                if (collisionInfo.Contacts.Collides(touchIsCollision)) {
                    return true;
                }
            }

            return false;
        }

        public bool CollidesButExcept(T exceptionSubject, bool touchIsCollision) {
            if (touchIsCollision) {
                foreach (CollisionInfo<T> collisionInfo in this) {
                    if (collisionInfo.Subject.Equals(exceptionSubject)) {
                        continue;
                    }

                    foreach (Contact c in collisionInfo.Contacts) {
                        if (c.PenetrationDepth >= 0f) {
                            return true;
                        }
                    }
                }
            } else {
                foreach (CollisionInfo<T> collisionInfo in this) {
                    if (collisionInfo.Subject.Equals(exceptionSubject)) {
                        continue;
                    }

                    foreach (Contact c in collisionInfo.Contacts) {
                        if (c.PenetrationDepth > 0.01f) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public IEnumerable<T> Subjects() {
            foreach (CollisionInfo<T> collisionInfo in this) {
                yield return collisionInfo.Subject;
            }
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (CollisionInfo<T> collisionInfo in this) {
                stringBuilder.AppendLine(collisionInfo.ToString());
            }

            return $"[\n{stringBuilder}]";
        }
    }
}
