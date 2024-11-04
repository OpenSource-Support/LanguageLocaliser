namespace LanguageLocaliser
{
    /// <summary>
    /// Identifies Locale information
    /// </summary>
    /// <param name="language">The language for the item (e.g. en)</param>
    /// <param name="region">The region for the item (e.g. US)</param>
    public struct LocaleInfo(string language, string region, string name = null) : IEquatable<LocaleInfo>
    {
        /// <summary>
        /// The readable name for the locale, e.g. English (US)
        /// </summary>
        public string Name = name;

        /// <summary>
        /// The language code for the locale, e.g. en
        /// </summary>
        public readonly string Language = language;

        /// <summary>
        /// The region for the locale, e.g. US
        /// </summary>
        public readonly string Region = region;

        /// <summary>
        /// Determines if the locale has all information filled in
        /// </summary>
        public bool Valid
        {
            get
            {
                return !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Language) && !String.IsNullOrEmpty(Region);
            }
        }

        /// <summary>
        /// Gets the ID in the format {langCode}_{region}
        /// </summary>
        public string Id
        {
            get
            {
                return $"{Language}_{Region}";
            }
        }

        /// <summary>
        /// Determines if two locale informations are equal
        /// </summary>
        /// <param name="other">The other one to compare it to</param>
        /// <returns>True if they are equal</returns>
        public bool Equals(LocaleInfo other)
        {
            return this.Id == other.Id;
        }

        /// <summary>
        /// Gets the hash code for the locale information
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Formats the locale information
        /// </summary>
        /// <returns>A string representing the locale information</returns>
        public override readonly string ToString()
        {
            return $"[Name='{Name}', Language='{Language}', Region='{Region}']";
        }

        /// <summary>
        /// Creates a new Locale Information from its Id in the form [language]_[region]
        /// </summary>
        /// <param name="id">The id to create the locale info for</param>
        /// <param name="name">The name associated with the locale</param>
        /// <returns>The new locale info or null if id is not valid</returns>
        public static LocaleInfo? Create(string id, string name = null)
        {
            var parts = id.Split('_');

            if (parts.Length == 2)
                return new LocaleInfo(parts[0], parts[1], name);

            return null;
        }

        /// <summary>
        /// Determines if two locale informations are equal
        /// </summary>
        /// <param name="obj">The other one to compare it to</param>
        /// <returns>True if they are equal</returns>
        public override bool Equals(object obj)
        {
            return obj is LocaleInfo info && Equals(info);
        }

        /// <summary>
        /// Determines if two locale informations are equal
        /// </summary>
        /// <param name="left">The first to compare</param>
        /// <param name="right">The second to compare</param>
        /// <returns>True if the locale informations are equal</returns>
        public static bool operator ==(LocaleInfo left, LocaleInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determine if two locale informations are not equal
        /// </summary>
        /// <param name="left">The first to compare</param>
        /// <param name="right">The second to compare</param>
        /// <returns>True if the locale informations are not equal</returns>
        public static bool operator !=(LocaleInfo left, LocaleInfo right)
        {
            return !(left == right);
        }
    }
}