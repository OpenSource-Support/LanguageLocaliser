using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static LanguageLocaliser.ITranslator;

namespace LanguageLocaliser
{
    /// <summary>
    /// Encapsulates a set of translation items for the Ryujinx Project
    /// </summary>
    public class RyujinxTranslationItems : TranslationItems
    {
        /// <summary>
        /// The name of the language name Josn field
        /// </summary>
        private const string _languageName = "Language";

        /// <summary>
        /// Creates a new set of Ryjinx translation items
        /// </summary>
        public RyujinxTranslationItems(ILogger logger = null) : base(logger)
        {
            // Don't upset the existing order for git diff reasons
            _defaultFileSortOption = SortOption.OriginalOrder;

            // Use pretty Json formatting with limited encoding
            _defaultFileJsonOptions = new JsonSerializerOptions() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        }

        /// <summary>
        /// Adds the additional Language value
        /// </summary>
        /// <param name="localeInfo">The locale info the data is being generated for</param>
        /// <returns>A list of additional name value combinations for the start of the json output</returns>
        protected override IEnumerable<NameValue> GetAdditionalHeaderValues(LocaleInfo localeInfo)
        {
            return
                new NameValue[] { new NameValue() { Name = _languageName, Value = localeInfo.Name } }
                .Concat(base.GetAdditionalHeaderValues(localeInfo));
        }

        /// <summary>
        /// Processes a json value into a dictionary
        /// </summary>
        /// <param name="localeInfo">The locale infor the Json is being generated for</param>
        /// <param name="items">The name value combinations available</param>
        /// <param name="item">The JsonProperty item containing the json data</param>
        protected override void ProcessJson(ref LocaleInfo localeInfo, Dictionary<string, string> items, JsonProperty item)
        {
            if (item.Name == _languageName)
            {
                localeInfo.Name = item.Value.ToString();
            }
            else
            {
                base.ProcessJson(ref localeInfo, items, item);
            }
        }

        /// <summary>
        /// Sets up the Locale, sorting and Json options for a translation file
        /// </summary>
        /// <param name="filename">The filename to process</param>
        /// <param name="localeInfo">The locale to setup for</param>
        /// <param name="encoding">The encoding to use</param>
        /// <param name="sortOption">The sort option to use</param>
        /// <param name="jsonOptions">The json serializer options to use</param>
        protected override void SetupWriteForFilename(string filename, LocaleInfo localeInfo, ref Encoding encoding, ref SortOption sortOption, JsonSerializerOptions jsonOptions)
        {
            base.SetupWriteForFilename(filename, localeInfo, ref encoding, ref sortOption, jsonOptions);

            if (File.Exists(filename))
            {
                var newLine = TextFile.DetectLineEnding(filename);
                if (newLine != jsonOptions.NewLine)
                    jsonOptions.NewLine = newLine;
                encoding = TextFile.GetEncoding(filename);
            }
        }

        /// <summary>
        /// Filters the json data before it is written
        /// </summary>
        /// <param name="json">The raw json text</param>
        /// <param name="options">The options to use for the json</param>
        protected override void FilterJsonBeforeWrite(ref string json, JsonSerializerOptions options)
        {
            base.FilterJsonBeforeWrite(ref json, options);

            json =
                json
                .Replace(@"\u00A0", Convert.ToChar(0x00A0).ToString())
                + (options?.NewLine ?? Environment.NewLine);
        }

        /// <summary>
        /// Gets a set of testable translations to check for comparison against existing translation values
        /// </summary>
        /// <param name="sourceLocale">The source locale to use as the test point</param>
        /// <returns>An enumerable of translation items to test</returns>
        public IEnumerable<Translation> TestableTranslations(LocaleInfo sourceLocale)
        {
            return base.TestableTranslations(sourceLocale, (item) => !item.Text.Contains('{') && item.Text.Where(ch => ch == ' ').Count() >= 2);
        }

        /// <summary>
        /// Checks one item from each language from the soure English US Locale to confirm the translation values are the same
        /// </summary>
        public void CheckLanguageTranslations()
        {
            var testTranslations = TestableTranslations(new LocaleInfo("en", "US"));
            var testResults = new GoogleTranslator(Logger).Translate(testTranslations.Where(tt => tt.From.Valid));

            foreach (var tr in testResults)
            {
                var tt =
                    testTranslations
                    .Where(ttitem => ttitem.ToLocale.Equals(tr.Locale))
                    .FirstOrDefault();

                if (!tt.From.Valid)
                {
                    Logger?.WriteLine($"Could not find testable translation for language {tt.ToLocale}");
                }
                else
                {
                    var existingValue = this[tt.From.Name, tt.ToLocale];
                    if (existingValue != tr.Text)
                    {
                        Logger?.WriteLine($"Translation from {tt.From} to {tt.ToLocale} failed conversion test with existing value being '{existingValue}' and new value being '{tr.Text}'.");
                    }
                }
            }
        }

    }
}