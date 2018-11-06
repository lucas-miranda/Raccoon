using System;
using System.Reflection;
using System.Collections.Generic;

using Raccoon.Graphics;

namespace Raccoon.Util.Tween {
    public class Tween {
        #region Private Static Members

        private static Dictionary<Type, ConstructorInfo> LerpersAvailable;

        #endregion Private Static Members

        #region Private Events

        private event Action _onStart, _onUpdate, _onEnd;

        #endregion Private Events

        #region Private Members

        private Dictionary<string, Lerper> _lerpers = new Dictionary<string, Lerper>();
        private bool _startReverse;

        #endregion Private Members

        #region Static 

        static Tween() {
            Type[] types = new Type[] { typeof(object), typeof(string), typeof(Func<float, float>) };

            LerpersAvailable = new Dictionary<Type, ConstructorInfo> {
                { typeof(int), typeof(NumberLerper).GetConstructor(types) },
                { typeof(byte), typeof(NumberLerper).GetConstructor(types) },
                { typeof(uint), typeof(NumberLerper).GetConstructor(types) },
                { typeof(long), typeof(NumberLerper).GetConstructor(types) },
                { typeof(ulong), typeof(NumberLerper).GetConstructor(types) },
                { typeof(sbyte), typeof(NumberLerper).GetConstructor(types) },
                { typeof(short), typeof(NumberLerper).GetConstructor(types) },
                { typeof(float), typeof(NumberLerper).GetConstructor(types) },
                { typeof(ushort), typeof(NumberLerper).GetConstructor(types) },
                { typeof(double), typeof(NumberLerper).GetConstructor(types) },
                { typeof(decimal), typeof(NumberLerper).GetConstructor(types) },
                { typeof(Vector2), typeof(Vector2Lerper).GetConstructor(types) },
                { typeof(Size), typeof(SizeLerper).GetConstructor(types) },
                { typeof(Rectangle), typeof(RectangleLerper).GetConstructor(types) },
                { typeof(Color), typeof(ColorLerper).GetConstructor(types) }
            };
        }

        #endregion Static

        #region Constructors

        public Tween(object subject, int duration) {
            Subject = subject;
            Duration = duration;
        }

        #endregion Constructors

        #region Public Properties

        public object Subject { get; set; }
        public uint Timer { get; private set; }
        public int Duration { get; private set; }
        public int RepeatTimes { get; set; } = 1;
        public int TimesPlayed { get; private set; }
        public float Time { get; private set; }
        public bool HasEnded { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsLooping { get { return RepeatTimes < 0; } set { RepeatTimes = value ? -1 : 0; } }
        public bool IsPingPong { get; set; }
        public bool IsReverse { get; set; }
        public bool IsForward { get { return !IsReverse; } set { IsReverse = !value; } }

        public Lerper this[string name] { get { return _lerpers[name]; } }

        #endregion Public Properties

        #region Public Methods

        public void Update(int delta) {
            if (!IsPlaying) {
                return;
            }

            Timer += (uint) delta;
            Time = Math.Clamp(Timer / (float) Duration, 0f, 1f);

            foreach (Lerper lerp in _lerpers.Values) {
                lerp.Interpolate(IsReverse ? 1f - Time : Time);
            }

            _onUpdate?.Invoke();

            if (Time >= 1) {
                TimesPlayed++;
                if (IsPingPong) {
                    IsReverse = !IsReverse;
                }

                if (!IsLooping && TimesPlayed == RepeatTimes) {
                    IsPlaying = false;
                    HasEnded = true;
                } else {
                    Timer = Timer - (uint) Duration;
                }

                _onEnd?.Invoke();
            }
        }

        public void Reset() {
            IsPlaying = false;
            HasEnded = false;
            Timer = 0;
            TimesPlayed = 0;
            Time = 0;
            IsReverse = _startReverse;

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

        public void Play(bool forceReset = true) {
            if (forceReset) {
                Reset();
            }

            IsPlaying = true;

            if (Timer == 0) {
                _onStart?.Invoke();
            }

            if (_lerpers.Count == 0) {
                IsPlaying = false;
                HasEnded = true;
            }
        }

        public void Pause() {
            IsPlaying = false;
        }

        public void ClearLerpers() {
            _lerpers.Clear();
        }

        public Tween From(object start) {
            PropertyInfo[] properties = start.GetType().GetProperties();
            foreach (PropertyInfo property in properties) {
                PropertyInfo subjectProperty = Subject.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (subjectProperty == null) {
                    throw new ArgumentException("Subject does not contains a property '" + property.Name + "'", "start");
                }

                if (!_lerpers.TryGetValue(subjectProperty.Name, out Lerper lerper)) {
                    lerper = CreateLerper(subjectProperty);
                }

                lerper.From = Convert.ChangeType(property.GetValue(start), subjectProperty.PropertyType);
            }

            return this;
        }

        public Tween To(object target, Func<float, float> easing = null) {
            PropertyInfo[] properties = target.GetType().GetProperties();
            foreach (PropertyInfo property in properties) {
                PropertyInfo subjectProperty = Subject.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (subjectProperty == null) {
                    throw new ArgumentException("Subject does not contains a property '" + property.Name + "'", "target");
                }

                if (!_lerpers.TryGetValue(subjectProperty.Name, out Lerper lerper)) {
                    lerper = CreateLerper(subjectProperty);
                }

                lerper.To = Convert.ChangeType(property.GetValue(target), subjectProperty.PropertyType);

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
            RepeatTimes *= 2;
            return this;
        }

        public Tween Reverse() {
            if (IsReverse) {
                return this;
            }

            if (IsPlaying) {
                Timer = (uint) ((1f - Time) * Duration);
            }

            IsReverse = true;
            _startReverse = true;
            return this;
        }

        public Tween Forward() {
            if (IsForward) {
                return this;
            }

            if (IsPlaying) {
                Timer = (uint) ((1f - Time) * Duration);
            }

            IsForward = true;
            _startReverse = false;
            return this;
        }

        public Tween OnStart(Action onStart) {
            _onStart += onStart;
            return this;
        }

        public Tween OnUpdate(Action onUpdate) {
            _onUpdate += onUpdate;
            return this;
        }

        public Tween OnEnd(Action onEnd) {
            _onEnd += onEnd;
            return this;
        }

        private Lerper CreateLerper(PropertyInfo property) {
            PropertyInfo subjectProperty = Subject.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (subjectProperty == null) {
                throw new ArgumentException("Subject does not contains a property '" + property.Name + "'", "target");
            }

            if (_lerpers.TryGetValue(subjectProperty.Name, out Lerper lerper)) {
                return lerper;
            }

            if (LerpersAvailable.TryGetValue(subjectProperty.PropertyType, out ConstructorInfo constructorInfo)) {
                lerper = (Lerper) constructorInfo.Invoke(new object[] { Subject, property.Name, new Func<float, float>(Ease.Linear) });
            }

            if (lerper == null) {
                throw new NotImplementedException("Lerper with type '" + subjectProperty.PropertyType.Name + "' not found");
            }

            _lerpers.Add(subjectProperty.Name, lerper);
            return lerper;
        }

        #endregion Public Methods
    }
}
