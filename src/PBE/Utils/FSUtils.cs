using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PBE.Utils
{
    public class FSUtils
    {
        /// <summary>
        /// Erzeugt einen Mutex mit dem übergebenen Namen und wartet ggf. darauf bis dieser von einem anderen Prozess wieder freigegeben wird.
        /// Beim Dispose des zurückgegebenen Objektes wird der Mutex freigegeben. Am besten mit einem using verwenden.
        /// <code>
        /// using (FSUtils.GetMutex("MyMutex"))
        /// { /* aktion */ }
        /// </code>
        /// </summary>
        /// <param name="mutexName">Es kann auch ein Dateiname übergeben werden.</param>
        /// <returns></returns>
        public static IDisposable GetMutex(string mutexName)
        {
            return new FSMutex(mutexName);
        }

        // Kopie aus Framework-Studio. Die Logik hier muss identisch mit FS sein!!
        private class FSMutex : IDisposable
        {
            private System.Threading.Mutex mutex;

            public FSMutex(string mutexName)
            {
                bool mutexWasCreated;

                // Ein zu langer Dateiname könnte zu Exceptions führen
                if (mutexName.Length > 100)
                {
                    mutexName = mutexName.Substring(0, 50) + Math.Sign(mutexName.GetHashCode()) + mutexName.Substring(mutexName.Length - 50);
                }

                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    mutexName = mutexName.Replace(c, '_');
                }

                this.mutex = new System.Threading.Mutex(true, mutexName, out mutexWasCreated);
                try
                {
                    if (!mutexWasCreated)
                    {
                        this.mutex.WaitOne();
                    }
                }
                catch (System.Threading.AbandonedMutexException) { }
            }

            public void Dispose()
            {
                this.mutex.ReleaseMutex();
                this.mutex.Dispose();
            }
        }

        #region EscapeCommandLineArgs

        private static readonly Regex cmdEscape_InvalidChar = new Regex("[\x00\x0a\x0d]", RegexOptions.Compiled); //  these can not be escaped
        private static readonly Regex cmdEscape_needsQuotes = new Regex(@"\s|""", RegexOptions.Compiled); //          contains whitespace or two quote characters
        private static readonly Regex cmdEscape_escapeQuote = new Regex(@"(\\*)(""|$)", RegexOptions.Compiled); //    one or more '\' followed with a quote or end of string

        /// <summary>
        /// Wandelt den übergebenen String so um dass er als Command-Line-Argument verwendet werden kann.
        /// Dabei werden Leerzeichen, \ und " escaped.
        /// </summary>
        public static string EscapeCommandLineArgs(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return "\"\"";

            if (cmdEscape_InvalidChar.IsMatch(arg))
                throw new ArgumentOutOfRangeException("arg", "Argument contains invalid chars");

            if (!cmdEscape_needsQuotes.IsMatch(arg))
                return arg;

            return "\"" +
                cmdEscape_escapeQuote.Replace(arg, m =>
                    m.Groups[1].Value + m.Groups[1].Value + (m.Groups[2].Value == "\"" ? "\\\"" : "")) +
                "\"";
        }

        /// <summary>
        /// Wandelt die ünergebenen String-Argumente in einen string mit Command-Line-Arguments um.
        /// Dabei werden Leerzeichen, \ und " escaped.
        /// </summary>
        public static string EscapeCommandLineArgs(IEnumerable<string> args)
        {
            if (args == null)
                return "";

            return string.Join(" ", args.Select(EscapeCommandLineArgs));
        }

        /// <summary>
        /// Wandelt die ünergebenen String-Argumente in einen string mit Command-Line-Arguments um.
        /// Dabei werden Leerzeichen, \ und " escaped.
        /// </summary>
        public static string EscapeCommandLineArgs(params string[] args)
        {
            if (args == null)
                return "";

            return string.Join(" ", args.Select(EscapeCommandLineArgs));
        }

        #endregion EscapeCommandLineArgs
    }
}