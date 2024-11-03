using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LanguageLocaliserTests")]

namespace LanguageLocaliser
{
    static class Program
    {
        public static void Main(string[] args)
        {   
            var jsonPaths = args.Count() == 1 ? args[0] : ".";
            Console.WriteLine("Starting");
            var ti = new RyujinxTranslationItems();
            ti.ReadFromFolderAsync(jsonPaths).Wait();
            var requiredTranslations = ti.RequiredTranslations([LocaleInfo.Create("en", "US"), LocaleInfo.Create("fr", "FR")]);
            var translations = new GoogleTranslator().Translate(requiredTranslations);
            ti.AddOrReplace(translations);
            ti.WriteToFolderAsync(jsonPaths).Wait();
        }
    }
}