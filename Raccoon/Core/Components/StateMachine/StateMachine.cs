using System.Collections;
using System.Collections.Generic;

using Raccoon.Components.StateMachine;

namespace Raccoon.Components {
    public class StateMachine<T> : Component {
        #region Public Members

        public const string OnEnterStateName = "OnEnterState",
                            OnUpdateStateName = "OnUpdateState",
                            OnLeaveStateName = "OnLeaveState";

        #endregion Public Members

        #region Private Members

        private Dictionary<T, State<T>> _states = new Dictionary<T, State<T>>();
        private Dictionary<string, System.IComparable> _triggerValues = new Dictionary<string, System.IComparable>();

        #endregion Private Members

        #region Constructors

        public StateMachine() {
        }

        #endregion Constructors

        #region Public Properties

        public State<T> StartState { get; private set; }
        public State<T> PreviousState { get; private set; }
        public State<T> CurrentState { get; private set; }
        public State<T> NextState { get; private set; } = null;
        public bool KeepTriggerValuesBetweenStates { get; set; } = false;
        public Coroutine CurrentCoroutine { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public override void OnAdded(Entity entity) {
            base.OnAdded(entity);

            // register all enum values (if T is an enum)
            if (typeof(T).IsEnum) {
                foreach (T stateName in System.Enum.GetValues(typeof(T))) {
                    AddState(stateName);
                }
            }
        }

        public override void OnRemoved() {
            base.OnRemoved();
            Stop();
        }

        public override void Update(int delta) {
            if (CurrentState == null) {
                return;
            }

            // default triggers
            SetTrigger("Timer", GetTrigger<uint>("Timer") + (uint) delta);

            if (NextState != null && NextState != CurrentState) {
                UpdateState();
                return;
            }
            
            foreach (Transition<T> transition in CurrentState.Transitions) {
                foreach (KeyValuePair<string, Trigger> triggerEntry in transition.Triggers) {
                    if (_triggerValues.TryGetValue(triggerEntry.Key, out System.IComparable triggerValue) && triggerEntry.Value.Comparison(triggerValue)) {
                        NextState = _states[transition.TargetStateName];
                        UpdateState();
                        return;
                    }
                }
            }
        }

        public override void Render() {
        }

        public override void DebugRender() {
            Debug.DrawString(Entity.Transform.Position + new Vector2(Entity.Graphic.Width / 1.9f, 0), $"State:\n Previous: {(PreviousState == null ? "-" : PreviousState.Label.ToString())}\n Current: {(CurrentState == null ? "-" : CurrentState.Label.ToString())}\n Next: {(NextState == null ? "-" : NextState.Label.ToString())}");
        }

        public void Start(T label) {
            if (CurrentState != null) {
                return;
            }

            PreviousState = CurrentState = StartState = _states[label];
            CurrentState.OnEnter();
            CurrentCoroutine = Coroutines.Instance.Start(CurrentState.OnUpdate);
        }

        public void Stop() {
            NextState = PreviousState = CurrentState = StartState = null;
            if (CurrentCoroutine != null) {
                CurrentCoroutine.Stop();
            }

            ClearTriggers();
        }

        public void ChangeState(T label) {
            State<T> nextState = _states[label];

            if (nextState == CurrentState) {
                return;
            }

            NextState = nextState;
            CurrentCoroutine.Stop(); // avoid running current state update 
        }

        public void AddState(T label) {
            if (_states.ContainsKey(label)) {
                throw new System.ArgumentException($"StateMachine already contains a State '{label}'");
            }

            // searches for OnEnterState, OnUpdateState and OnLeaveState on Entity's methods
            System.Reflection.MethodInfo onEnterStateMethodInfo = null, onUpdateStateMethodInfo = null, onLeaveStateMethodInfo = null;
            string onEnterStateMethodName = OnEnterStateName + label.ToString(), 
                   onUpdateStateMethodName = OnUpdateStateName + label.ToString(), 
                   onLeaveStateMethodName = OnLeaveStateName + label.ToString();

            foreach (System.Reflection.MethodInfo methodInfo in Entity.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)) {
                if (methodInfo.Name == onEnterStateMethodName) {
                    onEnterStateMethodInfo = methodInfo;
                } else if (methodInfo.Name == onUpdateStateMethodName) {
                    onUpdateStateMethodInfo = methodInfo;
                } else if (methodInfo.Name == onLeaveStateMethodName) {
                    onLeaveStateMethodInfo = methodInfo;
                }
            }

            if (onEnterStateMethodInfo == null) {
                throw new System.InvalidOperationException($"'{Entity.GetType().Name}' doesn't contains a definition of onEnter method '{onEnterStateMethodName}'.");
            } else if (onUpdateStateMethodInfo == null) {
                throw new System.InvalidOperationException($"'{Entity.GetType().Name}' doesn't contains a definition of onUpdate method '{onUpdateStateMethodName}'.");
            } else if (onLeaveStateMethodInfo == null) {
                throw new System.InvalidOperationException($"'{Entity.GetType().Name}' doesn't contains a definition of onLeave method '{onLeaveStateMethodName}'.");
            }

            State<T> state = new State<T>(
                                label,
                                (System.Action) onEnterStateMethodInfo.CreateDelegate(typeof(System.Action), Entity),
                                (System.Func<IEnumerator>) onUpdateStateMethodInfo.CreateDelegate(typeof(System.Func<IEnumerator>), Entity),
                                (System.Action) onLeaveStateMethodInfo.CreateDelegate(typeof(System.Action), Entity)
                              );

            _states.Add(label, state);
        }

        public void AddStates(params T[] labels) {
            foreach (T label in labels) {
                AddState(label);
            }
        }

        public Transition<T> AddTransition(T fromStateLabel, T toStateLabel) {
            if (fromStateLabel.Equals(toStateLabel)) {
                throw new System.InvalidOperationException($"Can't add a transition from a state to itself.");
            }

            if (!_states.TryGetValue(fromStateLabel, out State<T> fromState)) {
                throw new System.InvalidOperationException($"StateMachine doesn't contains a State '{fromStateLabel}");
            }

            if (!_states.TryGetValue(toStateLabel, out State<T> toState)) {
                throw new System.InvalidOperationException($"StateMachine doesn't contains a State '{toStateLabel}");
            }

            if (fromState.Transitions.Find(p => p.TargetStateName.Equals(toStateLabel)) != null) {
                throw new System.InvalidOperationException($"StateMachine already have a transition from '{fromStateLabel}' to '{toStateLabel}'");
            }

            Transition<T> transition = new Transition<T>(toStateLabel);
            fromState.Transitions.Add(transition);
            return transition;
        }

        public void SetTrigger<K>(string label, K value) where K : System.IComparable {
            _triggerValues[label] = value;
        }

        public K GetTrigger<K>(string label) where K : System.IComparable {
            if (_triggerValues.TryGetValue(label, out System.IComparable value)) {
                return (K) value;
            }

            return default;
        }

        public void RemoveTrigger(string label) {
            _triggerValues.Remove(label);
        }

        public void ClearTriggers() {
            _triggerValues.Clear();
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateState() {
            CurrentState.OnLeave();
            CurrentCoroutine.Stop();

            if (!KeepTriggerValuesBetweenStates) {
                ClearTriggers();
            }

            PreviousState = CurrentState;
            CurrentState = NextState;
            NextState = null;

            if (CurrentState == null) {
                return;
            }

            CurrentState.OnEnter();
            CurrentCoroutine = Coroutines.Instance.Start(CurrentState.OnUpdate);
        }

        #endregion Private Methods
    }
}
