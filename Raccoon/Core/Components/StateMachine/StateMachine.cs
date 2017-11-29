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
        private Coroutines.Coroutine _onUpdateCoroutine;

        #endregion Private Members

        #region Constructors

        public StateMachine() {
        }

        #endregion Constructors

        #region Public Properties

        public State<T> StartState { get; private set; }
        public State<T> CurrentState { get; private set; }
        public bool KeepTriggerValuesBetweenStates { get; set; } = false;

        #endregion Public Properties

        #region Protected Properties

        protected State<T> NextState { get; private set; } = null;

        #endregion Protected Properties

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

        public override void Update(int delta) {
            if (CurrentState == null) {
                return;
            }

            // default triggers
            SetTrigger("Timer", GetTrigger<uint>("Timer") + (uint) delta);

            if (NextState != CurrentState && NextState != null) {
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
        }

        public void Start(T name) {
            if (StartState != null) {
                return;
            }

            CurrentState = StartState = _states[name];
            CurrentState.OnEnter();
            _onUpdateCoroutine = Coroutines.Instance.Start(CurrentState.OnUpdate);
        }

        public void ChangeState(T name) {
            NextState = _states[name];
        }

        public void AddState(T name) {
            if (_states.ContainsKey(name)) {
                throw new System.ArgumentException($"StateMachine already contains a State '{name}'");
            }

            // searches for OnEnterState, OnUpdateState and OnLeaveState on Entity's methods
            System.Reflection.MethodInfo onEnterStateMethodInfo = null, onUpdateStateMethodInfo = null, onLeaveStateMethodInfo = null;
            string onEnterStateMethodName = OnEnterStateName + name.ToString(), 
                   onUpdateStateMethodName = OnUpdateStateName + name.ToString(), 
                   onLeaveStateMethodName = OnLeaveStateName + name.ToString();

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
                                name,
                                (System.Action) onEnterStateMethodInfo.CreateDelegate(typeof(System.Action), Entity),
                                (System.Func<IEnumerator>) onUpdateStateMethodInfo.CreateDelegate(typeof(System.Func<IEnumerator>), Entity),
                                (System.Action) onLeaveStateMethodInfo.CreateDelegate(typeof(System.Action), Entity)
                              );

            _states.Add(name, state);
        }

        public void AddStates(params T[] names) {
            foreach (T name in names) {
                AddState(name);
            }
        }

        public Transition<T> AddTransition(T fromStateName, T toStateName) {
            if (fromStateName.Equals(toStateName)) {
                throw new System.InvalidOperationException($"Can't add a transition from a state to itself.");
            }

            if (!_states.TryGetValue(fromStateName, out State<T> fromState)) {
                throw new System.InvalidOperationException($"StateMachine doesn't contains a State '{fromStateName}");
            }

            if (!_states.TryGetValue(toStateName, out State<T> toState)) {
                throw new System.InvalidOperationException($"StateMachine doesn't contains a State '{toStateName}");
            }

            if (fromState.Transitions.Find(p => p.TargetStateName.Equals(toStateName)) != null) {
                throw new System.InvalidOperationException($"StateMachine already have a transition from '{fromStateName}' to '{toStateName}'");
            }

            Transition<T> transition = new Transition<T>(toStateName);
            fromState.Transitions.Add(transition);
            return transition;
        }

        public void SetTrigger<K>(string name, K value) where K : System.IComparable {
            _triggerValues[name] = value;
        }

        public K GetTrigger<K>(string name) where K : System.IComparable {
            if (_triggerValues.TryGetValue(name, out System.IComparable value)) {
                return (K) value;
            }

            return default(K);
        }

        public void RemoveTrigger(string name) {
            _triggerValues.Remove(name);
        }

        public void ClearTriggers() {
            _triggerValues.Clear();
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateState() {
            CurrentState.OnLeave();
            _onUpdateCoroutine.Stop();

            if (!KeepTriggerValuesBetweenStates) {
                ClearTriggers();
            }

            CurrentState = NextState;
            NextState = null;
            CurrentState.OnEnter();
            _onUpdateCoroutine = Coroutines.Instance.Start(CurrentState.OnUpdate);
        }

        #endregion Private Methods
    }
}
