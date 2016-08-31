using Raccoon;
using Raccoon.Graphics;
using Raccoon.Graphics.Transition;
using Raccoon.Input;

namespace Test {
    class GameScene : Scene {
        Animation<string> anim;
        Controller p1Controller;
        SceneTransition transition;
        Text text, text2, text3;
        Font font;
        Shader shader;

        public GameScene() : base() {
            anim = new Animation<string>("graphics/deathflower", 18, 20);
            anim.Add("idle", "0-6", 0.120f);
            anim.Play("idle");
            anim.X = Game.Instance.Width / 2;
            anim.Y = Game.Instance.Height / 2;
            anim.Origin = new Vector2(9, 10);
            AddGraphic(anim);
            //Console.WriteLine($"Angle = 180 | Rad = {MathHelper.ToRadian(180)} | Cos = {Math.Cos(MathHelper.ToRadian(180))}");

            p1Controller = new XboxController(0, 0);
            p1Controller.AddButton(ButtonLabel.A, new Button(Key.Z));
            p1Controller.AddButton(ButtonLabel.B, new Button(Key.X));
            p1Controller.AddButton(ButtonLabel.C, new Button(Key.C));
            Axis axis = p1Controller.Axis(XboxController.Label.LeftStick);
            axis.DeadZone = 0.25f;
            p1Controller.Connect();

            transition = new FadeTransition(Color.Cyan);
            AddGraphic(transition);

            font = new Font("m5x7");
            text = new Text("the quick brown fox", font, new Color(0x02dbdbff)) { Position = new Vector2(100, 100), Rotation = 2 };
            text2 = new Text("jumps over", font, new Color(0x026edbff)) { Position = new Vector2(150, 100), Rotation = 1 };
            text3 = new Text("the lazy dog", font, new Color(0x02db6eff)) { Position = new Vector2(175, 75), Rotation = 3 };

            shader = new Shader("Test");
            shader.CurrentTechnique = "BasicColorDrawing";
        }

        public override void Update(int delta) {
            base.Update(delta);
            p1Controller.Update(delta);

            if (p1Controller.Button(ButtonLabel.A).Pressed) {
                transition.Play(1);
            }

            if (p1Controller.Button(ButtonLabel.B).Pressed) {
                (transition as FadeTransition).Play(Fade.In);
            }

            if (p1Controller.Button(ButtonLabel.C).Pressed) {
                (transition as FadeTransition).Play(Fade.Out);
            }

            //Axis axis = p1Controller.Axis(XboxController.Label.LeftStick);
            //System.Console.WriteLine($"X: {axis.X}, Y: {axis.Y}, DeadZoneMode: {axis.DeadZoneMode}, DeadZone: {axis.DeadZone}");
            //System.Console.WriteLine($"Mouse | ScreenX: {Mouse.ScreenX}, Y: {Mouse.ScreenY}, GameX: {Mouse.GameX}, GameY: {Mouse.GameY}, ScrollWheel: {Mouse.ScrollWheel}, ScrollWheelDelta: {Mouse.ScrollWheelDelta}\nLeftButtonDown? {Mouse.IsButtonDown(Mouse.Button.Left)}, RightButtonDown? {Mouse.IsButtonDown(Mouse.Button.Right)}, MiddleButtonDown? {Mouse.IsButtonDown(Mouse.Button.Middle)}\nM4ButtonDown? {Mouse.IsButtonDown(Mouse.Button.M4)}, M5ButtonDown? {Mouse.IsButtonDown(Mouse.Button.M5)}");
            //System.Console.WriteLine($"A: {p1Controller.Button(ButtonLabel.A).Pressed} B: {p1Controller.Button(XboxController.Label.B).Pressed}");
        }

        public override void Render() {
            base.Render();
            //shader.Apply();
            Debug.DrawText("oie, test msg", new Vector2(50, 50), Color.Blue);
            text.Draw();
            text2.Draw();
            text3.Draw();
        }
    }
}
