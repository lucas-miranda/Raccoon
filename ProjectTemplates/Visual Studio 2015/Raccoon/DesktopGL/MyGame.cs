using Raccoon;

namespace $safeprojectname$ {
    public static class $safeprojectname$ {
        static void Main() {
            using (var game = new Game("$safeprojectname$")) {
                game.AddScene(new GameplayScene());
                game.Start<GameplayScene>();
            }
        }
    }
}
