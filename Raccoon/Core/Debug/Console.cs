using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using Raccoon.Graphics;
using Raccoon.Graphics.Primitives;
using Raccoon.Input;
using Raccoon.Util;

namespace Raccoon {
    public class Console : TraceListener {
        #region Public Members

        public const int SpaceBetweenLines = 5,
                         MessagesLimit = 100;

        #endregion Public Members

        #region Private Members

        private List<Message> _messages = new List<Message>();
        private Dictionary<string, Category> _categories = new Dictionary<string, Category>();

        // graphics
        private RectanglePrimitive _background, _scrollGraphic;

        private Vector2 _pageScroll;

        #endregion Private Members

        #region Constructors

        internal Console() {
            RegisterCategory("Critical", (Message msg) => msg.Color = 0xFF6A00FF, showNameAtMessages: false);
            RegisterCategory("Warning", (Message msg) => msg.Color = 0xFFEE00FF, showNameAtMessages: false);
            RegisterCategory("Error", (Message msg) => msg.Color = Color.Red, showNameAtMessages: false);
            RegisterCategory("Info", (Message msg) => msg.Color = 0x00D4FFFF, showNameAtMessages: false);
            PageUpButton = new Button(Key.NumPad9);
            PageDownButton = new Button(Key.NumPad3);
            PageHomeButton = new Button(Key.NumPad7);
            PageEndButton = new Button(Key.NumPad1);
        }

        #endregion Constructors

        #region Public Properties

        public Renderer Renderer { get; private set; }
        public Font Font { get; set; }
        public Rectangle Viewport { get; set; }
        public int Lines { get; private set; }
        public int MessagesCount { get { return _messages.Count; } }
        public float TotalHeight { get { return Lines * (Font?.LineSpacing ?? 0); } } 
        public bool Visible { get; private set; }
        public bool ShowTimestamp { get; set; } = true;
        public bool MergeIdenticalMessages { get; set; } = true;
        public bool AlwaysShowCategory { get; set; }
        public Button PageUpButton { get; set; }
        public Button PageDownButton { get; set; }
        public Button PageHomeButton { get; set; }
        public Button PageEndButton { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void Start() {
            Viewport = new Rectangle(Game.Instance.WindowSize);
            Renderer = Game.Instance.DebugRenderer;

            if (Font == null) {
                Font = Game.Instance.StdFont;
            }

            _background = new RectanglePrimitive(1, 1) {
                Color = Color.Black,
                Renderer = Game.Instance.DebugRenderer,
                Opacity = .25f
            };

            _scrollGraphic = new RectanglePrimitive(3, 1) {
                Color = Color.White,
                Renderer = Game.Instance.DebugRenderer,
                Opacity = .3f
            };

            /*
            _inputBackground = new Graphics.Primitives.Rectangle(Viewport.Width, Font.LineSpacing + 5 * 2, Color.White) {
                Surface = Game.Instance.Core.DebugSurface,
                Opacity = 0.1f
            };

            _inputBackground.Position = new Vector2(0, Viewport.Width - _inputBackground.Height);
            */
        }

        public override void Write(string message, string category) {
            if (message == null) {
                return;
            }

            bool isMessageConsumed = false;

            if (_messages.Count > 0) {
                Message lastMessage = _messages[0];

                if (!lastMessage.IsClosed) {
                    if (IsSameCategory(lastMessage, category)) {
                        int newLines = lastMessage.Append(message);

                        if (newLines > 0) {
                            Lines += newLines;
                        }

                        isMessageConsumed = true;
                    }

                    lastMessage.Close();
                }
            }

            if (!isMessageConsumed) {
                InsertMessage(message, category, ended: false);
            }

            TrimExcess();
            return;
        }

        public override void Write(string message) {
            Write(message, "");
        }

        public override void Write(object obj, string category) {
            Write(obj.ToString(), category);
        }

        public override void Write(object obj) {
            Write(obj.ToString());
        }

        public override void WriteLine(string message, string category) {
            if (message == null) {
                return;
            }

            bool isMessageConsumed = false;

            if (_messages.Count > 0) {
                Message lastMessage = _messages[0];

                // close previous opened messages
                if (!lastMessage.IsClosed) {
                    if (IsSameCategory(lastMessage, category)) {
                        int newLines = lastMessage.Append(message);

                        if (newLines > 0) {
                            Lines += newLines;
                        }

                        isMessageConsumed = true;
                    }

                    lastMessage.Close();
                }

                if (MergeIdenticalMessages && !isMessageConsumed && IsSameMessage(lastMessage, message, category)) {
                    isMessageConsumed = true;
                    lastMessage.Repeat();
                }
            }

            if (!isMessageConsumed) {
                InsertMessage(message, category, ended: true);
            }

            TrimExcess();
        }

        public override void WriteLine(string message) {
            Write(message);
        }

        public override void WriteLine(object obj, string category) {
            Write(obj.ToString(), category);
        }

        public override void WriteLine(object obj) {
            Write(obj.ToString());
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

        public void RegisterCategory(string name, System.Action<Message> styleFormatter, bool showNameAtMessages = false) {
            Category category = new Category(styleFormatter) {
                ShowNameAtMessages = showNameAtMessages
            };

            _categories.Add(name.ToLowerInvariant(), category);
        }

        public void Clear() {
            _messages.Clear();
        }

        #endregion Public Methods

        #region Private Methods

        private void InsertMessage(string text, string category, bool ended) {
            Message msg = new Message(text, closed: ended);
            _messages.Insert(0, msg);
            Lines += msg.Lines;

            if (!string.IsNullOrWhiteSpace(category)) {
                msg.CategoryName = category;
                if (_categories.TryGetValue(category.ToLower(), out Category c)) {
                    c.Format(msg);
                }
            }
        }

        private void TrimExcess() {
            int excessCount = _messages.Count - MessagesLimit;
            if (excessCount <= 0) {
                return;
            }

            for (int i = _messages.Count - 1; i >= _messages.Count - excessCount; i--) {
                Lines -= _messages[i].Lines;
            }

            _messages.RemoveRange(_messages.Count - excessCount, excessCount);
        }

        #endregion Private Methods

        #region Internal Methods

        internal void Update(int delta) {
            if (Viewport.Size != Game.Instance.WindowSize) {
                Viewport = new Rectangle(Game.Instance.WindowSize);
            }

            PageUpButton.Update(delta);
            if (PageUpButton.IsPressed) {
                MoveVerticalScrollLines(-4);
            } else if (PageUpButton.IsDown) {
                MoveVerticalScrollLines(-2);
            }

            PageDownButton.Update(delta);
            if (PageDownButton.IsPressed) {
                MoveVerticalScrollLines(4);
            } else if (PageDownButton.IsDown) {
                MoveVerticalScrollLines(2);
            }

            PageEndButton.Update(delta);

            if (PageEndButton.IsPressed) {
                VerticalScrollTo(0f);
            }
        }

        internal void Render() {
            // background
            _background.Render(Viewport.Position, 0f, Viewport.Size.ToVector2());

            // scroll
            float scrollHeight = Viewport.Height / (TotalHeight / Viewport.Height);
            float scrollY = Math.Map(_pageScroll.Y, Math.Min(0f, -TotalHeight + Viewport.Height), 0f, 0f, Viewport.Height - scrollHeight);

            _scrollGraphic.Render(
                new Vector2(Viewport.Left, Viewport.Top + scrollY),
                0f,
                new Vector2(1f, scrollHeight)
            );

            // messages
            StringBuilder messagesBuilder = new StringBuilder();
            float messagesY = _pageScroll.Y % Font.LineSpacing;

            Color currentColor = Color.White;

            int lineStartIndex = (int) Math.Max(0, Math.Floor(Math.Abs(_pageScroll.Y) / Font.LineSpacing)),
                lineEndIndex = lineStartIndex + (int) Math.Ceiling(Viewport.Height / Font.LineSpacing);

            int linesToSkip = lineStartIndex,
                linesToProcess = lineEndIndex - lineStartIndex;

            float y;

            for (int i = 0; i < _messages.Count && linesToProcess > 0; i++) {
                Message message = _messages[i];

                if (linesToSkip - message.Lines > 0) {
                    linesToSkip -= message.Lines;
                    continue;
                } 

                int lines = message.Lines - linesToSkip;
                linesToProcess -= lines;

                if (linesToSkip > 0) {
                    linesToSkip = 0;
                }

                if (message.Color != currentColor) {
                    y = Math.Map(messagesY, -Viewport.Height, 0f, 0f, Viewport.Height);

                    FlushMessages(
                        messagesBuilder, 
                        Viewport.Position + new Vector2(15f, y), 
                        currentColor
                    );

                    currentColor = message.Color;
                }

                string categoryName = "";

                if (!string.IsNullOrWhiteSpace(message.CategoryName) && (!_categories.TryGetValue(message.CategoryName.ToLowerInvariant(), out Category cat) || cat.ShowNameAtMessages)) {
                    categoryName = message.CategoryName;
                }

                messagesBuilder.Insert(
                    0,
                    string.Format(
                        "{0}{1}{2} {3}\n", 
                        ShowTimestamp ? message.Timestamp.ToString("HH:mm:ss").PadRight(10) : "",
                        string.IsNullOrEmpty(categoryName) ? "" : $"[{categoryName}]  ",
                        message.Text,
                        message.Count > 1 ? $"[{message.Count.ToString()}]" : ""
                    )
                );

                messagesY -= lines * Font.LineSpacing;
            }

            // flush remaining messages
            y = Math.Map(messagesY, -Viewport.Height, 0f, 0f, Viewport.Height);

            FlushMessages(
                messagesBuilder, 
                Viewport.Position + new Vector2(15f, y), 
                currentColor
            );
        }

        #endregion Internal Methods

        #region Private Methods

        private void FlushMessages(StringBuilder messagesBuilder, Vector2 position, Color color) {
            Renderer.DrawString(
                Font,
                messagesBuilder.ToString(),
                position,
                0f,
                Vector2.One,
                ImageFlip.None,
                color,
                Vector2.Zero,
                Vector2.One
            );

            messagesBuilder.Clear();
        }

        private void VerticalScrollTo(float y) {
            _pageScroll = new Vector2(_pageScroll.X, Math.Clamp(y, Math.Min(-TotalHeight + Viewport.Height, 0), 0f));
        }

        private void MoveVerticalScroll(float y) {
            VerticalScrollTo(_pageScroll.Y + y);
        }

        private void MoveVerticalScrollLines(int lines) {
            MoveVerticalScroll(Font.LineSpacing * lines);
        }

        private bool IsSameMessage(Message message, string text, string category) {
            return IsSameCategory(message, category) && message.Text.Equals(text);
        }

        private bool IsSameCategory(Message message, string category) {
            if (string.IsNullOrWhiteSpace(message.CategoryName)) {
                if (string.IsNullOrWhiteSpace(category)) {
                    return true;
                }
            } else if (message.CategoryName.Equals(category, System.StringComparison.InvariantCultureIgnoreCase)) {
                return true;
            }

            return false;
        }

        #endregion Private Methods

        #region Class Message

        public class Message {
            public Message(string text, bool closed) {
                Text = text;
                Timestamp = System.DateTime.Now;
                Lines = 1 + Text.Count("\n");
                IsClosed = closed;
            }

            public string Text { get; private set; }
            public Color Color { get; set; } = Color.White;
            public int Count { get; private set; } = 1;
            public System.DateTime Timestamp { get; private set; }
            public int Lines { get; private set; } = 1;
            public bool IsMultiline { get { return Lines > 1; } }
            public string CategoryName { get; set; }
            public bool IsClosed { get; private set; }

            public void Repeat() {
                if (!IsClosed) {
                    return;
                }

                Count++;
                Timestamp = System.DateTime.Now;
            }

            public int Append(string text) {
                if (IsClosed) {
                    return 0;
                }

                int newLines = text.Count("\n");
                Lines += newLines;
                Text += text;
                Timestamp = System.DateTime.Now;
                return newLines;
            }

            public void Close() {
                if (IsClosed) {
                    return;
                }

                IsClosed = true;
            }
        }

        #endregion Class Message

        #region Class Category

        public class Category {
            public Category(System.Action<Message> format) {
                Format = format;
            }

            public System.Action<Message> Format { get; }
            public bool ShowNameAtMessages { get; set; }
        }

        #endregion Class Category
    }
}
