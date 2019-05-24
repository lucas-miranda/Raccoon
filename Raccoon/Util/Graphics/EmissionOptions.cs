namespace Raccoon.Util.Graphics {
    public class EmissionOptions {
        public string AnimationKey { get; set; }

        public int MinCount { get; set; } = 1;
        public int MaxCount { get; set; } = 1;
        public int Count { get { return MaxCount; } set { MinCount = MaxCount = value; } }
        public Vector2 DisplacementMin { get; set; }
        public Vector2 DisplacementMax { get; set; }
        public uint DurationMin { get; set; }
        public uint DurationMax { get; set; }
        public uint Duration { get { return DurationMax; } set { DurationMin = DurationMax = value; } }
        public uint DelayBetweenEmissions { get; set; }
    }
}
