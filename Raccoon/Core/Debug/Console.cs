using System.Collections.Generic;
using System.Diagnostics;

using Raccoon.Graphics;
using Raccoon.Graphics.Primitives;
using Raccoon.Input;
using Raccoon.Util;

namespace Raccoon {
    public class Console : TraceListener {
        #region Public Static Members

        public const int SpaceBetweenLines = 5,
                         MessagesLimit = 100;

        #endregion Public Static Members

        #region Private Members

        private List<Message> _messages = new List<Message>();
        private Dictionary<string, System.Action<Message>> _categoriesFormatter = new Dictionary<string, System.Action<Message>>();

        // graphics
        private RectanglePrimitive _background;

        private Vector2 _pageScroll;

        #endregion Private Members

        #region Constructors

        internal Console() {
            RegisterCategory("Critical", (Message msg) => msg.Color = 0xFF6A00FF);
            RegisterCategory("Warning", (Message msg) => msg.Color = 0xFFEE00FF);
            RegisterCategory("Error", (Message msg) => msg.Color = Color.Red);
            RegisterCategory("Info", (Message msg) => msg.Color = 0x00D4FFFF);
            PageUpButton = new Button(Key.NumPad9);
            PageDownButton = new Button(Key.NumPad3);
            PageHomeButton = new Button(Key.NumPad7);
            PageEndButton = new Button(Key.NumPad1);
        }

        #endregion Constructors

        #region Public Properties

        public Renderer Renderer { get; private set; }
        public bool Visible { get; private set; }
        public Font Font { get; set; }
        public bool ShowTimestamp { get; set; } = true;
        public bool MergeIdenticalMessages { get; set; } = true;
        public Button PageUpButton { get; set; }
        public Button PageDownButton { get; set; }
        public Button PageHomeButton { get; set; }
        public Button PageEndButton { get; set; }

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
            if (MergeIdenticalMessages && isDefaultCategory 
              && _messages.Count > 0 && _messages[0].Text == message) {
                msg = _messages[0];
                msg.Repeat();
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

        public void RegisterCategory(string name, System.Action<Message> styleFormatter) {
            _categoriesFormatter.Add(name, styleFormatter);
        }

        public void Clear() {
            _messages.Clear();
        }

        #endregion Public Methods

        #region Internal Methods

        internal void Update(int delta) {
            PageUpButton.Update();
            if (PageUpButton.IsPressed) {
                _pageScroll -= new Vector2(0, Font.LineSpacing * 4f);
            } else if (PageUpButton.IsDown) {
                _pageScroll -= new Vector2(0, Font.LineSpacing * 2f);
            }

            PageDownButton.Update();
            if (PageDownButton.IsPressed) {
                _pageScroll = Math.Approach(_pageScroll, Vector2.Zero, new Vector2(0, Font.LineSpacing * 4f));
            } else if (PageDownButton.IsDown) {
                _pageScroll = Math.Approach(_pageScroll, Vector2.Zero, new Vector2(0, Font.LineSpacing * 2f));
            }

            PageEndButton.Update();
            if (PageEndButton.IsPressed) {
                _pageScroll = Vector2.Zero;
            }
        }

        internal void Render() {
            _background.Render(Vector2.Zero, 0f, Game.Instance.WindowSize.ToVector2());

            // total messages
            Renderer.DrawString(
                Font, 
                _messages.Count.ToString(), 
                Renderer.ConvertScreenToWorld(new Vector2(Game.Instance.WindowWidth - 25, 15)), 
                0f, 
                Vector2.One, 
                ImageFlip.None, 
                Color.White, 
                Vector2.Zero, 
                Vector2.One
            );

            // messages
            Vector2 totalMessagesPos = Vector2.Zero;
            Range pageHeightRange = new Range(_pageScroll.Y - Game.Instance.WindowHeight, _pageScroll.Y);

            for (int i = 0; i < _messages.Count; i++) {
                Message message = _messages[i];
                float messageHeight = message.Lines * (Font.LineSpacing + SpaceBetweenLines);
                totalMessagesPos -= new Vector2(0f, messageHeight);

                Range messageHeightRange = new Range(totalMessagesPos.Y, totalMessagesPos.Y + messageHeight);
                if (!pageHeightRange.Overlaps(messageHeightRange)) {
                    if (messageHeightRange.Max < pageHeightRange.Min) {
                        break;
                    }

                    continue;
                }

                Renderer.DrawString(
                    Font, 
                    (ShowTimestamp ? message.Timestamp.ToString("HH:mm:ss").PadRight(10) : "") + (message.Count == 1 ? message.Text : $"{message.Text} [{message.Count}]"), 
                    new Vector2(15f, totalMessagesPos.Y - pageHeightRange.Min), 
                    0f, 
                    Vector2.One,
                    ImageFlip.None,
                    message.Color,
                    Vector2.Zero,
                    Vector2.One                                  
                );
            }
        }

        #endregion Internal Methods

        #region Class Message

        public class Message {
            public Message(string text) {
                Text = text;
                Timestamp = System.DateTime.Now;
                Lines = 1 + Text.Count("\n");
            }

            public string Text { get; private set; }
            public Color Color { get; set; } = Color.White;
            public int Count { get; private set; } = 1;
            public System.DateTime Timestamp { get; private set; }
            public int Lines { get; private set; } = 1;
            public bool IsMultiline { get { return Lines > 1; } }

            public void Repeat() {
                Count++;
                Timestamp = System.DateTime.Now;
            }
        }

        #endregion Class Message
    }
}
