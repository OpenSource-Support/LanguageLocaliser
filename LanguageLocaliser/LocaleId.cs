namespace LanguageLocaliser
{
    /// <summary>
    /// Encapsulates names for a locale Id
    /// </summary>
    /// <param name="englishName">The name as represented in English US</param>
    /// <param name="name">The name as represented in its own language</param>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class LocaleIdNamesAttribute(string englishName, string name) : System.Attribute
    {
        /// <summary>
        /// The name as represented in its own language
        /// </summary>
        public readonly string Name = name;

        /// <summary>
        /// The name as represented in Ensligh US
        /// </summary>
        public readonly string EnglishName = englishName;
    }

    public static class LocaleIdExtensionMethods
    {
        /// <summary>
        /// Gets the name of the Locale in its own language
        /// </summary>
        /// <param name="localeId">The locale to get the name for</param>
        /// <returns>The name of the lanuage or Null if not defined</returns>
        public static string Name(this LocaleId localeId)
        {

            return localeId.GetAttributeOfType<LocaleIdNamesAttribute>()?.Name;
        }

        /// <summary>
        /// Gets the name of the Locale in English US
        /// </summary>
        /// <param name="localeId">The locale to get the name for</param>
        /// <returns>The name of the lanuage or Null if not defined</returns>
        public static string EnglishName(this LocaleId localeId)
        {

            return localeId.GetAttributeOfType<LocaleIdNamesAttribute>()?.EnglishName;
        }

        /// <summary>
        /// Converts this Locale Id to a Locale info object
        /// </summary>
        /// <param name="localeId">The locale ID to convert</param>
        /// <returns>The Locale Info for the ID</returns>
        public static LocaleInfo ToLocale(this LocaleId localeId)
        {
            return LocaleInfo.Create(localeId.ToString(), localeId.Name()).Value;
        }
    }

    /// <summary>
    /// Encapsulates a pre-defined Locale Id
    /// </summary>
    public enum LocaleId
    {
        [LocaleIdNames("Arabic", "اَلْعَرَبِيَّةُ")]
        ar_SA,
        [LocaleIdNames("Dutch", "Deutsch")]
        de_DE,
        [LocaleIdNames("Greek", "Greek")]
        el_GR,
        [LocaleIdNames("English (US)", "English (US)")]
        en_US,
        [LocaleIdNames("Spanish", "Español (ES)")]
        es_ES,
        [LocaleIdNames("French", "Français")]
        fr_FR,
        [LocaleIdNames("Hebrew (Israel)", "עִברִית")]
        he_IL,
        [LocaleIdNames("Italian", "Italiano")]
        it_IT,
        [LocaleIdNames("Japanese", "日本語")]
        ja_JP,
        [LocaleIdNames("Korean", "한국어")]
        ko_KR,
        [LocaleIdNames("Polish", "Polski")]
        pl_PL,
        [LocaleIdNames("Portuguese (Brazil)", "Português (BR)")]
        pt_BR,
        [LocaleIdNames("Russian", "Русский (RU)")]
        ru_RU,
        [LocaleIdNames("Thai", "ภาษาไทย")]
        th_TH,
        [LocaleIdNames("Turkish", "Türkçe")]
        tr_TR,
        [LocaleIdNames("Ukranian", "Українська")]
        uk_UA,
        [LocaleIdNames("Chinese (Simplified)", "简体中文")]
        zh_CN,
        [LocaleIdNames("Traditional Chinese (Taiwan)", "简体中文")]
        zh_TW
    }
}