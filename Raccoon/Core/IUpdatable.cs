namespace Raccoon {
    public interface IUpdatable {
        bool Active { get; set; }
        int Order { get; set; }
        int ControlGroup { get; set; }

        void Update(int delta);
    }
}
