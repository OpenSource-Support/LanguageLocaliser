using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LanguageLocaliserTests")]

namespace LanguageLocaliser
{
    static class Program
    {
        const string jsonPaths = @"H:\Programming\Ryujinx\Ryujinx-GreemDev\src\Ryujinx\Assets\Locales";

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting");
            var ti = new RyujinxTranslationItems();
            ti.ReadFromFolderAsync(jsonPaths).Wait();
/*
            var testTranslations = ti.TestableTranslations(LocaleInfo.Create("en", "US"));
            var testResults = new GoogleTranslator().Translate(testTranslations.Where(tt => tt.From.Valid));

            foreach(var tr in testResults)
            {
                var tt = 
                    testTranslations
                    .Where(ttitem => ttitem.ToLocale.Equals(tr.Locale))
                    .FirstOrDefault();

                if (!tt.From.Valid)
                {
                    Console.WriteLine($"Could not find testable translation for language {tt.ToLocale}");
                }
                else 
                {
                    var existingValue = ti[tt.From.Name, tt.ToLocale];
                    if (existingValue != tr.Text)
                    {
                        Console.WriteLine($"Translation from {tt.From} to {tt.ToLocale} failed conversion test with existing value being '{existingValue}' and new value being '{tr.Text}'.");
                    }
                }
            }
*/
            
            var requiredTranslations = ti.RequiredTranslations([LocaleInfo.Create("en", "US"), LocaleInfo.Create("fr", "FR")]);
            var translations = new GoogleTranslator().Translate(requiredTranslations);
            ti.AddOrReplace(translations);
            ti.WriteToFolderAsync(jsonPaths).Wait();
        }
    }
}