namespace Raccoon {
    public class CollisionInfo<T> {
        public CollisionInfo(T subject, params Contact[] contacts) {
            Subject = subject;
            if (contacts == null) {
                Contacts = new Contact[0];
            } else {
                Contacts = new Contact[contacts.Length];
                contacts.CopyTo(Contacts, 0);
            }
        }

        public T Subject { get; private set; }
        public Contact[] Contacts { get; private set; }

        public bool ContainsContact(System.Predicate<Contact> predicate) {
            return System.Array.FindIndex(Contacts, predicate) > -1;
        }
    }
}
