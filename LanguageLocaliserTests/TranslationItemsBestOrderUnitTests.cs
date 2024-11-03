namespace LanguageLocaliser
{
    [TestFixture]
    public class TranslationItemsBestOrderUnitTests
    {
        private TranslationItems _items;

        private LocaleInfo _localeInfo;

        [SetUp]
        public void SetUp()
        {
            var json = @"
                {
                    ""AppleOrange1"": ""Apple Orange 1"",
                    ""AppleOrange2"": ""Apple Orange 2"",
                    ""AppleOrange3"": ""Apple Orange 3"",
                    ""AppleLime1"": ""Apple Lime 1"",
                    ""AppleLime3"": ""Apple Lime 3"",
                    ""AppleZingzang1"": ""Apple Zingzang 1""
                }
            ";
            _localeInfo = new LocaleInfo() { Language = "en", Region = "US", Name = "English (US)"};
            _items = new();
            _items.ReadFromJson(ref _localeInfo, json);
        }

        [Test]
        [TestCase("Top of List - No Match", "0AppleOrange", 0)]
        [TestCase("Top of List - Partial Match", "AppleOrange", 0)]
        [TestCase("Bottom of List - No Match", "ZapleOrange", 6)]
        [TestCase("Bottom of List - Partial Match", "AppleZingzang2", 6)]
        [TestCase("Middle of List - Partial Match End", "AppleOrange2.5", 2)]
        [TestCase("Middle of List - Top of Partial Match", "AppleLime0", 3)]
        [TestCase("Middle of List - Middle of Partial Match", "AppleLime2", 4)]
        [TestCase("Middle of List - Bottom of Partial Match", "AppleLime4", 5)]
        public void CorrectBestOrder(string testName, string itemName, int expectedIndex)
        {
            _items.AddOrReplace(new TranslationItem() { Locale = _localeInfo, Name = itemName, Text = itemName});
            var indexOf = _items.OrderedItems(_localeInfo, TranslationItems.SortOption.OriginalOrder).FirstIndexOf((idx, item) => item.Name == itemName);
            Assert.That(
                indexOf, 
                Is.EqualTo(expectedIndex),
                $"{nameof(CorrectBestOrder)} test '{testName}' failed with " +
                $"index of {indexOf} when expecting index of {expectedIndex}.");
        }

        [Test]
        public void NoItems()
        {
            var itemName = "AppleLime0";
            var expectedIndex = 0;
            var expectedOrder = 1.0;
            _items.ClearTranslations();
            _items.AddOrReplace(new TranslationItem() { Locale = _localeInfo, Name = "AppleLime0", Text = "Apple Lime Zero"});
            var orderedItems = _items.OrderedItems(_localeInfo, TranslationItems.SortOption.OriginalOrder);
            var indexOf = orderedItems.FirstIndexOf((idx, item) => item.Name == itemName);
            
            var orderOf = orderedItems.First().Order;
            Assert.Multiple(() =>
            {
                Assert.That(
                            orderedItems.FirstIndexOf((idx, item) => item.Name == itemName),
                            Is.EqualTo(expectedIndex),
                            $"{nameof(CorrectBestOrder)} test failed with " +
                            $"index of {indexOf} when expecting index of {expectedIndex}.");
                Assert.That(
                    orderOf,
                    Is.EqualTo(expectedOrder),
                    $"{nameof(CorrectBestOrder)} test failed with " +
                    $"Order of {orderOf} when expecting index of {expectedOrder}.");
            });
        }

        [TearDown]
        public void TeadDown()
        {
            _localeInfo = new LocaleInfo();
            _items = null;
        }
    }
}