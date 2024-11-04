namespace LanguageLocaliser
{
    /// <summary>
    /// A simple logger that logs to the console
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Write a line to the log
        /// </summary>
        /// <param name="line">The line to write</param>
        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }
}