﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using Raccoon.Graphics;

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
        private Graphics.Primitives.Rectangle _background;

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

        public Surface Surface { get; private set; }
        public bool Visible { get; private set; }
        public Font Font { get; set; }
        public bool ShowTimestamp { get; set; } = true;

        #endregion Public Properties

        #region Public Methods

        public void Start() {
            Surface = Game.Instance.DebugSurface;
            if (Font == null) {
                Font = Game.Instance.Core.StdFont;
            }

            _background = new Graphics.Primitives.Rectangle(Game.Instance.WindowWidth, Game.Instance.WindowHeight, Color.Black) {
                Surface = Game.Instance.Core.DebugSurface,
                Opacity = 0.25f
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

            Message msg;
            int messageIndex = _messages.FindIndex(m => m.Text == message);
            if (messageIndex != -1) {
                msg = _messages[messageIndex];
                msg.Repeat();

                _messages.RemoveAt(messageIndex);
                _messages.Insert(0, msg);

            } else {
                msg = new Message(message);
                _messages.Insert(0, msg);
            }

            if (!string.IsNullOrWhiteSpace(category)) {
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

        #endregion Public Methods

        #region Internal Methods

        internal void Update(int delta) {

        }

        internal void Render() {
            Vector2 position = Camera.Current != null ? Camera.Current.Position * Game.Instance.Scale : Vector2.Zero;
            _background.Render(position);

            // total messages
            Surface.DrawString(Font, _messages.Count.ToString(), position + new Vector2(Game.Instance.WindowWidth - 25, 15), Color.White);

            // messages
            position += new Vector2(15, Game.Instance.WindowHeight - 20 - Font.LineSpacing);
            foreach (Message message in _messages) {
                Surface.DrawString(Font, (ShowTimestamp ? message.Timestamp.ToString("HH:mm:ss").PadRight(10) : "") + (message.Count == 1 ? message.Text : $"{message.Text} [{message.Count}]"), position, message.Color);
                position += new Vector2(0, -Font.LineSpacing - SpaceBetweenLines);

                if (position.Y <= -Font.LineSpacing) {
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
            }

            public string Text { get; private set; }
            public Color Color { get; set; } = Color.White;
            public int Count { get; private set; } = 1;
            public DateTime Timestamp { get; private set; }

            public void Repeat() {
                Count++;
                Timestamp = DateTime.Now;
            }
        }

        #endregion Class Message
    }
}
