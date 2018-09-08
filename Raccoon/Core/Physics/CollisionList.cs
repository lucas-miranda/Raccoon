﻿using System.Collections.Generic;

namespace Raccoon {
    public class CollisionList<T> : List<CollisionInfo<T>>  {
        public void Add(T subject, params Contact[] contacts) {
            Add(new CollisionInfo<T>(subject, contacts));
        }

        public bool Contains(T subject) {
            return FindIndex(p => p.Equals(subject)) > -1;
        }

        public bool Contains(object subject) {
            return FindIndex(p => p.Equals(subject)) > -1;
        }

        public bool Contains(System.Predicate<CollisionInfo<T>> predicate) {
            return FindIndex(predicate) > -1;
        }

        public IEnumerable<T> Subjects() {
            foreach (CollisionInfo<T> collisionInfo in this) {
                yield return collisionInfo.Subject;
            }
        }
    }
}