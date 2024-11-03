using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using static LanguageLocaliser.ITranslator;

namespace LanguageLocaliser
{
    public class TranslationItems
    {
        /// <summary>
        /// Default Json file extension
        /// </summary>
        protected const string _jsonExt = ".json";

        /// <summary>
        /// The file specification for all locale json files
        /// </summary>
        private const string _fileSpec = "*_*";

        /// <summary>
        /// Indicates how to sort the output data for the localisation
        /// </summary>
        public enum SortOption
        {
            /// <summary>
            /// Order by the original sort order of the file
            /// </summary>
            OriginalOrder,

            /// <summary>
            /// Order the items in alphabetical order
            /// </summary>
            AlphabeticalOrder
        }

        /// <summary>
        /// A name and value combination
        /// </summary>
        public struct NameValue
        {
            /// <summary>
            /// The name for the value
            /// </summary>
            public string Name;

            /// <summary>
            /// The value
            /// </summary>
            public string Value;
        }

        /// <summary>
        /// Represents required translations to satisfy all combinations of locales
        /// </summary>
        public class RequiredTranslation
        {
            /// <summary>
            /// The source item to use when translating from
            /// </summary>
            public TranslationItem Source;

            /// <summary>
            /// The various destiniation locales that require a translation
            /// </summary>
            public IEnumerable<LocaleInfo> Destinations;
        }

        /// <summary>
        /// The locales that this list of translation items represents
        /// </summary>
        protected HashSet<LocaleInfo> _locales = [];

        /// <summary>
        /// The translation items by Name
        /// </summary>
        protected Dictionary<string, Dictionary<LocaleInfo, TranslationItem>> _items = [];

        /// <summary>
        /// The default file sort option
        /// </summary>
        protected SortOption _defaultFileSortOption = SortOption.OriginalOrder;

        /// <summary>
        /// The default json serlisation options to use
        /// </summary>
        protected JsonSerializerOptions _defaultFileJsonOptions = null;

        /// <summary>
        /// The default file encoding to use when savint to a file
        /// </summary>
        protected Encoding _defaultFileEncoding = Encoding.UTF8;

        /// <summary>
        /// Reads the data from a json element
        /// </summary>
        /// <param name="data">The jason element to load data from</param>
        /// <param name="localeInfo">The locale information for the json data</param>
        /// <returns></returns>
        private Dictionary<string, string> LoadFromJson(JsonElement data, ref LocaleInfo localeInfo)
        {
            var items = new Dictionary<string, string>();

            foreach (var item in data.EnumerateObject())
            {
                ProcessJson(ref localeInfo, items, item);
            }

            return items;
        }

        /// <summary>
        /// Gets an item by name and locale information
        /// </summary>
        /// <param name="name">The name to get the item for</param>
        /// <param name="localeInfo">The locale information to get the item for</param>
        /// <returns>The translation item</returns>
        private TranslationItem GetItem(string name, LocaleInfo localeInfo)
        {
            return _items[name][localeInfo];
        }

        /// <summary>
        /// Validates a locale information object
        /// </summary>
        /// <param name="localeInfo">The locale information to validate</param>
        /// <returns>The locale information that was validated and added</returns>
        /// <exception cref="InvalidOperationException">The locale information could not be validated</exception>
        private LocaleInfo ValidateLocale(LocaleInfo localeInfo)
        {
            if (!localeInfo.Valid)
                throw new InvalidOperationException($"The locale information of {localeInfo} was invalid");

            if (_locales.TryGetValue(localeInfo, out var existingLocaleInfo))
            {
                if (existingLocaleInfo.Name != localeInfo.Name)
                    throw new InvalidOperationException($"The locale {localeInfo} does not match the stored locale of {existingLocaleInfo}");
            }
            else
            {
                existingLocaleInfo = localeInfo;
                _locales.Add(existingLocaleInfo);
            }

            return existingLocaleInfo;
        }

        /// <summary>
        /// Adds or replaces a translation item
        /// </summary>
        /// <param name="name">The name for the item</param>
        /// <param name="localeInfo">The locale information for the item</param>
        /// <param name="text">The translated text for the item</param>
        /// <param name="order">The order of the item in the original file if applicable</param>
        /// <returns>The Translation item that was added</returns>
        private TranslationItem AddOrReplace(string name, LocaleInfo localeInfo, string text, double order = double.MaxValue)
        {
            localeInfo = ValidateLocale(localeInfo);
            if (!_items.TryGetValue(name, out var forName))
            {
                forName = [];
                _items[name] = forName;
            }

            if (order == -1)
                order = FindBestOrder(localeInfo, name);

            var item = new TranslationItem() { Locale = localeInfo, Name = name, Text = text, Order = order };
            forName[localeInfo] = item;
            return item;
        }

        /// <summary>
        /// Filters the json data before it is written
        /// </summary>
        /// <param name="json">The raw json text</param>
        /// <param name="options">The options to use for the json</param>
        protected virtual void FilterJsonBeforeWrite(ref string json, JsonSerializerOptions options)
        {
        }

        /// <summary>
        /// Sets up the Locale, sorting and Json options for writing translation json
        /// </summary>
        /// <param name="localeInfo">The locale to setup for</param>
        /// <param name="sortOption">The sort option to use</param>
        /// <param name="jsonOptions">The json serializer options to use</param>
        protected virtual void SetupWriteForLocale(LocaleInfo localeInfo, ref SortOption sortOption, JsonSerializerOptions jsonOptions)
        {
        }

        /// <summary>
        /// Sets up the Locale, sorting and Json options for a translation file
        /// </summary>
        /// <param name="filename">The filename to process</param>
        /// <param name="localeInfo">The locale to setup for</param>
        /// <param name="encoding">The encoding to use</param>
        /// <param name="sortOption">The sort option to use</param>
        /// <param name="jsonOptions">The json serializer options to use</param>
        protected virtual void SetupWriteForFilename(string filename, LocaleInfo localeInfo, ref Encoding encoding, ref SortOption sortOption, JsonSerializerOptions jsonOptions)
        {
        }

        /// <summary>
        /// Update the language and region from a file name
        /// </summary>
        /// <param name="localeInfo">The locale info to udpate</param>
        /// <param name="filename">The filename to update the locale for</param>
        /// <returns>A partially created locale info object</returns>
        protected virtual void UpdateLocaleForFilename(ref LocaleInfo localeInfo, string filename)
        {
            var fileParts = Path.GetFileNameWithoutExtension(filename).Split('_');

            if (fileParts.Length == 2)
            {
                localeInfo.Language = fileParts[0];
                localeInfo.Region = fileParts[1];
            }
        }

        /// <summary>
        /// Adds the additional Language value
        /// </summary>
        /// <param name="localeInfo">The locale info the data is being generated for</param>
        /// <returns>A list of additional name value combinations for the start of the json output</returns>
        protected virtual IEnumerable<NameValue> GetAdditionalHeaderValues(LocaleInfo localeInfo)
        {
            return [];
        }

        /// <summary>
        /// Gets the files to process for a folder for locale text information
        /// </summary>
        /// <param name="path">The path to search for files in</param>
        /// <returns>An IEnumerable of file paths to load</returns>
        protected virtual IEnumerable<string> GetFolderFilesToProcess(string path)
        {
            return Directory.EnumerateFiles(path, $"{_fileSpec}{_jsonExt}", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Gets the file name for the locale to process or create
        /// </summary>
        /// <param name="path">The path to the folder containing the file</param>
        /// <param name="localeInfo">The locale information for the file</param>
        /// <returns>The full path and filename of the file</returns>
        protected virtual string GetFilenameForLocale(string path, LocaleInfo localeInfo)
        {
            return Path.ChangeExtension(Path.Combine(path, localeInfo.Id), _jsonExt);
        }

        /// <summary>
        /// Processes a json value into a dictionary
        /// </summary>
        /// <param name="localeInfo">The locale infor the Json is being generated for</param>
        /// <param name="items">The name value combinations available</param>
        /// <param name="item">The JsonProperty item containing the json data</param>
        protected virtual void ProcessJson(ref LocaleInfo localeInfo, Dictionary<string, string> items, JsonProperty item)
        {
            items[item.Name.ToString()] = item.Value.ToString();
        }

        /// <summary>
        /// Finds the best location order value for an item
        /// </summary>
        /// <param name="localeInfo">The locale infor for the data being generated</param>
        /// <param name="name">The name of the item</param>
        /// <returns>A value for the Order of the Translation Item relative to the Orders of the other items</returns>
        /// <remarks>Finds the best place for the item given what items it is simlar to at the start</remarks>
        protected virtual double FindBestOrder(LocaleInfo localeInfo, string name)
        {
            var orderedItems =
                _items.Values
                .Where(v => v.ContainsKey(localeInfo))
                .Select(v => v[localeInfo])
                .OrderBy(i => i.Order)
                .Select((i, idx) => new { item = i, idx = idx });

            if (!orderedItems.Any())
                return 1.0;

            var endOrder = orderedItems.Max(i => i.item.Order) + 1.0;
            var startOrder = orderedItems.First().item.Order / 2.0;

            var namesMatching =
                orderedItems
                .Select(i => new { i.item, i.idx, matched = name.CountMatching(i.item.Name) });

            double maxMatching = namesMatching.Max(i => i.matched);
            if (maxMatching == 0)
            {
                if (String.Compare(orderedItems.First().item.Name, name) > 0)
                {
                    return startOrder;
                }
                else
                {
                    return endOrder;
                }
            }

            var namesMatchingLength =
                namesMatching
                .Where(i => i.matched == maxMatching);

            var firstAfter =
                namesMatchingLength
                .SkipWhile(i => String.Compare(i.item.Name, name) < 0)
                .Take(1)
                .Select(i => (int?)i.idx)
                .FirstOrDefault()
                ?? namesMatchingLength.Last().idx + 1;

            if (firstAfter >= orderedItems.Count())
                return endOrder;

            if (firstAfter == 0)
                return startOrder;

            var beforeOrder = orderedItems.ElementAt(firstAfter - 1).item.Order;
            var afterOrder = orderedItems.ElementAt(firstAfter).item.Order;
            return (afterOrder + beforeOrder) / 2.0;
        }

        /// <summary>
        /// Reads all compatible files from a folder asynchronously
        /// </summary>
        /// <param name="path">The path to read the files from</param>
        public async Task ReadFromFolderAsync(string path)
        {
            foreach (var filename in GetFolderFilesToProcess(path))
                await ReadFromFileAsync(filename);
        }

        /// <summary>
        /// Reads all compatible files from a folder
        /// </summary>
        /// <param name="path">The path to read the files from</param>
        public void ReadFromFolder(string path)
        {
            ReadFromFolderAsync(path).Wait();
        }

        /// <summary>
        /// Reads json translation data from a file asynchronously
        /// </summary>
        /// <param name="filename">The file to read from</param>
        public async Task ReadFromFileAsync(string filename)
        {
            try
            {
                string json = await File.ReadAllTextAsync(filename);
                var localeInfo = new LocaleInfo();
                UpdateLocaleForFilename(ref localeInfo, filename);
                ReadFromJson(ref localeInfo, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads json translation data from a file
        /// </summary>
        /// <param name="filename">The file to read from</param>
        public void ReadFromFile(string filename)
        {
            ReadFromFileAsync(filename).Wait();
        }

        /// <summary>
        /// Reads json translation data
        /// </summary>
        /// <param name="localeInfo">The locale information for the data</param>
        /// <param name="json">The json data to read from</param>
        public void ReadFromJson(ref LocaleInfo localeInfo, string json)
        {
            JsonElement data = JsonSerializer.Deserialize<JsonElement>(json);
            var items = LoadFromJson(data, ref localeInfo);
            localeInfo = ValidateLocale(localeInfo);

            double order = 1.0;
            foreach (var item in items)
                this[item.Key, localeInfo, order++] = item.Value;
        }

        /// <summary>
        /// Writes all files to a specific folder asynchronously, overwriting as needed
        /// </summary>
        /// <param name="path">The folder to write the file to</param>
        public async Task WriteToFolderAsync(string path)
        {
            foreach (var localeInfo in _locales)
            {
                string filename = GetFilenameForLocale(path, localeInfo);
                await WriteToFileAsync(filename, localeInfo);
            }
        }

        /// <summary>
        /// Writes all files to a specific folder, overwriting as needed
        /// </summary>
        /// <param name="path">The folder to write the file to</param>
        public void WriteToFolder(string path)
        {
            WriteToFolderAsync(path).Wait();
        }

        /// <summary>
        /// Clears all locales and items
        /// </summary>
        public void Clear()
        {
            ClearTranslations();
            _locales.Clear();
        }

        /// <summary>
        /// Clears all items
        /// </summary>
        public void ClearTranslations()
        {
            _items.Clear();
        }

        /// <summary>
        /// Writes translations for a specific locale to a file asynchronously
        /// </summary>
        /// <param name="filename">The path containing to contain the file to write to</param>
        /// <param name="localeInfo"></param>
        public async Task WriteToFileAsync(string filename, LocaleInfo localeInfo)
        {
            try
            {
                var encoding = _defaultFileEncoding;
                SetupLocaleForJson(localeInfo, out var sortOption, out var jsonOptions);
                SetupWriteForFilename(filename, localeInfo, ref encoding, ref sortOption, jsonOptions);
                string json = SerialiseJson(localeInfo, sortOption, jsonOptions);

                using var sr = TextFile.EncodingCompatibleStreamWriter(filename, false, encoding);
                await sr.WriteAsync(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Serlialises the Json for the locale
        /// </summary>
        /// <param name="localeInfo">The locale to serialise for</param>
        /// <param name="sortOption">The sorting option to use</param>
        /// <param name="jsonOptions">The json Options to use</param>
        /// <returns>The json as a string value</returns>
        private string SerialiseJson(LocaleInfo localeInfo, SortOption sortOption, JsonSerializerOptions jsonOptions)
        {
            var json =
                JsonSerializer.Serialize(
                    GetAdditionalHeaderValues(localeInfo)
                    .Concat(
                        OrderedItems(localeInfo, sortOption)
                        .Select(item => new NameValue() { Name = item.Name, Value = item.Text })
                    )
                    .ToDictionary(item => item.Name, item => item.Value),
                    jsonOptions
                );

            FilterJsonBeforeWrite(ref json, jsonOptions);
            return json;
        }

        /// <summary>
        /// Writes translations for a specific locale to a file
        /// </summary>
        /// <param name="filename">The path containing to contain the file to write to</param>
        /// <param name="localeInfo"></param>
        public void WriteToFile(string filename, LocaleInfo localeInfo)
        {
            WriteToFileAsync(filename, localeInfo).Wait();
        }

        /// <summary>
        /// Gets the Json data for a specific locale
        /// </summary>
        /// <param name="localeInfo">The locale to get the data for</param>
        /// <param name="sortOption">The sorting method of the items</param>
        /// <param name="jsonOptions">The Json Serializer Options to use when generating the Json</param>
        public void SetupLocaleForJson(LocaleInfo localeInfo, out SortOption sortOption, out JsonSerializerOptions jsonOptions)
        {
            jsonOptions = _defaultFileJsonOptions == null ? new JsonSerializerOptions() : new JsonSerializerOptions(_defaultFileJsonOptions);
            sortOption = _defaultFileSortOption;
            SetupWriteForLocale(localeInfo, ref sortOption, jsonOptions);
        }

        /// <summary>
        /// Gets the Json data for a specific locale
        /// </summary>
        /// <param name="localeInfo">The locale to get the data for</param>
        public void SetupLocaleForJson(LocaleInfo localeInfo)
        {
            SetupLocaleForJson(localeInfo, out _, out _);
        }

        /// <summary>
        /// Adds or replaces a Translation Item
        /// </summary>
        /// <param name="item">The item to add or replace</param>
        /// <returns>The item that was added or replaced</returns>
        public TranslationItem AddOrReplace(TranslationItem item)
        {
            return AddOrReplace(item.Name, item.Locale, item.Text, item.Order);
        }

        /// <summary>
        /// Adds or replaces mulitple Translation Items
        /// </summary>
        /// <param name="items">The items to add or replace</param>
        /// <returns>The items that were added or replaced</returns>
        public IEnumerable<TranslationItem> AddOrReplace(IEnumerable<TranslationItem> items)
        {
            var result = new List<TranslationItem>();
            foreach (var item in items)
                result.Add(AddOrReplace(item.Name, item.Locale, item.Text, item.Order));

            return result;
        }

        /// <summary>
        /// Gets the required translations need to ensure items are translated for all lacle
        /// </summary>
        /// <param name="preferredLocales">The locales to use as source locales from highest to lowest priority</param>
        /// <returns>An enumeration of translation items that are required</returns>
        public IEnumerable<Translation> RequiredTranslations(IEnumerable<LocaleInfo> preferredLocales)
        {
            var needed = NeededTranslations;
            var found = FoundTranslations;
            var prefLocales = preferredLocales.Select(locale => (LocaleInfo?)locale);

            return
                needed
                .Select(neededItem => new
                {
                    name = neededItem.Key,
                    sourceItem = GetItem(neededItem.Key, prefLocales.FirstOrDefault(locale => found[neededItem.Key].Contains(locale.Value)) ?? found[neededItem.Key].First()),
                    destinationLocales = neededItem.Value
                }).SelectMany(item => item.destinationLocales.Select(dl => new Translation() { From = item.sourceItem, ToLocale = dl }));
        }

        /// <summary>
        /// Gets the translation items sorted using a specific sort order
        /// </summary>
        /// <param name="localeInfo">The localisation info to get the ordered items for</param>
        /// <param name="sortOption">The sorting option to use to sort</param>
        /// <returns>The items in the correct sort order for the localisation</returns>
        public IEnumerable<TranslationItem> OrderedItems(LocaleInfo localeInfo, SortOption sortOption = SortOption.OriginalOrder)
        {
            return
                _items
                .Where(item => item.Value.ContainsKey(localeInfo))
                .OrderBy(item => sortOption == SortOption.OriginalOrder ? item.Value[localeInfo].Order : 0)
                .ThenBy(item => item.Value[localeInfo].Name)
                .Select(item => item.Value[localeInfo]);
        }

        /// <summary>
        /// Gets the locale information items in no particular order
        /// </summary>
        public IEnumerable<LocaleInfo> Locales => _locales;

        public string this[string name, LocaleInfo localeInfo, double order]
        {
            set => AddOrReplace(name, localeInfo, value, order);
        }

        /// <summary>
        /// Gets or sets the value for a translation item
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="localeInfo">The locale information</param>
        /// <returns>The text for the item</returns>
        public string this[string name, LocaleInfo localeInfo]
        {
            get => GetItem(name, localeInfo).Text;
            set => AddOrReplace(name, localeInfo, value);
        }

        /// <summary>
        /// Gets a dictionary of names and locale information for existing translations
        /// </summary>
        public IDictionary<string, IEnumerable<LocaleInfo>> FoundTranslations
        {
            get
            {
                return
                    _items
                    .ToImmutableDictionary(item => item.Key, item => (IEnumerable<LocaleInfo>)item.Value.Keys);
            }
        }

        /// <summary>
        /// Gets a list of test translations that can be performed
        /// </summary>
        /// <param name="sourceLocale">The source locale to use for testing</param>
        /// <returns>A list of Translations for each destination locale that is not the source</returns>
        public IEnumerable<Translation> TestableTranslations(LocaleInfo sourceLocale, Func<TranslationItem, bool> isTestable)
        {
            var testableSourceItems =
                _items
                .Where(item => item.Value.ContainsKey(sourceLocale))
                .Select(item => item.Value[sourceLocale])
                .Where(item => isTestable(item));

            return
                _locales
                .Where(destLocale => !destLocale.Equals(sourceLocale))
                .Select(destLocale => new ITranslator.Translation()
                    {
                        ToLocale = destLocale,
                        From = 
                            testableSourceItems
                            .Where(item => _items[item.Name].ContainsKey(destLocale))
                            .Select(item => item)
                            .FirstOrDefault()
                    }
                );
        }

        /// <summary>
        /// Gets a dicationary of names and locale information for needed translations
        /// </summary>
        public IDictionary<string, IEnumerable<LocaleInfo>> NeededTranslations
        {
            get
            {
                return
                    _locales
                    .SelectMany(locale => _items.Select(item => new { NamedItems = item, Locale = locale }))
                    .Where(item => !item.NamedItems.Value.ContainsKey(item.Locale))
                    .GroupBy(item => item.NamedItems.Key, item => item.Locale)
                    .ToImmutableDictionary(item => item.Key, item => (IEnumerable<LocaleInfo>)item);
            }
        }
    }
}