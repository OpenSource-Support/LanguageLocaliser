namespace LanguageLocaliser
{
    /// <summary>
    /// Implements a simple logger interface
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Write a line to the log
        /// </summary>
        /// <param name="line">The line to write</param>
        void WriteLine(string line);
    }
}