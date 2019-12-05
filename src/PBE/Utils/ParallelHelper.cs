using System.IO;

namespace PBE.Utils
{
    public class ParallelHelper
    {
        private static readonly object localMutex = new object();

        /// <summary>
        /// Liest den Inhalt der Datei ein. Dabei wird der Zugriff über einen Mutex abgesichert.
        /// Es ist das Gegenstück zu ParallelHelper.FileAppendAllText() im FS
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FileReadAllText(string filename)
        {
            lock (localMutex)
            {
                string mutexName = Path.GetFullPath(filename).ToLowerInvariant();
                using (FSUtils.GetMutex(mutexName))
                {
                    return File.ReadAllText(filename);
                }
            }
        }
    }
}