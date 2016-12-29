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

        private Dictionary<PropertyInfo, Lerper> _lerpers;

        #endregion Private Members

        #region Static 

        static Tween() {
            Type[] types = new Type[] { typeof(object), typeof(string), typeof(Func<float, float>) };

            LerpersAvailable = new Dictionary<Type, ConstructorInfo>();
            LerpersAvailable.Add(typeof(int), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(byte), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(uint), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(long), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(ulong), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(sbyte), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(short), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(float), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(ushort), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(double), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(decimal), typeof(NumberLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(Vector2), typeof(Vector2Lerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(Size), typeof(SizeLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(Rectangle), typeof(RectangleLerper).GetConstructor(types));
            LerpersAvailable.Add(typeof(Color), typeof(ColorLerper).GetConstructor(types));
        }

        #endregion Static

        #region Constructors

        public Tween(object subject, int duration) {
            _lerpers = new Dictionary<PropertyInfo, Lerper>();
            Subject = subject;
            Duration = duration;
        }

        #endregion Constructors

        #region Public Properties

        public object Subject { get; private set; }
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

        #endregion Public Properties

        #region Public Methods

        public void Update(int delta) {
            if (!IsPlaying) {
                return;
            }

            Timer += (uint) delta;
            Time = Math.Clamp(Timer / (float) Duration, 0, 1);

            foreach (Lerper lerp in _lerpers.Values) {
                lerp.Interpolate(IsReverse ? 1 - Time : Time);
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
                    Timer = 0;
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

            foreach (Lerper lerp in _lerpers.Values) {
                lerp.Initialize();
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

        public Tween From(object start) {
            PropertyInfo[] properties = start.GetType().GetProperties();
            foreach (PropertyInfo property in properties) {
                PropertyInfo subjectProperty = Subject.GetType().GetProperty(property.Name);
                if (subjectProperty == null) throw new ArgumentException("Subject does not contains a property '" + property.Name + "'", "start");

                Lerper lerper;
                if (!_lerpers.TryGetValue(subjectProperty, out lerper)) {
                    lerper = CreateLerper(subjectProperty);
                }

                //throw new ArgumentException("Lerper to property '" + property.Name + "' not found. Maybe you forgot to add it?", "start");

                lerper.From = property.GetValue(start);
            }

            return this;
        }

        public Tween To(object target, Func<float, float> easing = null) {
            PropertyInfo[] properties = target.GetType().GetProperties();
            foreach (PropertyInfo property in properties) {
                PropertyInfo subjectProperty = Subject.GetType().GetProperty(property.Name);
                if (subjectProperty == null) throw new ArgumentException("Subject does not contains a property '" + property.Name + "'", "target");

                Lerper lerper;
                if (!_lerpers.TryGetValue(subjectProperty, out lerper)) {
                    lerper = CreateLerper(subjectProperty);
                }

                lerper.To = property.GetValue(target);

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
            return this;
        }

        public Tween Reverse() {
            IsReverse = true;
            return this;
        }

        public Tween Forward() {
            IsForward = true;
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
            PropertyInfo subjectProperty = Subject.GetType().GetProperty(property.Name);
            if (subjectProperty == null) throw new ArgumentException("Subject does not contains a property '" + property.Name + "'", "target");

            Lerper lerper;
            if (_lerpers.TryGetValue(subjectProperty, out lerper)) {
                return lerper;
            }

            ConstructorInfo constructorInfo;
            if (LerpersAvailable.TryGetValue(subjectProperty.PropertyType, out constructorInfo)) {
                lerper = (Lerper) constructorInfo.Invoke(new object[] { Subject, property.Name, new Func<float, float>(Ease.Linear) });
            }

            if (lerper == null) throw new NotImplementedException("Lerper with type '" + subjectProperty.PropertyType.Name + "' not found");

            _lerpers.Add(subjectProperty, lerper);
            return lerper;
        }

        #endregion Public Methods
    }
}
