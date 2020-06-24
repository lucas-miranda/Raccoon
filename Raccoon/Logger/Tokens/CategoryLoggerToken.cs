
namespace Raccoon.Log {
    public class CategoryLoggerToken : LoggerToken, System.IEquatable<CategoryLoggerToken> {
        public CategoryLoggerToken(string categoryName) {
            CategoryName = categoryName;
        }

        public string CategoryName { get; private set; }

        public override bool Equals(LoggerToken token) {
            if (!(token is CategoryLoggerToken categoryToken)) {
                return false;
            }

            return Equals(categoryToken);
        }

        public virtual bool Equals(CategoryLoggerToken token) {
            return token.CategoryName.Equals(CategoryName);
        }
    }
}
