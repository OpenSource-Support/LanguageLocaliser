namespace LanguageLocaliser
{
    public interface ITranslator
    {
        /// <summary>
        /// Indicates a translation that needs to occur
        /// </summary>
        public struct Translation
        {
            /// <summary>
            /// The translation item to translate from
            /// </summary>
            public TranslationItem From;

            /// <summary>
            /// The Locale to translate to
            /// </summary>
            public LocaleInfo ToLocale;

            public bool Valid => ToLocale.Valid && From.Valid;

            public override readonly string ToString()
            {
                return $"[From='{From}', ToLocale='{ToLocale}']";
            }
        }

        /// <summary>
        /// Translates a set of Translations to TranslationItems asynchronously
        /// </summary>
        /// <param name="items">The translations to translate</param>
        /// <returns>An enumerable of the translation items</returns>
        Task<IEnumerable<TranslationItem>> TranslateAsync(IEnumerable<Translation> items);

        /// <summary>
        /// Translates a set of Translations to TranslationItems
        /// </summary>
        /// <param name="items">The translations to translate</param>
        /// <returns>An enumerable of the translation items</returns>
        IEnumerable<TranslationItem> Translate(IEnumerable<Translation> items);
    }
}