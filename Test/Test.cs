using Raccoon;
//using Raccoon.Input;

namespace Test {
    enum ButtonLabel {
        A,
        B,
        C,
        D,
        E,
        F
    };

    class Test {
        static void Main(string[] args) {
            using (Game game = new Game()) {
                game.Scale = 3;
                //p1Controller.AddButton(ControllerMode.Xbox, ButtonLabel.A, new Button(XboxLabel.A));
                game.AddScene(new GameScene());
                game.Start("GameScene");
            }
        }
    }
}
