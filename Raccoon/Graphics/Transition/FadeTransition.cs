namespace Raccoon.Graphics.Transition {
    public enum Fade {
        In,
        Out
    }

    public class FadeTransition : SceneTransition {
        public FadeTransition(Color color) : base(new Primitive.Rectangle(Game.Instance.Width, Game.Instance.Height, color)) {
            Type = Fade.In;
        }

        public Fade Type { get; set; }

        public override void Update(int delta) {
            base.Update(delta);
            EndGraphic.Opacity = Time;
        }

        public void Play(Fade type, int repeatTimes = 0) {
            Type = type;
            Play(repeatTimes);
        }

        public override void Play(int repeatTimes = 0) {
            switch (Type) {
                case Fade.In:
                    Reverse = false;
                    EndGraphic.Opacity = 0f;
                    break;

                case Fade.Out:
                    Reverse = true;
                    EndGraphic.Opacity = 1f;
                    break;

                default:
                    break;
            }
            base.Play(repeatTimes);
        }

        internal override void Load() {
            EndGraphic.Load();
        }
    }
}
