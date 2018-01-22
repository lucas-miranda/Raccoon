using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon.Components {
    public class Body : Component {
        #region Public Members

        public static IMaterial StandardMaterial = new StandardMaterial();

        #endregion Public Members

        #region Private Members

        private List<IConstraint> _constraints = new List<IConstraint>();
        private Movement _movement;
        private Vector2 _movementBuffer;
        private bool _isTryingToMove;

        #endregion Private Members

        #region Constructors

        public Body(IShape shape, IMaterial material = null) {
            Shape = shape;
            Shape.Body = this;
            Material = material ?? StandardMaterial;
            Mass = Shape.ComputeMass(1f);
            InverseMass = Mass == 0f ? 0f : (1f / Mass);
        }

        #endregion Constructors

        #region Public Properties

        public IShape Shape { get; private set; }
        public IMaterial Material { get; set; }
        public float Mass { get; private set; }
        public float InverseMass { get; private set; }
        /*public float Torque { get; private set; }
        public float LastRotation { get; private set; }
        public float Rotation { get { return Entity.Rotation; } set { Entity.Rotation = value; } }
        public float AngularVelocity { get { return Rotation - LastRotation; } }*/
        public Vector2 LastPosition { get; set; }
        public Vector2 Position { get { return Entity.Position - Origin; } set { Entity.Position = value + Origin; } }
        public Vector2 Origin { get; set; }
        public Vector2 Velocity { get { return Position - LastPosition; } }
        public Vector2 Force { get; set; }
        public int Constraints { get { return _constraints.Count; } }
        public bool IsResting { get; private set; } = true;

#if DEBUG
        public Color Color { get; set; } = Color.White;
#endif

        public Movement Movement {
            get {
                return _movement;
            }

            set {
                if (_movement != null) {
                    _movement.Body = null;
                }

                _movement = value;
                _movement.Body = this;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void OnAdded(Entity entity) {
            base.OnAdded(entity);
            LastPosition = Position;
            //LastRotation = Rotation;
            Physics.Instance.AddCollider(this);
        }

        public override void OnRemoved() {
            base.OnRemoved();
            Physics.Instance.RemoveCollider(this);
        }

        public override void Update(int delta) {
            Movement?.Update(delta);
        }
        
        public override void Render() {
        }

#if DEBUG
        Manifold m;
        public override void DebugRender() {
            Shape.DebugRender(Position, Color);

            if (m != null) {
                foreach (Contact contact in m.Contacts) {
                    Debug.DrawLine(contact.Position, contact.Position + contact.Normal * contact.PenetrationDepth, Color.Magenta);
                }

                m = null;
            }

            // Position and Velocity
            //Debug.DrawString(Debug.Transform(Position + new Vector2(Shape.BoundingBox.Width / 1.9f, 0)), string.Format("[{0:0.##}, {1:0.##}]\nVelocity: [{2:0.##}, {3:0.##}]\nM: {4}", Position.X, Position.Y, Velocity.X, Velocity.Y, Mass));

            if (Movement != null && Movement.IsDebugRenderEnabled) {
                Movement.DebugRender();
            }
        }
#endif

        public void OnCollide(Body otherBody) {
            Movement?.OnCollide();
        }

        public void SolveConstraints() {
            foreach (IConstraint constraint in _constraints) {
                constraint.Solve();
            }
        }

        public void AddConstraint(IConstraint constraint) {
            _constraints.Add(constraint);
        }

        public void RemoveConstraint(IConstraint constraint) {
            _constraints.Remove(constraint);
        }

        public Vector2 PrepareMovement(float dt) {
            Vector2 velocity = Velocity;

            // velocity correction
            if (Util.Math.EqualsEstimate(velocity.X, 0)) {
                velocity.X = 0f;
            }

            if (Util.Math.EqualsEstimate(velocity.Y, 0)) {
                velocity.Y = 0f;
            }

            velocity = Movement?.HandleVelocity(velocity, dt) ?? velocity;

            if (!_isTryingToMove) {
                IsResting = true;
                _movementBuffer = Vector2.Zero;
            }

            // position
            return Position + _movementBuffer + velocity + Force * dt * dt;
        }

        public void AfterMovement(Vector2 nextPosition, Vector2 movementBuffer) {
            _movementBuffer = movementBuffer;
            LastPosition = Entity.Position;
            Position = nextPosition;

            // orientation
            /*float nextAngle = Rotation + AngularVelocity + Torque * dt * dt;
            LastRotation = Rotation;
            Rotation = nextAngle;*/

            // clear values
            Force = Vector2.Zero;
            //Torque = 0;

            //Movement?.LateHandleVelocity(velocity);
            _isTryingToMove = false;
        }

        public void ApplyForce(Vector2 force) {
            Force += Movement?.HandleForce(force) ?? force;
            _isTryingToMove = true;
            IsResting = false;
        }

        public void ApplyImpulse(Vector2 impulse) {
            Force += Movement?.HandleImpulse(impulse * InverseMass) ?? (impulse * InverseMass);
            //Torque += InverseInertia * Vector2.Cross(contactVector, impulse);
            _isTryingToMove = true;
            IsResting = false;
        }

        public override string ToString() {
            return $"[Body | Shape: {Shape}, Movement: {Movement}]";
        }

        #endregion Public Methods
    }
}
