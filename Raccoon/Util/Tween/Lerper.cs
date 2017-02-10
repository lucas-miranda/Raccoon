using System;
using System.Reflection;

using Raccoon.Graphics;

namespace Raccoon.Util.Tween {
    #region Base Lerper

    public abstract class Lerper {
        public Lerper(object owner, string name, Func<float, float> easing) {
            Owner = owner;
            Property = Owner.GetType().GetProperty(name);
            Name = name;
            From = To = Value;
            Easing = easing;
        }

        public object Owner { get; protected set; }
        public PropertyInfo Property { get; protected set; }
        public string Name { get; protected set; }
        public Func<float, float> Easing { get; set; }
        public object From { get; set; }
        public object To { get; set; }
        public object Value { get { return Property.GetValue(Owner); } set { Property.SetValue(Owner, value); } }

        public virtual void Begin() {
            Value = From;
        }

        public virtual void End() {
            Value = To;
        }

        public abstract void Interpolate(float t);
    }

    #endregion Base Lerper

    #region Number Lerper

    public class NumberLerper : Lerper {
        public NumberLerper(object owner, string name, Func<float, float> easing) : base(owner, name, easing) { }

        public new float From { get { return (float) base.From; } set { base.From = value; } }
        public new float To { get { return (float) base.To; } set { base.To = value; } }
        public new float Value { get { return (float) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            Value = From + (To - From) * Easing(t);
        }
    }

    #endregion Number Lerper

    #region Vector2 Lerper

    public class Vector2Lerper : Lerper {
        public Vector2Lerper(object owner, string name, Func<float, float> easing) : base(owner, name, easing) { }

        public new Vector2 From { get { return (Vector2) base.From; } set { base.From = value; } }
        public new Vector2 To { get { return (Vector2) base.To; } set { base.To = value; } }
        public new Vector2 Value { get { return (Vector2) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            Value = From + (To - From) * Easing(t);
        }
    }

    #endregion Vector2 Lerper

    #region Size Lerper

    public class SizeLerper : Lerper {
        public SizeLerper(object owner, string name, Func<float, float> easing) : base(owner, name, easing) { }

        public new Size From { get { return (Size) base.From; } set { base.From = value; } }
        public new Size To { get { return (Size) base.To; } set { base.To = value; } }
        public new Size Value { get { return (Size) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            Value = From + (To - From) * Easing(t);
        }
    }

    #endregion Size Lerper

    #region Rectangle Lerper

    public class RectangleLerper : Lerper {
        public RectangleLerper(object owner, string name, Func<float, float> easing) : base(owner, name, easing) { }

        public new Rectangle From { get { return (Rectangle) base.From; } set { base.From = value; } }
        public new Rectangle To { get { return (Rectangle) base.To; } set { base.To = value; } }
        public new Rectangle Value { get { return (Rectangle) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            t = Easing(t);
            Value = new Rectangle(From.Position + (To.Position - From.Position) * t, From.Size + (To.Size - From.Size) * t);
        }
    }

    #endregion Rectangle Lerper

    #region Color Lerper

    public class ColorLerper : Lerper {
        public ColorLerper(object owner, string name, Func<float, float> easing) : base(owner, name, easing) { }

        public new Color From { get { return (Color) base.From; } set { base.From = value; } }
        public new Color To { get { return (Color) base.To; } set { base.To = value; } }
        public new Color Value { get { return (Color) base.Value; } set { base.Value = value; } }

        public override void Interpolate(float t) {
            t = Easing(t);
            Value = new Color((byte) (From.R + (To.R - From.R) * t), (byte) (From.G + (To.G - From.G) * t), (byte) (From.B + (To.B - From.B) * t));
        }
    }

    #endregion Color Lerper
}
