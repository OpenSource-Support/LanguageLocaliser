using LanguageLocaliser;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using static LanguageLocaliser.ITranslator;

namespace LanguageLocaliserTests
{
    [TestFixture]
    public class GoogleTranslatorTests
    {
        private GoogleTranslator _translator = new(new TestLogger());

        /// <summary>
        /// Tests raw translation of some languages
        /// </summary>
        /// <param name="testName">The test name</param>
        /// <param name="sourceLocaleId">The source locale Id to translate from</param>
        /// <param name="destLocaleId">The destination locale Id to translate to</param>
        /// <param name="text">The text in the source language to translate</param>
        /// <param name="matchingText">The text to match for the translation test</param>
        [Test]
        [TestCase("English to French", "en_US", "fr_FR", new string[] { "Hello", "Goodbye" }, new string[] { "Bonjour", "Au revoir" })]
        [TestCase("English to Traditional Chinese", "en_US", "zh_TW", new string[] { "Hello", "Goodbye" }, new string[] { "你好", "再見" })]
        [TestCase("English to Simplified Chinese", "en_US", "zh_CN", new string[] { "Hello", "Goodbye" }, new string[] { "你好", "再见" })]
        [TestCase("English to French (Sentence with Substitute)", "en_US", "fr_FR", new string[] { "Hello, my name is {0}." }, new string[] { "Bonjour, je m'appelle {0}." })]
        public void Correct(string testName, string sourceLocaleId, string destLocaleId, string[] text, string[] matchingText)
        {
            var translations =
                _translator.TranslateTexts(
                    GoogleTranslator.LanguageFor(LocaleInfo.Create(sourceLocaleId).Value),
                    GoogleTranslator.LanguageFor(LocaleInfo.Create(destLocaleId).Value),
                    text
                );

            Assert.That(
                translations,
                Has.Length.EqualTo(text.Length),
                $"TranslateTexts `{testName}` returned incorrect number of translations, count was {translations} expected count was {text.Length}");

            foreach (var (expected, result) in matchingText.Zip(translations))
                Assert.That(
                    result,
                    Is.EqualTo(expected),
                    $"TranslateTexts `{testName} returned a translation of `{result}` when `{expected}` was expected.");
        }

        /// <summary>
        /// Tests bulk translation of muiltiple items for different languages
        /// </summary>
        [Test]
        public void BulkTest()
        {
            var items = new[] {
                new { localeFrom = LocaleId.en_US, localeTo = LocaleId.fr_FR, name = "E1", text = "Hello", expected = "Bonjour" },
                new { localeFrom = LocaleId.en_US, localeTo = LocaleId.fr_FR, name = "E2", text = "Goodbye", expected = "Au revoir" },
                new { localeFrom = LocaleId.en_US, localeTo = LocaleId.fr_FR, name = "E3", text = "Nice to see you", expected = "Ravi de vous voir" },
                new { localeFrom = LocaleId.en_US, localeTo = LocaleId.fr_FR, name = "E4", text = "Have a happy holiday", expected = "Passez de bonnes vacances" },
                new { localeFrom = LocaleId.fr_FR, localeTo = LocaleId.en_US, name = "F1", text = "Bonjour", expected = "Good morning" },
                new { localeFrom = LocaleId.fr_FR, localeTo = LocaleId.en_US, name = "F2", text = "Au revoir", expected = "Bye" },
                new { localeFrom = LocaleId.fr_FR, localeTo = LocaleId.en_US, name = "F3", text = "Ravi de vous voir", expected = "nice to see you" },
                new { localeFrom = LocaleId.fr_FR, localeTo = LocaleId.en_US, name = "F4", text = "Passez de bonnes vacances", expected = "Have a great vacation" }
            };

            var translations =
                items
                .Select(item => new Translation()
                {
                    From = new()
                    {
                        Name = item.name,
                        Locale = item.localeFrom.ToLocale(),
                        Text = item.text
                    },
                    ToLocale = item.localeTo.ToLocale()
                });

            var results = _translator.Translate(translations);

            Assert.That(
                items,
                Has.Length.EqualTo(results.Count()),
                $"TranslateTexts `BulkTest` returned incorrect number of translations, count was {results.Count()} expected count was {items.Count()}");

            foreach (var item in items)
            {
                var result = results.Where(result => item.name == result.Name).Single().Text;
                Assert.That(
                    item.expected,
                    Is.EqualTo(result),
                    $"TranslateTexts `Bulktest` returned a translation of `{result}` when `{item.expected}` was expected.");
            }
        }
    }
}