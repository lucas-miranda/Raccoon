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
            return FindIndex(p => p.Subject.Equals(subject)) > -1;
        }

        public bool Contains(object subject) {
            return FindIndex(p => p.Subject.Equals(subject)) > -1;
        }

        public bool Contains(System.Predicate<CollisionInfo<T>> predicate) {
            return FindIndex(predicate) > -1;
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
