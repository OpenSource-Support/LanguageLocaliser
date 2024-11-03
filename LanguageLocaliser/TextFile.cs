using System.Text;
using System.Text.RegularExpressions;

namespace LanguageLocaliser
{
    /// <summary>
    /// Contains utility functions to be used on text files
    /// </summary>
    internal static class TextFile
    {
        /// <summary>
        /// Detects the line ending format for a text file
        /// </summary>
        /// <param name="filename">The text file to detect the line ending format for</param>
        /// <returns>Returns either #13#10 or #13 depending on the detected line ending</returns>
        /// <exception cref="InvalidOperationException">If line ending detecetion fails</exception>
        public static string DetectLineEnding(string filename)
        {
            var text = File.ReadAllText(filename);

            var lineRNCount = new Regex(Regex.Escape("\r\n")).Matches(text).Count;
            var lineRCount = new Regex(Regex.Escape("\n")).Matches(text).Count - lineRNCount;

            if (lineRNCount > 0 && lineRCount > 0)
                throw new InvalidOperationException("File has both types of line endings.");
            else
                if (lineRNCount > 0) return "\r\n";
            else
                if (lineRCount > 0) return "\n";
            else
                throw new InvalidOperationException("File has no lines.");
        }

        /// <summary>
        /// Opens a text file for writing, ensuring that the same encoding is used as the original file (even if the original file has no encoding byte marks)
        /// </summary>
        /// <param name="outFilename">The filename to write to</param>
        /// <param name="append">True to append to the file or false to overwrite</param>
        /// <param name="encoding">The encodign to use, or Null if no encoding byte marks are to be written</param>
        /// <returns>A new stream reader using the correct encoding value</returns>
        /// <remarks>
        /// Just passing in the encoding does not work as the StreamReader errors when encoding is Null, 
        /// so we need to use the overload when its Null instead to ensure byte order marks are not written
        /// </remarks>
        public static StreamWriter EncodingCompatibleStreamWriter(string outFilename, bool append, Encoding encoding)
        {
            if (encoding == null)
                return new StreamWriter(outFilename, append);
            else
                return new StreamWriter(outFilename, append, encoding);
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        /// <remarks>
        /// Using StreamReader.DetectEncodingFromByteOrderMarks does not work as it default to Utf8 if no marks are 
        /// found when it should return Null
        /// </remarks>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                if (file.Read(bom, 0, 4) != 4)
                    return null;
            }

            // Analyze the BOM
#pragma warning disable SYSLIB0001
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
#pragma warning restore SYSLIB0001

            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

            // Return null if cant detect
            return null;
        }
    }
}
