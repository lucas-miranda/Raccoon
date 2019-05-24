using System.Collections.Generic;

namespace Raccoon.Components.StateMachine {
    public class Transition<T> {
        public Transition(T targetStateName) {
            TargetStateName = targetStateName;
        }

        public T TargetStateName { get; private set; }
        public Dictionary<string, Trigger> Triggers { get; } = new Dictionary<string, Trigger>();

        public Transition<T> AddTrigger(string name, object value, Comparison comparisonType) {
            Triggers.Add(name, new Trigger(value, comparisonType));
            return this;
        }
    }
}
