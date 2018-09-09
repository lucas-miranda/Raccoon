namespace Raccoon {
    public class ContactList {
        private readonly Contact[] _entries;
        
        public ContactList(int capacity) {
            _entries = new Contact[capacity];
        }

        public ContactList(params Contact[] contacts) {
            if (contacts == null || contacts.Length == 0) {
                _entries = new Contact[0];
            } else {
                _entries = new Contact[contacts.Length];
                contacts.CopyTo(_entries, 0);
            }
        }

        public ContactList() : this(0) {
        }

        public int Count { get { return _entries.Length; } }
        public Contact this[int index] { get { return _entries[index]; } }

        public bool Contains(System.Predicate<Contact> predicate) {
            return System.Array.FindIndex(_entries, predicate) > -1;
        }

        public int FindIndex(System.Predicate<Contact> predicate) {
            return System.Array.FindIndex(_entries, predicate);
        }
    }
}
