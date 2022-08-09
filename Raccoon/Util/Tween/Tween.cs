using System.Reflection;
using System.Collections.Generic;

using Raccoon.Graphics;

namespace Raccoon.Util.Tween {
    public class Tween : System.IDisposable {
        #region Public Members

        public delegate void TweenUpdateDelegate(float t);

        #endregion Public Members

        #region Private Members

        private static Dictionary<System.Type, ConstructorInfo> LerpersAvailable;

        private event System.Action _onStart, _onEnd;
        private event TweenUpdateDelegate _onUpdate;

        private Dictionary<string, Lerper> _lerpers = new Dictionary<string, Lerper>();
        private bool _startReverse;
        private int _duration;

        #endregion Private Members

        #region Static

        static Tween() {
            System.Type[] types = new System.Type[] {
                typeof(object),
                typeof(string),
                typeof(MemberTypes),
                typeof(System.Func<float, float>),
                typeof(bool)
            };

            LerpersAvailable = new Dictionary<System.Type, ConstructorInfo> {
                { typeof(int),          typeof(NumberLerper).GetConstructor(types)    },
                { typeof(byte),         typeof(NumberLerper).GetConstructor(types)    },
                { typeof(uint),         typeof(NumberLerper).GetConstructor(types)    },
                { typeof(long),         typeof(NumberLerper).GetConstructor(types)    },
                { typeof(ulong),        typeof(NumberLerper).GetConstructor(types)    },
                { typeof(sbyte),        typeof(NumberLerper).GetConstructor(types)    },
                { typeof(short),        typeof(NumberLerper).GetConstructor(types)    },
                { typeof(float),        typeof(NumberLerper).GetConstructor(types)    },
                { typeof(ushort),       typeof(NumberLerper).GetConstructor(types)    },
                { typeof(double),       typeof(NumberLerper).GetConstructor(types)    },
                { typeof(decimal),      typeof(NumberLerper).GetConstructor(types)    },
                { typeof(Vector2),      typeof(Vector2Lerper).GetConstructor(types)   },
                { typeof(Size),         typeof(SizeLerper).GetConstructor(types)      },
                { typeof(Rectangle),    typeof(RectangleLerper).GetConstructor(types) },
                { typeof(Color),        typeof(ColorLerper).GetConstructor(types)     }
            };
        }

        #endregion Static

        #region Constructors

        public Tween(object subject, int duration, bool additional = false) {
            if (subject == null) {
                throw new System.ArgumentNullException(nameof(subject));
            }

            if (duration <= 0) {
                throw new System.ArgumentException("Duration can't be zero or negative.");
            }

            Subject = subject;
            _duration = duration;
            IsAdditional = additional;
        }

        #endregion Constructors

        #region Public Properties

        public object Subject { get; set; }
        public uint Timer { get; set; }
        public int RepeatTimes { get; set; }
        public int TimesPlayed { get; private set; }
        public float Time { get; private set; }
        public bool IsAdditional { get; private set; }
        public bool HasEnded { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsLooping { get { return RepeatTimes < 0; } set { RepeatTimes = value ? -1 : 0; } }
        public bool IsPingPong { get; set; }
        public bool IsReverse { get; set; }
        public bool IsForward { get { return !IsReverse; } set { IsReverse = !value; } }
        public bool CanDisposeWhenRemoved { get; set; } = true;
        public bool IsDisposed { get; private set; }

        public Lerper this[string name] { get { return _lerpers[name]; } }

        public int Duration {
            get {
                return _duration;
            }

            set {
                if (value <= 0) {
                    throw new System.ArgumentException("Duration can't be zero or negative.");
                }

                _duration = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static float Apply(float value, float t, System.Func<float, float> easing) {
            return value * easing(t);
        }

        public static float Apply(float from, float to, float t, System.Func<float, float> easing) {
            return from + (to - from) * easing(t);
        }

        public static Vector2 Apply(Vector2 value, float t, System.Func<float, float> easing) {
            return value * easing(t);
        }

        public static Vector2 Apply(Vector2 from, Vector2 to, float t, System.Func<float, float> easing) {
            return from + (to - from) * easing(t);
        }

        public void Update(int delta) {
            if (!IsPlaying || IsPaused) {
                return;
            }

            Timer += (uint) delta;
            Time = Math.Clamp(Timer / (float) Duration, 0f, 1f);

            foreach (Lerper lerp in _lerpers.Values) {
                lerp.Interpolate(IsReverse ? 1f - Time : Time);
            }

            _onUpdate?.Invoke(IsReverse ? 1f - Time : Time);

            if (Time >= 1) {
                TimesPlayed++;
                if (IsPingPong) {
                    IsReverse = !IsReverse;
                }

                if (!IsLooping && TimesPlayed >= RepeatTimes + 1) {
                    IsPlaying = false;
                    HasEnded = true;
                } else {
                    Timer = Timer - (uint) Duration;
                }

                _onEnd?.Invoke();
            }
        }

        public void Reset() {
            IsPlaying = IsPaused = false;
            HasEnded = false;
            Timer = 0;
            TimesPlayed = 0;
            Time = 0;
            IsReverse = _startReverse;

            foreach (Lerper lerp in _lerpers.Values) {
                lerp.Reset();
            }
        }

        public Tween Play(bool forceReset = true) {
            if (forceReset) {
                Reset();
            } else if (IsPlaying && IsPaused) {
                Resume();
                return this;
            } else if (HasEnded) {
                // tween has ended previously
                // and it should play again
                // (without reset)
                TimesPlayed = 0;
                HasEnded = false;
            }

            IsPlaying = true;
            IsPaused = false;

            if (Timer == 0) {
                _onStart?.Invoke();
            }

            if (_lerpers.Count == 0) {
                IsPlaying = false;
                HasEnded = true;
            }

            if (forceReset) {
                MoveLerpersToStart();
            }

            return this;
        }

        public void Resume() {
            if (!IsPaused) {
                return;
            }

            IsPaused = false;
        }

        public void Pause() {
            if (IsPaused) {
                return;
            }

            IsPaused = true;
        }

        public void ClearLerpers() {
            foreach (Lerper lerper in _lerpers.Values) {
                lerper.Dispose();
            }

            _lerpers.Clear();
        }

        public Tween From(object start) {
            foreach (PropertyInfo property in start.GetType().GetProperties()) {
                Lerper lerper = Lerper(property);
                lerper.From = System.Convert.ChangeType(property.GetValue(start), lerper.DataType);
            }

            return this;
        }

        public Tween To(object target, System.Func<float, float> easing = null) {
            foreach (PropertyInfo property in target.GetType().GetProperties()) {
                Lerper lerper = Lerper(property);
                lerper.To = System.Convert.ChangeType(property.GetValue(target), lerper.DataType);

                if (easing != null) {
                    lerper.Easing = easing;
                }
            }

            return this;
        }

        public Tween Repeat(int times = -1) {
            RepeatTimes = times;
            return this;
        }

        public Tween Loop() {
            IsLooping = true;
            return this;
        }

        public Tween PingPong() {
            IsPingPong = true;

            if (RepeatTimes == 0) {
                RepeatTimes = 1;
            } else {
                RepeatTimes *= 2;
            }

            return this;
        }

        public Tween Reverse() {
            if (IsReverse) {
                return this;
            }

            Timer = (uint) ((1f - Time) * Duration);
            IsReverse = true;
            _startReverse = true;
            return this;
        }

        public Tween Forward() {
            if (IsForward) {
                return this;
            }

            Timer = (uint) ((1f - Time) * Duration);
            IsForward = true;
            _startReverse = false;
            return this;
        }

        public Tween OnStart(System.Action onStart) {
            _onStart += onStart;
            return this;
        }

        public Tween OnUpdate(TweenUpdateDelegate onUpdate) {
            _onUpdate += onUpdate;
            return this;
        }

        public Tween OnEnd(System.Action onEnd) {
            _onEnd += onEnd;
            return this;
        }

        public Tween DisposeWhenRemoved(bool canDispose = true) {
            CanDisposeWhenRemoved = canDispose;
            return this;
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            IsDisposed = true;
            HasEnded = true;
            _onStart = null;
            _onEnd = null;
            _onUpdate = null;

            foreach (Lerper lerper in _lerpers.Values) {
                lerper.Dispose();
            }
            _lerpers.Clear();

            Subject = null;
        }

        #endregion Public Methods

        #region Private Methods

        private void MoveLerpersToStart() {
            if (IsForward) {
                foreach (Lerper lerp in _lerpers.Values) {
                    lerp.Begin();
                }
            } else {
                foreach (Lerper lerp in _lerpers.Values) {
                    lerp.End();
                }
            }
        }

        private MemberInfo FindSubjectMember(PropertyInfo property) {
            PropertyInfo subjectProperty = Subject.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (subjectProperty != null) {
                return subjectProperty;
            } else {
                FieldInfo subjectField = Subject.GetType().GetField(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (subjectField != null) {
                    return subjectField;
                }
            }

            return null;
        }

        private Lerper Lerper(PropertyInfo property) {
            MemberInfo subjectMember = FindSubjectMember(property);

            if (subjectMember == null) {
                throw new System.ArgumentException($"Subject '{Subject.GetType()}' does not contains a property nor field called '{property.Name}'.");
            }

            // try to get
            if (_lerpers.TryGetValue(subjectMember.Name, out Lerper lerper)) {
                return lerper;
            }

            System.Type dataType = null;

            if (subjectMember is PropertyInfo propertyInfo && propertyInfo != null) {
                dataType = propertyInfo.PropertyType;
            } else if (subjectMember is FieldInfo fieldInfo && fieldInfo != null) {
                dataType = fieldInfo.FieldType;
            }

            // create
            if (LerpersAvailable.TryGetValue(dataType, out ConstructorInfo constructorInfo)) {
                lerper = (Lerper) constructorInfo.Invoke(
                    new object[] {
                        Subject,
                        property.Name,
                        subjectMember.MemberType,
                        new System.Func<float, float>(Ease.Linear),
                        IsAdditional
                    }
                );
            }

            if (lerper == null) {
                throw new System.NotImplementedException($"Lerper for data type '{dataType.Name}' not found.");
            }

            _lerpers.Add(subjectMember.Name, lerper);
            return lerper;
        }

        #endregion Private Methods
    }
}
