﻿using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon {
    public class Entity {
        #region Private Members

        private Vector2 position;
        private float rotation;

        #endregion Private Members

        #region Constructors

        public Entity() {
            Name = "Entity";
            Active = Visible = true;
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public bool Active { get; set; }
        public bool Visible { get; set; }
        public Graphic Graphic { get { return Graphics[0]; } set { Graphics[0] = value; } }
        public List<Graphic> Graphics { get; set; }

        public Vector2 Position {
            get { return position; }
            set {
                position = value;
                foreach (Graphic g in Graphics) {
                    g.Position = position;
                }
            }
        }

        public float Rotation {
            get { return rotation; }
            set {
                float alpha = value - rotation;
                rotation = value;
                foreach (Graphic g in Graphics) {
                    g.Rotation = alpha;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void Update(int delta) {
            if (Active) {
                foreach (Graphic g in Graphics) {
                    g.Update(delta);
                }
            }
        }

        public virtual void Draw() {
            if (Visible) {
                foreach (Graphic g in Graphics) {
                    g.Draw();
                }
            }
        }

        public void AddGraphic(Graphic graphic) {
            if (Graphics == null) {
                Graphics = new List<Graphic>();
            }

            Graphic = graphic;
        }

        public void AddGraphics(IEnumerable<Graphic> graphics) {
            if (Graphics == null) {
                Graphics = new List<Graphic>();
            }

            Graphics.AddRange(graphics);
        }

        public override string ToString() {
            return $"[Entiy '{Name}' X: {X} Y: {Y}]";
        }

        #endregion Public Methods
    }
}
