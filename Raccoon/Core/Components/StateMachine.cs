using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Components {
    public class StateMachine<T> : Component {
        public const string OnEnterStateName = "OnEnterState",
                            OnUpdateStateName = "OnUpdateState",
                            OnLeaveStateName = "OnLeaveState";

        //

        private Dictionary<T, State> _states = new Dictionary<T, State>();
        private int _onUpdateCoroutineId = -1;

        public StateMachine() {
        }

        public State StartState { get; private set; }
        public State CurrentState { get; private set; }

        protected State NextState { get; private set; } = null;

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

            if (NextState == null) {
                foreach (Transition transition in CurrentState.Transitions) {
                    transition.Update(delta);
                    if (transition.IsReady) {
                        NextState = _states[transition.TargetStateName];
                        break;
                    }
                }
            }

            if (NextState != null) {
                CurrentState.OnLeave();
                Coroutine.Instance.Stop(_onUpdateCoroutineId);

                CurrentState = NextState;
                NextState = null;
                CurrentState.ResetTransitions();
                CurrentState.OnEnter();
                _onUpdateCoroutineId = Coroutine.Instance.Start(CurrentState.OnUpdate());
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
            _onUpdateCoroutineId = Coroutine.Instance.Start(CurrentState.OnUpdate());
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

            State state = new State(
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

        public void AddTransition(T fromStateName, T toStateName, uint mili, System.Func<bool> prerequisite = null) {
            if (fromStateName.Equals(toStateName)) {
                throw new System.InvalidOperationException($"Can't add a transition from a state to itself.");
            }

            if (!_states.TryGetValue(fromStateName, out State fromState)) {
                throw new System.InvalidOperationException($"StateMachine doesn't contains a State '{fromStateName}");
            }

            if (!_states.TryGetValue(toStateName, out State toState)) {
                throw new System.InvalidOperationException($"StateMachine doesn't contains a State '{toStateName}");
            }

            if (fromState.Transitions.Find(p => p.TargetStateName.Equals(toStateName)) != null) {
                throw new System.InvalidOperationException($"StateMachine already have a transition from '{fromStateName}' to '{toStateName}'");
            }

            fromState.Transitions.Add(new Transition(toStateName, mili, prerequisite));
        }

        public void AddTransition(T fromStateName, T toStateName, int mili, System.Func<bool> prerequisite = null) {
            AddTransition(fromStateName, toStateName, (uint) mili, prerequisite);
        }

        public void AddTransition(T fromStateName, T toStateName, float seconds, System.Func<bool> prerequisite = null) {
            AddTransition(fromStateName, toStateName, (uint) System.Math.Round(seconds * Util.Time.SecToMili), prerequisite);
        }

        #region Class State

        public class State {
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

            public List<Transition> Transitions { get; } = new List<Transition>();

            public void ResetTransitions() {
                foreach (Transition transition in Transitions) {
                    transition.Reset();
                }
            }
        }

        #endregion Class State

        #region Class Transition
        
        public class Transition {
            public Transition(T targetStateName, uint interval, System.Func<bool> prerequisite) {
                Prerequisite = prerequisite;
                TargetStateName = targetStateName;
                Interval = interval;
            }

            public T TargetStateName { get; private set; }
            public uint Interval { get; set; }
            public uint Timer { get; private set; }
            public System.Func<bool> Prerequisite { get; private set; }
            public bool IsReady { get { return Timer >= Interval && (Prerequisite == null || Prerequisite.Invoke()); } }

            public void Update(int delta) {
                Timer += (uint) delta;
            }

            public void Reset() {
                Timer = 0;
            }
        }

        #endregion Class Transition
    }
}
