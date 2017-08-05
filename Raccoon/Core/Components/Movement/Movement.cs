using System;
using System.Collections.Generic;

namespace Raccoon.Components {
    public abstract class Movement : Component {
        private float _moveUpdateTime;
        private Vector2 _nextAxis;
    
        public Movement(Vector2 maxSpeed, Vector2 acceleration, Collider collider = null) {
            CollisionTags = new List<string>();
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            Collider = collider;
            CanMove = true;
        }

        public Collider Collider { get; set; }
        public Vector2 Speed { get; protected set; }
        public Vector2 MaxSpeed { get; set; }
        public Vector2 TargetSpeed { get; protected set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 Axis { get; set; }
        public Vector2 LastAxis { get; protected set; }
        public float DragForce { get; set; } = 2f;
        public bool HorizontalAxisSnap { get; set; }
        public bool VerticalAxisSnap { get; set; }
        public bool AxesSnap { get { return HorizontalAxisSnap && VerticalAxisSnap; } set { HorizontalAxisSnap = VerticalAxisSnap = value; } }
        public bool CanMove { get; set; }

        protected List<string> CollisionTags { get; private set; }
        protected float MoveHorizontalBuffer { get; set; }
        protected float MoveVerticalBuffer { get; set; }

        public override void Update(int delta) {
            Axis = _nextAxis;
            if (CanMove) {
                _moveUpdateTime += delta;
                while (_moveUpdateTime >= Physics.MinUpdateInterval) {
                    OnMoveUpdate(Physics.MinUpdateInterval / 1000f);
                    _moveUpdateTime -= Physics.MinUpdateInterval;
                }
            }

            _nextAxis = Vector2.Zero;
        }

        public override void Render() { }
        public override void DebugRender() { }

        public bool HasCollisionTag(string tag) {
            return CollisionTags.Contains(tag);
        }

        public bool HasCollisionTag(Enum tag) {
            return HasCollisionTag(tag.ToString());
        }

        public void AddCollisionTag(string tag) {
            if (HasCollisionTag(tag)) {
                return;
            }

            CollisionTags.Add(tag);
        }

        public void AddCollisionTag(Enum tag) {
            AddCollisionTag(tag.ToString());
        }

        public void RemoveCollisionTag(string tag) {
            CollisionTags.Remove(tag);
        }

        public void RemoveCollisionTag(Enum tag) {
            RemoveCollisionTag(tag.ToString());
        }

        public void ClearCollisionTags() {
            CollisionTags.Clear();
        }

        public abstract void OnCollide(Vector2 moveDirection);
        public abstract void OnMoveUpdate(float dt);

        public virtual void Move(Vector2 axis) {
            axis = Util.Math.Clamp(axis, new Vector2(-1, -1), new Vector2(1, 1));
            if (axis != Vector2.Zero) {
                LastAxis = axis;
            }

            _nextAxis = axis;
        }

        public virtual void Move(float x, float y) {
            Move(new Vector2(x, y));
        }

        public virtual void MoveHorizontal(float x) {
            x = Util.Math.Clamp(x, -1, 1);

            if (x != 0) {
                LastAxis = new Vector2(x, LastAxis.Y);
            }

            _nextAxis = new Vector2(x, _nextAxis.Y);
        }

        public virtual void MoveVertical(float y) {
            y = Util.Math.Clamp(y, -1, 1);

            if (y != 0) {
                LastAxis = new Vector2(LastAxis.X, y);
            }

            _nextAxis = new Vector2(_nextAxis.X, y);
        }
    }
}
