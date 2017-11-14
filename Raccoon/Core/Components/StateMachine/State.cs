using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Components.StateMachine {
    public class State<T> {
        public State(T name, System.Action onEnter, System.Func<IEnumerator> onUpdate, System.Action onLeave) {
            Name = name;
            OnEnter = onEnter;
            OnUpdate = onUpdate;
            OnLeave = onLeave;
        }

        public T Name { get; private set; }
        public System.Action OnEnter { get; private set; }
        public System.Func<IEnumerator> OnUpdate { get; private set; }
        public System.Action OnLeave { get; private set; }

        public List<Transition<T>> Transitions { get; } = new List<Transition<T>>();
    }
}
