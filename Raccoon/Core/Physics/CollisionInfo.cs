namespace Raccoon {
    public class CollisionInfo<T> {
        public CollisionInfo(T subject, ContactList contacts) {
            Subject = subject;
            Contacts = contacts ?? new ContactList();
        }

        public CollisionInfo(T subject, params Contact[] contacts) : this(subject, new ContactList(contacts)) {
        }

        public T Subject { get; private set; }
        public ContactList Contacts { get; private set; }
    }
}
