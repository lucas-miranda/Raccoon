using Raccoon.Graphics.Primitives;

namespace Raccoon.Graphics.Transitions {
    public enum Fade {
        In,
        Out
    }

    public class FadeTransition : SceneTransition {
        public FadeTransition(Color color) : base(new RectangleShape(Game.Instance.ScreenWidth, Game.Instance.ScreenHeight, color)) {
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
    }
}
