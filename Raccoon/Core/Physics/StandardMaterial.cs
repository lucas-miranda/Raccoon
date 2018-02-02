namespace Raccoon {
    public class StandardMaterial : IMaterial {
        public float Density { get; } = 0.6f;
        public float Restitution { get; } = 1f; //0.2f;
    }
}
