using LanguageLocaliser;
using NUnit.Framework;

namespace LanguageLocaliserTests
{
    [TestFixture]
    public class CountMatchingTests
    {
        /// <summary>
        /// Tests the count matching IEnumerable extension method
        /// </summary>
        /// <param name="testName">The name of the test</param>
        /// <param name="firstValue">The first IEnumerable value</param>
        /// <param name="secondValue">The second IEnumerable value</param>
        /// <param name="start">The index to start searching at</param>
        /// <param name="expectedResult">The expected number of matching contiguous elements</param>
        [Test]
        [TestCase("Partial Match from Start", "appleHere", "appleThere", 0, 5)]
        [TestCase("Full Match from Start", "apples", "apples", 0, 6)]
        [TestCase("No Match from Start", "apple", "banana", 0, 0)]
        [TestCase("Partial Match", "1234pineappleHere", "4321pineappleThereitIs", 4, 9)]
        [TestCase("No Match", "1234appleHere", "4321appleThere", 0, 0)]
        [TestCase("No Match - Empty", "", "", 0, 0)]
        [TestCase("No Match - Different Lengths", "apple", "orange", 0, 0)]
        [TestCase("Partial Match - Different Lengths", "apple", "apples", 0, 5)]
        public void Correct(string testName, string firstValue, string secondValue, int start, int expectedResult)
        {
            var actualResult = firstValue.CountMatching(secondValue, start);
            Assert.That(
                expectedResult, Is.EqualTo(actualResult),
                $"{nameof(Correct)} test '{testName}' failed with parameters " +
                $"('{firstValue}', '{secondValue}', '{start}') " +
                $"expected value {expectedResult} but returned {actualResult}");
        }
    }

}