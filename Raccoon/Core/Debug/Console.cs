using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Graphics.Primitives;
using Raccoon.Input;
using Raccoon.Util;

namespace Raccoon {
    public class Console : ILoggerListener {
        #region Public Members

        public const int SpaceBetweenLines = 5,
                         MessagesLimit = 100;

        #endregion Public Members

        #region Private Members

        private List<Message> _messages = new List<Message>();
        private Dictionary<string, Context> _contexts;

        // graphics
        private RectanglePrimitive _background, _scrollGraphic;

        private Vector2 _pageScroll;

        #endregion Private Members

        #region Constructors

        internal Console() {
            _contexts = new Dictionary<string, Context> {
                { 
                    "timestamp",
                    new Context("timestamp", new Color(0xA3A3A3FF))
                },
                { 
                    "error",
                    new Context("error", Color.Red)
                },
                { 
                    "info",
                    new Context("info", new Color(0x00D4FFFF))
                },
                { 
                    "warning",
                    new Context("warning", new Color(0xFFEE00FF))
                },
                { 
                    "critical",
                    new Context("critical", new Color(0xFF6A00FF))
                },
                { 
                    "success",
                    new Context("success", Color.Green)
                },
                { 
                    "subject-name",
                    new Context("subject-name", new Color(0xA3A3A3FF))
                },
            };

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
        public bool IsDisposed { get; private set; }
        public Button PageUpButton { get; set; }
        public Button PageDownButton { get; set; }
        public Button PageHomeButton { get; set; }
        public Button PageEndButton { get; set; }

        public Message LastMessage { 
            get {
                if (_messages.Count == 0) {
                    return null;
                }

                return _messages[0];
            }
        }

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

        public void Write(string context, string message) {
            if (message == null) {
                throw new System.ArgumentNullException("message");
            }

            Message lastMessage = LastMessage;
            switch (context) {
                case "start-message":
                    CloseLastMessage();
                    _messages.Add(new Message());
                    return;

                case "timestamp":
                    EnsureLastMessageIsOpen();

                    if (_contexts.TryGetValue("timestamp", out Context timestampContext)) {
                        lastMessage.SetOrAppend(timestampContext, message);
                    } else {
                        lastMessage.Append(Context.None, message);
                    }

                    return;

                default:
                    EnsureLastMessageIsOpen();
                    break;
            }

            if (_contexts.TryGetValue(context, out Context c)) {
                Lines += lastMessage.Append(c, message);
            } else {
                Lines += lastMessage.Append(Context.None, message);
            }

            TrimExcess();
            return;

            void EnsureLastMessageIsOpen() {
                if (lastMessage == null) {
                    lastMessage = new Message();
                    _messages.Add(lastMessage);
                } else if (lastMessage.IsClosed) {
                    lastMessage = new Message();
                    _messages.Insert(0, lastMessage);
                }
            }
        }

        public void Write(string message) {
            Write(string.Empty, message);
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

        public void RegisterContext(Context context, bool overrides = false) {
            if (overrides) {
                _contexts[context.Name] = context;
                return;
            }

            _contexts.Add(context.Name, context);
        }

        public void ClearContexts() {
            _contexts.Clear();
        }

        public void ClearMessages() {
            _messages.Clear();
        }

        public void RemoveMessage(int index) {
            if (index < 0 || index >= _messages.Count) {
                throw new System.ArgumentException($"Invalid index '{index}'. (Messages Count: {_messages.Count})");
            }

            Message message = _messages[index];
            Lines -= message.Lines;
            _messages.RemoveAt(index);
        }

        public void RemoveLastMessage() {
            if (_messages.Count == 0) {
                throw new System.InvalidOperationException("Messages are empty.");
            }

            Message lastMessage = _messages[0];
            Lines -= lastMessage.Lines;
            _messages.RemoveAt(0);
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            foreach (Message message in _messages) {
                message.Dispose();
            }

            _messages.Clear();
            _contexts.Clear();

            _background.Dispose();
            _scrollGraphic.Dispose();
            _background = null;
            _scrollGraphic = null;

            Renderer = null;

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Private Methods

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
            float messagesY = _pageScroll.Y % Font.LineSpacing;

            Color currentColor = Color.White;

            int lineStartIndex = (int) Math.Max(0, Math.Floor(Math.Abs(_pageScroll.Y) / Font.LineSpacing)),
                lineEndIndex = lineStartIndex + (int) Math.Ceiling(Viewport.Height / Font.LineSpacing);

            int linesToSkip = lineStartIndex,
                linesToProcess = lineEndIndex - lineStartIndex;

            float y;

            // help perfectly align contexts one above each other
            float whitespaceX = Font.MeasureText(" ").X;

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

                y = Math.Map(messagesY, -Viewport.Height, 0f, 0f, Viewport.Height) - lines * Font.LineSpacing;
                float x = Viewport.X + 60f;

                for (int j = 0; j < message.Sections; j++) {
                    (Context Context, string Text) textEntry = message[j];

                    if (textEntry.Context.Name == "spacing") {
                        x += whitespaceX * textEntry.Text.Length;
                        continue;
                    } else if (textEntry.Text.Length == 0) {
                        continue;
                    }

                    Renderer.DrawString(
                        Font,
                        textEntry.Text,
                        new Vector2(x, y), 
                        rotation: 0f,
                        scale: Vector2.One,
                        ImageFlip.None,
                        textEntry.Context.TextColor,
                        origin: Vector2.Zero,
                        scroll: Vector2.One
                    );

                    if (!textEntry.Text.Contains("\n")) {
                        x += Font.MeasureText(textEntry.Text).X;
                    }
                }

                // message repeat count
                if (MergeIdenticalMessages && message.Count > 1) {
                    string countText = message.Count > 999 ? "[999+]" : $"[{message.Count}]";
                    x = Viewport.X + (60f - Font.MeasureText(countText).X) / 2f;

                    if (!_contexts.TryGetValue("repeat-count", out Context repeatCountContext)) {
                        repeatCountContext = Context.None;
                    }

                    Renderer.DrawString(
                        Font,
                        countText,
                        new Vector2(x, y), 
                        rotation: 0f,
                        scale: Vector2.One,
                        ImageFlip.None,
                        repeatCountContext.TextColor,
                        origin: Vector2.Zero,
                        scroll: Vector2.One
                    );
                }

                messagesY -= lines * Font.LineSpacing;
            }
        }

        #endregion Internal Methods

        #region Private Methods

        private void VerticalScrollTo(float y) {
            _pageScroll = new Vector2(_pageScroll.X, Math.Clamp(y, Math.Min(-TotalHeight + Viewport.Height, 0), 0f));
        }

        private void MoveVerticalScroll(float y) {
            VerticalScrollTo(_pageScroll.Y + y);
        }

        private void MoveVerticalScrollLines(int lines) {
            MoveVerticalScroll(Font.LineSpacing * lines);
        }

        private void CloseLastMessage() {
            Message lastMessage = LastMessage;
            if (lastMessage == null) {
                return;
            }

            lastMessage.Close();

            if (!MergeIdenticalMessages || _messages.Count < 2) {
                return;
            }
            
            Message secondLastMessage = _messages[1];
            if (secondLastMessage.Equals(lastMessage)) {
                // merge
                secondLastMessage.Repeat(lastMessage.GetTextByContext("timestamp"));
                RemoveLastMessage();
            }
        }

        #endregion Private Methods

        #region Class Message

        public class Message : System.IEquatable<Message>, System.IDisposable {
            private List<(Context Context, string Text)> _text = new List<(Context, string)>();

            public Message() {
            }

            public int Count { get; private set; } = 1;
            public int Lines { get; private set; }
            public int Sections { get { return _text.Count; } }
            public bool IsMultiline { get { return Lines > 1; } }
            public bool IsClosed { get; private set; }
            public bool IsOpened { get { return !IsClosed; } }
            public bool IsDisposed { get; private set; }

            public (Context Context, string Text) this [int index] {
                get {
                    return _text[index];
                }
            }

            public void Repeat(string newTimestamp) {
                if (!IsClosed) {
                    return;
                }

                if (newTimestamp == null) {
                    throw new System.ArgumentNullException(nameof(newTimestamp));
                }

                for (int i = 0; i < _text.Count; i++) {
                    (Context Context, string Text) textEntry = _text[i];
                    if (textEntry.Context.Name == "timestamp") {
                        _text[i] = (textEntry.Context, newTimestamp);
                        break;
                    }
                }

                Count++;
            }

            public int Append(Context context, string text) {
                if (IsClosed) {
                    return 0;
                }

                int newLines = text.Count("\n");
                Lines += newLines;

                if (_text.Count > 0 && _text[_text.Count - 1].Context.Equals(context)) {
                    (Context Context, string Text) textEntry = _text[_text.Count - 1];
                    _text[_text.Count - 1] = (textEntry.Context, textEntry.Text + text);
                } else {
                    _text.Add((context, text));
                }

                return newLines;
            }

            public void SetOrAppend(Context context, string text) {
                if (IsClosed) {
                    return;
                }

                for (int i = 0; i < _text.Count; i++) {
                    (Context Context, string Text) textEntry = _text[i];
                    if (textEntry.Context.Equals(context)) {
                        _text[i] = (textEntry.Context, text);
                        // TODO  recalculate lines
                        return;
                    }
                }

                Append(context, text);
            }

            public bool GetLastEntry(out (Context Context, string Text)? lastEntry) {
                if (_text.Count == 0) {
                    lastEntry = null;
                    return false;
                }

                lastEntry = _text[_text.Count - 1];
                return true;
            }

            public string GetTextByContext(string context) {
                foreach ((Context Context, string Text) entry in _text) {
                    if (entry.Context.Name == context) {
                        return entry.Text;
                    }
                }

                return null;
            }

            public void Close() {
                if (IsClosed) {
                    return;
                }

                IsClosed = true;
            }

            public IEnumerator<(Context Context, string Text)> GetEnumerator() {
                return _text.GetEnumerator();
            }

            public bool Equals(Message message) {
                if (message._text.Count != _text.Count) {
                    return false;
                }

                for (int i = 0; i < _text.Count; i++) {
                    (Context Context, string Text) text = _text[i],
                                                   otherText = message._text[i];

                    if (!text.Context.Equals(otherText.Context)) {
                        return false;
                    }

                    if (text.Context.Equals(Context.None) && text.Text != otherText.Text) {
                        return false;
                    }
                }

                return true;
            }

            public override bool Equals(object obj) {
                return obj is Message message && Equals(message);
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }

            public void Dispose() {
                if (IsDisposed) {
                    return;
                }

                _text.Clear();

                IsDisposed = true;
            }
        }

        #endregion Class Message

        #region Class Context

        public class Context : System.IEquatable<Context> {
            public static readonly Context None = new Context("none", Color.White);

            public Context(string name, Color textColor) {
                Name = name;
                TextColor = textColor;
            }

            public string Name { get; private set; }
            public Color TextColor { get; private set; }

            public bool Equals(Context context) {
                return context.Name == Name;
            }

            public override bool Equals(object obj) {
                return obj is Context context && context.Name == Name;
            }

            public override int GetHashCode() {
                return Name.GetHashCode();
            }
        }

        #endregion Class Category
    }
}
