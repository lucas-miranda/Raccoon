namespace Raccoon {
    public interface IUpdatable : IPausable {
        bool Active { get; set; }
        int Order { get; set; }

        void Update(int delta);
    }
}
