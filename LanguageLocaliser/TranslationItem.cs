namespace LanguageLocaliser
{
    /// <summary>
    /// Encapsulates a translation item from the locale files
    /// </summary>
    public struct TranslationItem
    {
        /// <summary>
        /// The source locale for the item
        /// </summary>
        public LocaleInfo Locale;
        
        /// <summary>
        /// The name for the item
        /// </summary>
        public string Name;

        /// <summary>
        /// The translation text for the item
        /// </summary>
        public string Text;

        /// <summary>
        /// The order to sort by to maintain original file order
        /// </summary>
        public double Order = -1;

        /// <summary>
        /// Creates a new empty translation item
        /// </summary>
        public TranslationItem()
        {
        }

        public bool Valid => Locale.Valid && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Text);

        public override readonly string ToString()
        {
            return $"[Name='{Name}', Locale='{Locale}', Order='{Order}', Text='{Text}']";
        }
    }
}