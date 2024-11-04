using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LanguageLocaliserTests")]

namespace LanguageLocaliser
{
    static class Program
    {
        public static ILogger Logger = new ConsoleLogger();

        public static void Main(string[] args)
        {
            var jsonPaths = args.Length == 1 ? args[0] : ".";
            Logger?.WriteLine("Starting");
            var ti = new RyujinxTranslationItems(Logger);
            ti.ReadFromFolderAsync(jsonPaths).Wait();
            var requiredTranslations = ti.RequiredTranslations([LocaleId.en_US.ToLocale(), LocaleId.fr_FR.ToLocale()]);
            var translations = new GoogleTranslator(Logger).Translate(requiredTranslations);
            ti.AddOrReplace(translations);
            ti.WriteToFolderAsync(jsonPaths).Wait();
            Logger?.WriteLine("Finished");
        }
    }
}