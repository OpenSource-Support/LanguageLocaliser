namespace LanguageLocaliser
{
    /// <summary>
    /// A simple logger that logs to the test output
    /// </summary>
    public class TestLogger : ILogger
    {
        /// <summary>
        /// Write a line to the log
        /// </summary>
        /// <param name="line">The line to write</param>
        public void WriteLine(string line)
        {
            TestContext.Out.WriteLine(line);
        }
    }
}