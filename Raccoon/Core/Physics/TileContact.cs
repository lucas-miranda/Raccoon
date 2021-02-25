
namespace Raccoon {
    public class TileContact : Contact {
        public TileContact(int column, int row, Vector2 position, Vector2 normal, float penetrationDepth) : base(position, normal, penetrationDepth) {
            Column = column;
            Row = row;
        }

        public TileContact(int column, int row, Contact contact) : base(contact.Position, contact.Normal, contact.PenetrationDepth) {
            Column = column;
            Row = row;
        }

        public int Column { get; private set; }
        public int Row { get; private set; }
    }
}
