using System;

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
    }
}