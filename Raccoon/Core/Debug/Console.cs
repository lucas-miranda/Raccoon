using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Graphics.Primitives;
using Raccoon.Log;
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
        private Dictionary<System.Type, TextFormatter> _contexts;
        private Dictionary<string, TextFormatter> _categories;

        // graphics
        private RectanglePrimitive _background, _scrollGraphic;

        // scroll
        private Vector2 _pageScroll;

        // tokens
        private List<ConsoleLoggerToken> _previousTokens = new List<ConsoleLoggerToken>();

        #endregion Private Members

        #region Constructors

        internal Console() {
            _contexts = new Dictionary<System.Type, TextFormatter> {
                { 
                    typeof(TimestampLoggerToken),
                    new TextFormatter(new Color(0xA3A3A3FF))
                },
                { 
                    typeof(SubjectsLoggerToken),
                    new TextFormatter(new Color(0xA3A3A3FF))
                }
            };

            _categories = new Dictionary<string, TextFormatter> {
                { 
                    "error",
                    new TextFormatter(Color.Red)
                },
                { 
                    "info",
                    new TextFormatter(new Color(0x00D4FFFF))
                },
                { 
                    "warning",
                    new TextFormatter(new Color(0xFFEE00FF))
                },
                { 
                    "critical",
                    new TextFormatter(new Color(0xFF6A00FF))
                },
                { 
                    "success",
                    new TextFormatter(Color.Green)
                }
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

        public void WriteTokens(in MessageLoggerTokenTree tokens) {
            if (tokens == null) {
                throw new System.ArgumentNullException(nameof(tokens));
            }

            Message lastMessage = LastMessage;
            if (tokens.HeaderToken == null) {
                if (tokens.TextToken == null) {
                    return;
                }

                if (lastMessage == null || lastMessage.IsClosed) {
                    lastMessage = PushEmptyMessage();
                }

                lastMessage.AppendToken(tokens.TextToken, out int newLines);

                if (newLines > 0) {
                    Lines += newLines;
                }

                return;
            }

            // check if last message is the same
            if (lastMessage != null) {
                if (lastMessage.Equals(tokens)) {
                    lastMessage.Repeat(tokens.HeaderToken.TimestampToken);
                    lastMessage.Close();
                    return;
                } else if (lastMessage.IsOpened) {
                    lastMessage.Close();
                }
            }

            lastMessage = PushEmptyMessage();
            List<LoggerToken> tokenList = tokens.Decompose();
            foreach (LoggerToken token in tokenList) {
                if (token == null) {
                    continue;
                }

                lastMessage.AppendToken(token, out int newLines);

                if (newLines > 0) {
                    Lines += newLines;
                }
            }

            TrimExcess();
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

        /*
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
        */

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

            if (_messages.Count == 0) {
                return;
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

            if (_messages.Count == 0) {
                return;
            }

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

            float y = Math.Map(messagesY, -Viewport.Height, 0f, 0f, Viewport.Height);

            // find viewable messages
            int startIndex = -1,
                endIndex = -1;

            for (int i = 0; i < _messages.Count; i++) {
                Message message = _messages[i];

                if (linesToSkip - message.Lines > 0) {
                    linesToSkip -= message.Lines;
                    continue;
                } 

                int lines = message.Lines - linesToSkip;
                linesToProcess -= lines;
                y -= lines * Font.LineSpacing;

                if (linesToSkip > 0) {
                    linesToSkip = 0;
                }

                if (endIndex < 0) {
                    endIndex = i;
                }

                if (linesToProcess <= 0) {
                    startIndex = i;
                    break;
                }
            }

            float x;
            for (int i = startIndex; i >= endIndex; i--) {
                Message message = _messages[i];
                x = Viewport.X + 60f;
                
                RenderMessage(new Vector2(x, y), message);

                // message repeat count
                if (MergeIdenticalMessages && message.Count > 1) {
                    string countText = message.Count > 999 ? "[999+]" : $"[{message.Count}]";
                    x = Viewport.X + (60f - Font.MeasureText(countText).X) / 2f;

                    Renderer.DrawString(
                        Font,
                        countText,
                        new Vector2(x, y), 
                        rotation: 0f,
                        scale: Vector2.One,
                        ImageFlip.None,
                        Color.White,
                        origin: Vector2.Zero,
                        scroll: Vector2.One
                    );
                }

                y += message.Lines * Font.LineSpacing;
            }

            _previousTokens.Clear();
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

        private Message PushEmptyMessage() {
            Message message = new Message();
            _messages.Insert(0, message);
            return message;
        }

        private void RenderMessage(Vector2 topLeft, Message message) {
            Vector2 localPos = Vector2.Zero;

            // check if need to force position recalculation
            if (message.HasTokenType<SubjectsLoggerToken>()) {
                _previousTokens.Clear();
            }

            int previousTokenIndex = 0;
            for (int i = 0; i < message.TokensCount; i++) {
                ConsoleLoggerToken token = message[i];
                string representation = token.Representation;
                Color textColor = Color.White;

                if (string.IsNullOrEmpty(representation)) {
                    continue;
                }

                if (token.LoggerToken is CategoryLoggerToken categoryToken
                 && _categories.TryGetValue(categoryToken.CategoryName, out TextFormatter categoryFormatter)) {
                    textColor = categoryFormatter.TextColor;
                } else if (_contexts.TryGetValue(token.LoggerToken.GetType(), out TextFormatter contextFormatter)) {
                    textColor = contextFormatter.TextColor; }

                if (token.TextSize == null) {
                    token.CalculateTextSize(Font);
                }

                if (_previousTokens.Count > previousTokenIndex) {
                    int previousTokenStartIndex = previousTokenIndex;
                    ConsoleLoggerToken previousToken = _previousTokens[previousTokenIndex];

                    while (!previousToken.IsLoggerType(token.LoggerToken)) {
                        previousTokenIndex += 1;

                        if (previousTokenIndex >= _previousTokens.Count) {
                            break;
                        }

                        previousToken = _previousTokens[previousTokenIndex];
                    }

                    if (previousTokenIndex < _previousTokens.Count) {
                        if (previousTokenIndex - 1 >= 0) {
                            for (int j = previousTokenStartIndex - 1; j <= previousTokenIndex - 1; j++) {
                                localPos.X += _previousTokens[j].TextSize.GetValueOrDefault().X + 16f;
                            }
                        }

                        if (previousToken.Representation.Length != token.Representation.Length) {
                            _previousTokens.RemoveRange(previousTokenIndex, _previousTokens.Count - previousTokenIndex);
                            _previousTokens.Add(token);
                        }
                    } else {
                        if (previousTokenStartIndex - 1 >= 0) {
                            localPos.X += _previousTokens[previousTokenStartIndex - 1].TextSize.GetValueOrDefault().X + 16f;
                        }

                        _previousTokens.RemoveRange(previousTokenStartIndex, _previousTokens.Count - previousTokenStartIndex);
                        _previousTokens.Add(token);
                        previousTokenIndex = previousTokenStartIndex;
                    }
                } else {
                    if (previousTokenIndex - 1 >= 0) {
                        localPos.X += _previousTokens[previousTokenIndex - 1].TextSize.GetValueOrDefault().X + 16f;
                    }

                    _previousTokens.Add(token);
                }

                Renderer.DrawString(
                    Font,
                    representation,
                    topLeft + localPos, 
                    rotation: 0f,
                    scale: Vector2.One,
                    ImageFlip.None,
                    textColor,
                    origin: Vector2.Zero,
                    scroll: Vector2.One
                );

                //localPos.X += token.TextSize.GetValueOrDefault().X + 16f;
                previousTokenIndex += 1;
            }
        }

        #endregion Private Methods

        #region Class Message

        public class Message : System.IEquatable<Message>, System.IEquatable<MessageLoggerTokenTree>, System.IDisposable {
            private List<ConsoleLoggerToken> _tokens = new List<ConsoleLoggerToken>();

            public Message() {
            }

            public int Count { get; private set; } = 1;
            public int Lines { get; private set; }
            public int TokensCount { get { return _tokens.Count; } }
            public bool IsMultiline { get { return Lines > 1; } }
            public bool IsClosed { get; private set; }
            public bool IsOpened { get { return !IsClosed; } }
            public bool IsDisposed { get; private set; }

            public ConsoleLoggerToken this [int index] {
                get {
                    return _tokens[index];
                }
            }

            public void Repeat(TimestampLoggerToken timestampToken) {
                if (timestampToken == null) {
                    throw new System.ArgumentNullException(nameof(timestampToken));
                }

                for (int i = 0; i < _tokens.Count; i++) {
                    if (_tokens[i].LoggerToken is TimestampLoggerToken previousTimestampToken) {
                        if (timestampToken.IsEarlier(previousTimestampToken)) {
                            throw new System.InvalidOperationException("Repeat only accepts later timestamps than current one.");
                        }

                        _tokens[i] = ConsoleLoggerToken.From(timestampToken);
                        break;
                    }
                }

                Count++;
            }

            public void AppendToken(LoggerToken token, out int newLines) {
                if (IsClosed) {
                    throw new System.InvalidOperationException("Can't modify a closed message.");
                }

                newLines = CountLines(token);
                Lines += newLines;
                _tokens.Add(ConsoleLoggerToken.From(token));
            }

            /*
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
            */

            public bool SetOrAppend(LoggerToken token) {
                if (IsClosed) {
                    throw new System.InvalidOperationException("Can't modify a closed message.");
                }

                for (int i = 0; i < _tokens.Count; i++) {
                    ConsoleLoggerToken consoleToken = _tokens[i];

                    if (consoleToken.IsLoggerType(token.GetType())) {
                        int previousEntryLines = CountLines(consoleToken),
                            newEntryLines = CountLines(token);

                        Lines += -previousEntryLines + newEntryLines;
                        _tokens[i] = ConsoleLoggerToken.From(token);
                        return true;
                    }
                }

                _tokens.Add(ConsoleLoggerToken.From(token));
                return false;
            }

            public ConsoleLoggerToken GetLastEntry() {
                if (_tokens.Count == 0) {
                    return null;
                }

                return _tokens[_tokens.Count - 1];
            }

            public ConsoleLoggerToken ByTokenType<T>() where T : LoggerToken {
                foreach (ConsoleLoggerToken token in _tokens) {
                    if (token.LoggerToken is T) {
                        return token;
                    }
                }

                return null;
            }

            public bool HasTokenType<T>() where T : LoggerToken {
                foreach (ConsoleLoggerToken token in _tokens) {
                    if (token.LoggerToken is T) {
                        return true;
                    }
                }

                return false;
            }

            public void Close() {
                if (IsClosed) {
                    return;
                }

                IsClosed = true;
            }

            public IEnumerator<ConsoleLoggerToken> GetEnumerator() {
                return _tokens.GetEnumerator();
            }

            public bool Equals(Message message) {
                if (message._tokens.Count != _tokens.Count) {
                    return false;
                }

                for (int i = 0; i < _tokens.Count; i++) {
                    ConsoleLoggerToken token = _tokens[i],
                                       otherToken = message._tokens[i];

                    if (!token.Equals(otherToken)) {
                        return false;
                    }
                }

                return true;
            }

            public bool Equals(MessageLoggerTokenTree message) {
                List<LoggerToken> tokens = message.Decompose();
                if (tokens.Count != _tokens.Count) {
                    return false;
                }

                for (int i = 0; i < tokens.Count; i++) {
                    ConsoleLoggerToken token = _tokens[i];
                    LoggerToken messageToken = tokens[i];

                    switch (token.LoggerToken) {
                        case TimestampLoggerToken timestampToken:
                            if (!(messageToken is TimestampLoggerToken)) {
                                return false;
                            }

                            break;

                        default:
                            if ((token.LoggerToken == null && messageToken != null) 
                             || (token.LoggerToken != null && !token.LoggerToken.Equals(messageToken))) {
                                return false;
                            }

                            break;
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

                _tokens.Clear();

                IsDisposed = true;
            }

            private int CountLines(LoggerToken token) {
                int newLines;

                switch (token) {
                    case TextLoggerToken textToken:
                        newLines = textToken.Text.Count("\n");
                        break;

                    default:
                        newLines = 0;
                        break;
                }

                return newLines;
            }

            private int CountLines(ConsoleLoggerToken token) {
                int newLines;

                switch (token.LoggerToken) {
                    case TextLoggerToken textToken:
                        newLines = textToken.Text.Count("\n");
                        break;

                    default:
                        newLines = 0;
                        break;
                }

                return newLines;
            }
        }

        #endregion Class Message

        #region Class ConsoleLoggerToken

        public class ConsoleLoggerToken {
            private ConsoleLoggerToken() {
            }

            public LoggerToken LoggerToken { get; private set; }
            public string Representation { get; private set; }
            public Vector2? TextSize { get; private set; }

            public static ConsoleLoggerToken From(LoggerToken token) {
                if (token == null) {
                    throw new System.ArgumentNullException(nameof(token));
                }

                string representation = string.Empty;

                switch (token) {
                    case TimestampLoggerToken timestampToken:
                        representation = timestampToken.Timestamp;
                        break;

                    case CategoryLoggerToken categoryToken:
                        representation = categoryToken.CategoryName;
                        break;

                    case SubjectsLoggerToken subjectsToken:
                        if (subjectsToken.Subjects.Count == 0) {
                            break;
                        }

                        representation = subjectsToken.Subjects[0];
                        for (int i = 1; i < subjectsToken.Subjects.Count; i++) {
                            representation += "->" + subjectsToken.Subjects[i];
                        }

                        break;

                    case TextLoggerToken textToken:
                        representation = textToken.Text;
                        break;

                    default:
                        break;
                }

                return new ConsoleLoggerToken() {
                    LoggerToken = token,
                    Representation = representation
                };
            }

            public void CalculateTextSize(Font font) {
                TextSize = font.MeasureText(Representation);
            }

            public bool IsLoggerType(System.Type loggerType) {
                return LoggerToken != null && LoggerToken.GetType().Equals(loggerType);
            }

            public bool IsLoggerType(LoggerToken token) {
                return token != null 
                    && LoggerToken != null 
                    && LoggerToken.GetType().Equals(token.GetType());
            }
        }

        #endregion Class ConsoleLoggerToken

        #region Class TextFormatter

        public class TextFormatter {
            public static readonly TextFormatter None = new TextFormatter(Color.White);

            public TextFormatter(Color textColor) {
                TextColor = textColor;
            }

            public Color TextColor { get; private set; }
        }

        #endregion Class MessageFormat
    }
}
