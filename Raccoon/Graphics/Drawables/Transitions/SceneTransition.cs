namespace Raccoon.Graphics.Transitions {
    public abstract class SceneTransition : Graphic {
        public delegate void CallbackHandler();
        public event CallbackHandler OnStart;
        public event CallbackHandler OnEnd;

        protected SceneTransition(Graphic endGraphic) {
            EndGraphic = endGraphic;
            ElapsedTime = 0f;
        }

        public Graphic EndGraphic { get; protected set; }
        public int RepeatTimes { get; set; }
        public float Time { get; set; }
        public float ElapsedTime { get; set; }
        public bool Playing { get; private set; }
        public bool Finished { get; private set; }
        public bool Reverse { get; set; }

        public override void Update(int delta) {
            if (!Playing)
                return;

            float deltaSec = delta * Util.Time.MiliToSec;
            ElapsedTime += deltaSec;
            if (Reverse) {
                Time -= deltaSec;
                if (Time <= 0f) {
                    Time = 0f;
                    Finished = true;
                }
            } else {
                Time += deltaSec;
                if (Time >= 1f) {
                    Time = 1f;
                    Finished = true;
                }
            }

            if (Finished) {
                if (RepeatTimes > 0) {
                    Finished = false;
                    Reverse = !Reverse;
                    Time = Reverse ? 1f : 0f;
                    RepeatTimes--;
                } else {
                    Playing = false;
                    OnEnd?.Invoke();
                }
            }
        }

        public override void Render(Vector2 position, Color color, float rotation) {
            EndGraphic.Render(position, rotation);
        }

        public virtual void Play(int repeatTimes = 0) {
            Playing = true;
            Finished = false;
            Time = Reverse ? 1f : 0f;
            ElapsedTime = 0f;
            RepeatTimes = repeatTimes;
            OnStart?.Invoke();
        }

        public override void Dispose() {
            if (EndGraphic != null) {
                EndGraphic.Dispose();
            }
        }
    }
}
