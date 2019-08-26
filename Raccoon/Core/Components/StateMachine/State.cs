using System.Reflection;
using System.Collections.Generic;

namespace Raccoon.Components.StateMachine {
    //public delegate void StateEnterDelegate();
    //public delegate IEnumerator StateUpdateDelegate();
    //public delegate void StateLeaveDelegate();

    public class State<T> {
        public State(T label, MethodInfo onEnter, MethodInfo onUpdate, MethodInfo onLeave) {
            Label = label;
            OnEnter = onEnter;
            OnUpdate = onUpdate;
            OnLeave = onLeave;
        }

        public T Label { get; private set; }
        public MethodInfo OnEnter { get; private set; }
        public MethodInfo OnUpdate { get; private set; }
        public MethodInfo OnLeave { get; private set; }

        public List<Transition<T>> Transitions { get; } = new List<Transition<T>>();
    }
}
