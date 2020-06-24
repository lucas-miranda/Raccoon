using System.Collections.Generic;

namespace Raccoon.Log {
    public class SubjectsLoggerToken : LoggerToken, System.IEquatable<SubjectsLoggerToken> {
        public List<string> Subjects { get; private set; } = new List<string>();

        public void AddSubject(string subject) {
            if (subject == null) {
                throw new System.ArgumentNullException(nameof(subject));
            }

            if (subject.Length == 0) {
                throw new System.ArgumentException("Empty subject name isn't allowed.");
            }

            Subjects.Add(subject);
        }

        public int SubjectsSimilarity(SubjectsLoggerToken token) {
            int similarity = 0;

            for (int i = 0; i < Subjects.Count; i++) {
                if (token.Subjects.Count <= i || Subjects[i] != token.Subjects[i]) {
                    break;
                }

                similarity += 1;
            }

            return similarity;
        }

        public override bool Equals(LoggerToken token) {
            if (!(token is SubjectsLoggerToken subjectsToken)) {
                return false;
            }

            return Equals(subjectsToken);
        }

        public virtual bool Equals(SubjectsLoggerToken token) {
            if (token.Subjects.Count != Subjects.Count) {
                return false;
            }

            for (int i = 0; i < token.Subjects.Count; i++) {
                if (!token.Subjects[i].Equals(Subjects[i])) {
                    return false;
                }
            }

            return true;
        }
    }
}
