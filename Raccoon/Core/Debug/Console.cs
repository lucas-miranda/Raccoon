using System;
using System.Collections.Generic;
using System.Diagnostics;

using Raccoon.Graphics;
using Raccoon.Graphics.Primitives;

namespace Raccoon {
    public class Console : TraceListener {
        #region Public Static Members

        public const int SpaceBetweenLines = 5,
                         MessagesLimit = 100;

        #endregion Public Static Members

        #region Private Members

        private List<Message> _messages = new List<Message>();
        private Dictionary<string, Action<Message>> _categoriesFormatter = new Dictionary<string, Action<Message>>();

        // graphics
        private RectanglePrimitive _background;

        #endregion Private Members

        #region Constructors

        internal Console() {
            RegisterCategory("Critical", (Message msg) => msg.Color = 0xFF6A00FF);
            RegisterCategory("Warning", (Message msg) => msg.Color = 0xFFEE00FF);
            RegisterCategory("Error", (Message msg) => msg.Color = Color.Red);
            RegisterCategory("Info", (Message msg) => msg.Color = 0x00D4FFFF);
        }

        #endregion Constructors

        #region Public Properties

        public Renderer Renderer { get; private set; }
        public bool Visible { get; private set; }
        public Font Font { get; set; }
        public bool ShowTimestamp { get; set; } = true;
        public bool MergeIdenticalMessages { get; set; } = true;

        #endregion Public Properties

        #region Public Methods

        public void Start() {
            Renderer = Game.Instance.DebugRenderer;
            if (Font == null) {
                Font = Game.Instance.StdFont;
            }

            _background = new RectanglePrimitive(1, 1) {
                Color = Color.Black,
                Renderer = Game.Instance.DebugRenderer,
                Opacity = .25f
            };

            /*_inputBackground = new Graphics.Primitives.Rectangle(Game.Instance.WindowWidth, Font.LineSpacing + 5 * 2, Color.White) {
                Surface = Game.Instance.Core.DebugSurface,
                Opacity = 0.1f
            };

            _inputBackground.Position = new Vector2(0, Game.Instance.WindowHeight - _inputBackground.Height);*/
        }

        public override void Write(string message) {
            Write(message, "");
        }

        public override void Write(string message, string category) {
            if (string.IsNullOrWhiteSpace(message)) {
                return;
            }

            bool isDefaultCategory = string.IsNullOrWhiteSpace(category);

            Message msg = null;
            if (MergeIdenticalMessages && isDefaultCategory) {
                if (_messages.Count > 0 && _messages[0].Text == message) {
                    msg = _messages[0];
                    msg.Repeat();
                }
            }

            if (msg == null) {
                msg = new Message(message);
                _messages.Insert(0, msg);
            }

            if (!isDefaultCategory) {
                _categoriesFormatter[category](msg);
            }

            int excessCount = _messages.Count - MessagesLimit;
            if (excessCount <= 0) {
                return;
            }

            _messages.RemoveRange(_messages.Count - excessCount, excessCount);
        }

        public override void Write(object obj, string category) {
            Write(obj.ToString(), category);
        }

        public override void Write(object obj) {
            Write(obj.ToString());
        }

        public override void WriteLine(string message) {
            Write(message);
        }

        public override void WriteLine(string message, string category) {
            Write(message, category);
        }

        public override void WriteLine(object obj) {
            Write(obj.ToString());
        }
        public override void WriteLine(object obj, string category) {
            Write(obj.ToString(), category);
        }

        public void Show() {
            Visible = true;
        }

        public void Hide() {
            Visible = false;
        }

        public void Toggle() {
            if (Visible) {
                Hide();
                return;
            }

            Show();
        }

        public void RegisterCategory(string name, Action<Message> styleFormatter) {
            _categoriesFormatter.Add(name, styleFormatter);
        }

        public void Clear() {
            _messages.Clear();
        }

        #endregion Public Methods

        #region Internal Methods

        internal void Update(int delta) {
        }

        internal void Render() {
            Vector2 scale = Vector2.One;

            if (Camera.Current != null) {
                scale = new Vector2(Renderer.PixelScale / (Camera.Current.Zoom * Game.Instance.MainRenderer.PixelScale));
            }

            _background.Render(Renderer.ConvertScreenToWorld(Vector2.Zero), 0f, Game.Instance.WindowSize.ToVector2());

            // total messages
            Renderer.DrawString(
                Font, 
                _messages.Count.ToString(), 
                Renderer.ConvertScreenToWorld(new Vector2(Game.Instance.WindowWidth - 25, 15)), 
                0f, 
                scale, 
                ImageFlip.None, 
                Color.White, 
                Vector2.Zero, 
                Vector2.One
            );

            // messages
            Vector2 messagePos = new Vector2(15, Game.Instance.WindowHeight - Font.LineSpacing);
            foreach (Message message in _messages) {
                 messagePos += new Vector2(0, message.Lines * (-Font.LineSpacing - SpaceBetweenLines));
                
                 Renderer.DrawString(
                    Font, 
                    (ShowTimestamp ? message.Timestamp.ToString("HH:mm:ss").PadRight(10) : "") + (message.Count == 1 ? message.Text : $"{message.Text} [{message.Count}]"), 
                    Renderer.ConvertScreenToWorld(messagePos), 
                    0f, 
                    scale,
                    ImageFlip.None,
                    message.Color,
                    Vector2.Zero,
                    Vector2.One                                  
                );

                if (messagePos.Y <= -Font.LineSpacing) {
                    break;
                }
            }
        }

        #endregion Internal Methods

        #region Class Message

        public class Message {
            public Message(string text) {
                Text = text;
                Timestamp = DateTime.Now;
                Lines = 1 + Text.Count("\n");
            }

            public string Text { get; private set; }
            public Color Color { get; set; } = Color.White;
            public int Count { get; private set; } = 1;
            public DateTime Timestamp { get; private set; }
            public int Lines { get; private set; } = 1;
            public bool IsMultiline { get { return Lines > 1; } }

            public void Repeat() {
                Count++;
                Timestamp = DateTime.Now;
            }
        }

        #endregion Class Message
    }
}
