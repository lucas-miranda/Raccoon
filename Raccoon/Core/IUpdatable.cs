namespace Raccoon {
    public interface IUpdatable {
        bool Active { get; set; }
        int Order { get; set; }

        void Update(int delta);
    }
}
