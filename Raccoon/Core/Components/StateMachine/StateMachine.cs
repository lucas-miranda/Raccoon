using System.Reflection;
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
        public Coroutine CurrentCoroutine { get; private set; }
        public bool IsRunning { get; private set; }
        public bool HasStarted { get; private set; }

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

        public override void OnSceneAdded() {
            base.OnSceneAdded();
            if (HasStarted) {
                Resume();
            }
        }

        public override void OnSceneRemoved(bool wipe) {
            base.OnSceneRemoved(wipe);
            if (HasStarted) {
                Pause();
            }
        }

        public override void Update(int delta) {
            if (!IsRunning) {
                return;
            }

            if (NextState != null && NextState != CurrentState) {
                UpdateState();
            }
        }

        public override void Render() {
        }

        public override void DebugRender() {
            Debug.DrawString(Entity.Transform.Position + new Vector2(Entity.Graphic.Width / 1.9f, 0), $"State ({(IsRunning ? "R" : (CurrentState == null ? "S" : "P"))}):\n Previous: {(PreviousState == null ? "-" : PreviousState.Label.ToString())}\n Current: {(CurrentState == null ? "-" : CurrentState.Label.ToString())}\n Next: {(NextState == null ? "-" : NextState.Label.ToString())}");
        }

        public void Start(T label) {
            if (HasStarted) {
                return;
            }

            PreviousState = CurrentState = StartState = _states[label];
            ProcessStateEnter(CurrentState.OnEnter);
            CurrentCoroutine = Coroutines.Instance.Start(ProcessStateUpdate(CurrentState.OnUpdate));
            HasStarted = IsRunning = true;
        }

        public void Pause() {
            if (!HasStarted) {
                throw new System.InvalidOperationException("Can't pause an uninitiated State Machine, call Start() first.");
            }

            if (!IsRunning) {
                return;
            }

            if (CurrentCoroutine != null) {
                CurrentCoroutine.Pause();
                Coroutines.Instance.Remove(CurrentCoroutine);
            }

            IsRunning = false;
        }

        public void Resume() {
            if (!HasStarted) {
                throw new System.InvalidOperationException("Can't resume an uninitiated State Machine, call Start() first.");
            }

            if (IsRunning) {
                return;
            }

            if (CurrentCoroutine != null) {
                CurrentCoroutine.Resume();
                Coroutines.Instance.Start(CurrentCoroutine);
            }

            IsRunning = true;
        }

        public void Stop() {
            if (!HasStarted) {
                return;
            }

            NextState = PreviousState = CurrentState = StartState = null;

            if (CurrentCoroutine != null) {
                Coroutines.Instance.Remove(CurrentCoroutine);
                CurrentCoroutine.Stop();
                CurrentCoroutine = null;
            }

            HasStarted = IsRunning = false;
        }

        public void ChangeState(T label) {
            if (CurrentState == null) {
                throw new System.InvalidOperationException("StateMachine hasn't started yet.\nStart() should be called before anything else.");
            }

            if (!_states.TryGetValue(label, out State<T> nextState)) {
                throw new System.ArgumentException($"There is no registered state with label '{label.ToString()}'.");
            }

            if (nextState == CurrentState) {
                return;
            }

            NextState = nextState;
            UpdateState();
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

            /*
                                (StateEnterDelegate) onEnterStateMethodInfo.CreateDelegate(typeof(StateEnterDelegate), Entity),
                                (StateUpdateDelegate) onUpdateStateMethodInfo.CreateDelegate(typeof(StateUpdateDelegate), Entity),
                                (StateLeaveDelegate) onLeaveStateMethodInfo.CreateDelegate(typeof(StateLeaveDelegate), Entity)
            */

            State<T> state = new State<T>(
                label,
                onEnterStateMethodInfo,
                onUpdateStateMethodInfo,
                onLeaveStateMethodInfo
            );

            _states.Add(label, state);
        }

        public void AddStates(params T[] labels) {
            foreach (T label in labels) {
                AddState(label);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateState() {
            ProcessStateLeave(CurrentState.OnLeave);

            CurrentCoroutine.Stop();

            PreviousState = CurrentState;
            CurrentState = NextState;
            NextState = null;

            if (CurrentState == null) {
                return;
            }

            ProcessStateEnter(CurrentState.OnEnter);
            CurrentCoroutine = Coroutines.Instance.Start(ProcessStateUpdate(CurrentState.OnUpdate));
        }

        private void ProcessStateEnter(MethodInfo onEnter) {
            onEnter.Invoke(Entity, parameters: null);
        }

        private IEnumerator ProcessStateUpdate(MethodInfo onUpdate) {
            IEnumerator currentState = (IEnumerator) onUpdate.Invoke(Entity, parameters: null);

            while (currentState.MoveNext() && currentState != null && Entity != null) {
                // intercept Update() yield values
                if (currentState.Current is T stateLabel) {
                    ChangeState(stateLabel);
                    yield break;
                }

                yield return currentState.Current;
            }
        }

        private void ProcessStateLeave(MethodInfo onLeave) {
            onLeave.Invoke(Entity, parameters: null);
        }

        #endregion Private Methods
    }
}
