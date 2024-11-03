
using System.Net.Http.Headers;
using System.Text.Json;
using static LanguageLocaliser.ITranslator;

namespace LanguageLocaliser
{
    /// <summary>
    /// Encapsulates a translator that uses the public Google translation End Point
    /// </summary>
    public class GoogleTranslator : ITranslator
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Translates multiple source texts to a specific destination language
        /// </summary>
        /// <param name="sourceLang">The source language code (e.g. en, es)</param>
        /// <param name="targetLang">The destination language code (e.g. en, es)</param>
        /// <param name="texts">The texts to translate from sourceLang to targetLang</param>
        /// <returns>An array of strings containing the translated text</returns>
        public static async Task<string[]> TranslateTextsAsync(string sourceLang, string targetLang, IEnumerable<String> texts)
        {
            try
            {
                Console.WriteLine($"Requesting translation from {sourceLang} to {targetLang} for: \r\n- {String.Join("\r\n- ", texts)}");

                var encodedTexts = string.Join("&q=", texts.Select(t => Uri.EscapeDataString(t)));
                string url = $"https://translate.googleapis.com/translate_a/t?client=gtx&sl={sourceLang}&tl={targetLang}&dt=t&q={encodedTexts}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response - {jsonResponse}");

                var result = JsonDocument.Parse(jsonResponse);

                var translations = new string[result.RootElement.GetArrayLength()];

                for (int i = 0; i < translations.Length; i++)
                    translations[i] = result.RootElement[i].GetString();

                return translations;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Translates a set of Translations to TranslationItems
        /// </summary>
        /// <param name="items">The translations to translate</param>
        /// <returns>An enumerable of the translation items</returns>
        public IEnumerable<TranslationItem> Translate(IEnumerable<Translation> items)
        {
            var task = TranslateAsync(items);
            task.Wait();
            return task.Result;
        }

        public static string LanguageFor(LocaleInfo locale)
        {
            return $"{locale.Language}-{locale.Region}";
        }

        /// <summary>
        /// Translates a set of Translations to TranslationItems asynchronously
        /// </summary>
        /// <param name="items">The translations to translate</param>
        /// <returns>An enumerable of the translation items</returns>
        public async Task<IEnumerable<TranslationItem>> TranslateAsync(IEnumerable<Translation> items)
        {
            var result = new List<TranslationItem>();

            foreach(var sourceLang in items.Select(i => LanguageFor(i.From.Locale)).Distinct())
            {
                foreach(var targetLang in items.Where(i => LanguageFor(i.From.Locale) == sourceLang).Select(i => LanguageFor(i.ToLocale)).Distinct())
                {
                    var toTranslate = items.Where(i => LanguageFor(i.From.Locale) == sourceLang && LanguageFor(i.ToLocale) == targetLang);
                    var translation = await TranslateTextsAsync(sourceLang, targetLang, toTranslate.Select(i => i.From.Text));
                    result.AddRange(toTranslate.Select((i, idx) => new TranslationItem() 
                    { 
                        Locale = i.ToLocale, 
                        Name = i.From.Name, 
                        Text = translation[idx] 
                    }));
                }
            }

            return result;
        }
    }
}