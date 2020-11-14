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
        public uint TriggeredCount { get; private set; }

        public override void Update(int delta) {
            Timer += (uint) delta;

            if (TriggeredCount > RepeatTimes || Timer < NextActivationTimer) {
                return;
            }

            Action();
            TriggeredCount++;
            NextActivationTimer += Interval;
        }

        public override void Render() {
        }

        public override void DebugRender() {
        }

        public void Reset() {
            TriggeredCount = 0;
            Timer = 0;
            NextActivationTimer = Interval;
        }

        public override void Dispose() {
            base.Dispose();
            Action = null;
        }
    }
}
