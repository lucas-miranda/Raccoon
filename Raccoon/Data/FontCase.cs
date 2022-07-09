
namespace Raccoon.Data {
    public enum FontCase {
        None = 0,

        /// <summary>
        /// First letter of every word is uppercase, except at the first word.
        /// </summary>
        CamelCase,

        /// <summary>
        /// First letter of every word is uppercase.
        /// </summary>
        Capitalized,

        /// <summary>
        /// Every letter is lowercase.
        /// </summary>
        LowerCase,
    }

    public static class FontCaseExtensions {
        public static string Apply(this FontCase fontCase, string str, bool withSeparation) {
            str = withSeparation ? str.SeparateCapitalized() : str;

            switch (fontCase) {
                case FontCase.CamelCase:
                    return str.ToCamelCase();

                case FontCase.Capitalized:
                    return str.ToCapitalized();

                case FontCase.LowerCase:
                    return str.ToLowerInvariant();

                case FontCase.None:
                default:
                    break;
            }

            return str;
        }
    }
}
