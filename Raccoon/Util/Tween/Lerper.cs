using System.Reflection;

using Raccoon.Graphics;

namespace Raccoon.Util.Tween {
    #region Base Lerper

    public abstract class Lerper : System.IDisposable {
        #region Private Members

        private object _totalAdditionalValue;

        #endregion Private Members

        #region Constructors

        public Lerper(object owner, string name, MemberTypes memberType, System.Func<float, float> easing, bool additional) {
            Owner = owner;
            Name = name;
            Easing = easing;
            IsAdditional = additional;

            System.Type ownerType = Owner.GetType();

            switch (memberType) {
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = Owner.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    MemberInfo = propertyInfo;
                    DataType = propertyInfo.PropertyType;
                    break;

                case MemberTypes.Field:
                    FieldInfo fieldInfo = Owner.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    MemberInfo = fieldInfo;
                    DataType = fieldInfo.FieldType;
                    break;

                default:
                    throw new System.NotSupportedException($"Member type '{memberType}'.");
            }

            _totalAdditionalValue = System.Activator.CreateInstance(DataType);
            From = To = Value;
        }

        #endregion Constructors

        #region Public Properties

        public object Owner { get; protected set; }
        public MemberInfo MemberInfo { get; protected set; }
        public System.Type DataType { get; protected set; }
        public string Name { get; protected set; }
        public bool IsAdditional { get; private set; }
        public System.Func<float, float> Easing { get; set; }
        public object From { get; set; }
        public object To { get; set; }
        public bool IsDisposed { get; private set; }

        public object Value {
            get {
                if (MemberInfo is PropertyInfo propertyInfo) {
                    return propertyInfo.GetValue(Owner);
                } else if (MemberInfo is FieldInfo fieldInfo) {
                    return fieldInfo.GetValue(Owner);
                }

                return null;
            }

            set {
                object result;

                if (IsAdditional) {
                    result = CalculateAdditionalValue(value);
                } else {
                    result = value;
                }

                ApplyValue(result);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void Begin() {
            if (IsAdditional) {
                ApplyValue(CalculateAdditionalValue(From));
            } else {
                Value = From;
            }
        }

        public virtual void End() {
            if (IsAdditional) {
                ApplyValue(CalculateAdditionalValue(To));
            } else {
                Value = To;
            }
        }

        public void Reset() {
            if (IsAdditional) {
                // reapply base value
                ApplyValue(Subtract(Value, _totalAdditionalValue));
                _totalAdditionalValue = System.Activator.CreateInstance(DataType);
            } else {
                Value = From;
            }
        }

        public abstract void Interpolate(float t);

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            Disposed();
            IsDisposed = true;

            _totalAdditionalValue = null;
            Owner = null;
            MemberInfo = null;
            DataType = null;
            Easing = null;
            From = To = null;
        }

        #endregion Public Methods

        #region Protected Methods

        protected abstract object Add(object a, object b);
        protected abstract object Subtract(object a, object b);
        protected abstract void Disposed();

        #endregion Protected Methods

        #region Private Methods

        private void ApplyValue(object value) {
            if (MemberInfo is PropertyInfo propertyInfo) {
                propertyInfo.SetValue(Owner, value);
            } else if (MemberInfo is FieldInfo fieldInfo) {
                fieldInfo.SetValue(Owner, value);
            }
        }

        private object CalculateAdditionalValue(object value) {
            object baseValue = Subtract(Value, _totalAdditionalValue);
            _totalAdditionalValue = value;
            return Add(baseValue, _totalAdditionalValue);
        }

        #endregion Private Methods
    }

    #endregion Base Lerper

    #region Number Lerper

    public class NumberLerper : Lerper {
        public NumberLerper(object owner, string name, MemberTypes memberType, System.Func<float, float> easing, bool additional) : base(owner, name, memberType, easing, additional) {
        }

        public new float From { get { return (float) base.From; } set { base.From = value; } }
        public new float To { get { return (float) base.To; } set { base.To = value; } }
        public new float Value { get { return (float) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            Value = From + (To - From) * Easing(t);
        }

        protected override object Add(object a, object b) {
            return (float) a + (float) b;
        }

        protected override object Subtract(object a, object b) {
            return (float) a - (float) b;
        }

        protected override void Disposed() {
        }
    }

    #endregion Number Lerper

    #region Vector2 Lerper

    public class Vector2Lerper : Lerper {
        public Vector2Lerper(object owner, string name, MemberTypes memberType, System.Func<float, float> easing, bool additional) : base(owner, name, memberType, easing, additional) {
        }

        public new Vector2 From { get { return (Vector2) base.From; } set { base.From = value; } }
        public new Vector2 To { get { return (Vector2) base.To; } set { base.To = value; } }
        public new Vector2 Value { get { return (Vector2) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            Value = From + (To - From) * Easing(t);
        }

        protected override object Add(object a, object b) {
            return (Vector2) a + (Vector2) b;
        }

        protected override object Subtract(object a, object b) {
            return (Vector2) a - (Vector2) b;
        }

        protected override void Disposed() {
        }
    }

    #endregion Vector2 Lerper

    #region Size Lerper

    public class SizeLerper : Lerper {
        public SizeLerper(object owner, string name, MemberTypes memberType, System.Func<float, float> easing, bool additional) : base(owner, name, memberType, easing, additional) {
        }

        public new Size From { get { return (Size) base.From; } set { base.From = value; } }
        public new Size To { get { return (Size) base.To; } set { base.To = value; } }
        public new Size Value { get { return (Size) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            Value = From + (To - From) * Easing(t);
        }

        protected override object Add(object a, object b) {
            return (Size) a + (Size) b;
        }

        protected override object Subtract(object a, object b) {
            return (Size) a - (Size) b;
        }

        protected override void Disposed() {
        }
    }

    #endregion Size Lerper

    #region Rectangle Lerper

    public class RectangleLerper : Lerper {
        public RectangleLerper(object owner, string name, MemberTypes memberType, System.Func<float, float> easing, bool additional) : base(owner, name, memberType, easing, additional) {
        }

        public new Rectangle From { get { return (Rectangle) base.From; } set { base.From = value; } }
        public new Rectangle To { get { return (Rectangle) base.To; } set { base.To = value; } }
        public new Rectangle Value { get { return (Rectangle) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            t = Easing(t);
            Value = new Rectangle(From.Position + (To.Position - From.Position) * t, From.Size + (To.Size - From.Size) * t);
        }

        protected override object Add(object a, object b) {
            Rectangle rectA = (Rectangle) a,
                      rectB = (Rectangle) b;

            return new Rectangle(rectA.Position + rectB.Position, rectA.Size + rectB.Size);
        }

        protected override object Subtract(object a, object b) {
            Rectangle rectA = (Rectangle) a,
                      rectB = (Rectangle) b;

            return new Rectangle(rectA.Position - rectB.Position, rectA.Size - rectB.Size);
        }

        protected override void Disposed() {
        }
    }

    #endregion Rectangle Lerper

    #region Color Lerper

    public class ColorLerper : Lerper {
        public ColorLerper(object owner, string name, MemberTypes memberType, System.Func<float, float> easing, bool additional) : base(owner, name, memberType, easing, additional) {
        }

        public new Color From { get { return (Color) base.From; } set { base.From = value; } }
        public new Color To { get { return (Color) base.To; } set { base.To = value; } }
        public new Color Value { get { return (Color) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            t = Easing(t);
            Value = new Color(
                (byte) (From.R + (To.R - From.R) * t),
                (byte) (From.G + (To.G - From.G) * t),
                (byte) (From.B + (To.B - From.B) * t)
            );
        }

        protected override object Add(object a, object b) {
            return (Color) a + (Color) b;
        }

        protected override object Subtract(object a, object b) {
            return (Color) a - (Color) b;
        }

        protected override void Disposed() {
        }
    }

    #endregion Color Lerper
}
