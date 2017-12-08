namespace Raccoon.Components {
    public class Body : Component {
        public static IMaterial StandardMaterial = new StandardMaterial();

        public Body(IShape shape, float mass = 1f, IMaterial material = null) {
            Shape = shape;
            Mass = mass;
            InverseMass = 1f / Mass;
            Material = material ?? StandardMaterial;
        }

        public IShape Shape { get; private set; }
        public IMaterial Material { get; set; }
        public float Mass { get; private set; }
        public float InverseMass { get; private set; }
        public Vector2 Position { get { return Entity.Position - Origin; } set { Entity.Position = value + Origin; } }
        public Vector2 Origin { get; set; }
        public Vector2 Velocity { get; set; }

        public override void OnAdded(Entity entity) {
            base.OnAdded(entity);
            Physics.Instance.AddCollider(this);
        }

        public override void OnRemoved() {
            base.OnRemoved();
            Physics.Instance.RemoveCollider(this);
        }

        public override void Update(int delta) {
        }
        
        public override void Render() {
        }

        Manifold m;
        public override void DebugRender() {
            /*Shape.DebugRender(Position);

            if (m != null) {
                foreach (Contact contact in m.Contacts) {
                    Debug.DrawLine(contact.Position, contact.Position + contact.Normal * contact.PenetrationDepth, Graphics.Color.Magenta);
                }
            }

            Debug.DrawString(Debug.Transform(Position + new Vector2(Shape.BoundingBox.Width / 1.9f, 0)), Position.ToString());*/
        }

        public void OnCollide(Body otherBody, Manifold manifold) {
            m = manifold;
        }
    }
}
