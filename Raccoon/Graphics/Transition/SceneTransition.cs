namespace Raccoon.Graphics {
    public abstract class SceneTransition : Graphic {
        public delegate void EventHandler();
        public event EventHandler OnStart;
        public event EventHandler OnEnd;

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

            float deltaSec = delta * Raccoon.Time.MiliToSec;
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
                    if (OnEnd != null) {
                        OnEnd();
                    }
                }
            }
        }

        public override void Draw() {
            EndGraphic.Draw();
        }

        public virtual void Play(int repeatTimes = 0) {
            Playing = true;
            Finished = false;
            Time = Reverse ? 1f : 0f;
            ElapsedTime = 0f;
            RepeatTimes = repeatTimes;
            if (OnStart != null)
                OnStart();
        }
    }
}
