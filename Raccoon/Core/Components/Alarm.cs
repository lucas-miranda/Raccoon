namespace Raccoon.Components {
    public class Alarm : Component {
        public Alarm(uint interval, System.Action action) {
            NextActivationTimer = Interval = interval;
            Action = action;
        }

        public System.Action Action { get; set; }
        public uint Timer { get; private set; }
        public uint NextActivationTimer { get; private set; }
        public uint Interval { get; set; }
        public uint RepeatTimes { get; set; } = uint.MaxValue;
        public uint RepeatCount { get; private set; }

        public override void Update(int delta) {
            Timer += (uint) delta;

            if (Timer < NextActivationTimer) {
                return;
            }

            Action();

            if (RepeatCount < RepeatTimes) {
                RepeatCount++;
                NextActivationTimer += Interval;
            }
        }

        public override void Render() {
        }

        public override void DebugRender() {
        }

        public void Reset() {
            RepeatCount = 0;
            Timer = 0;
            NextActivationTimer = Interval;
        }
    }
}
