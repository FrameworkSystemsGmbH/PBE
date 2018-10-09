using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace PBE
{
    internal class Program
    {
        public static bool Automatic = false;
        public static string Directory = String.Empty;
        public static HashSet<String> TaskFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static void Main(string[] args)
        {
            #region parse Arguments

            if (args.Length >= 1)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if ("AUTO".Equals(args[i], StringComparison.OrdinalIgnoreCase))
                    {
                        Automatic = true;
                    }

                    if ("/FILTER".Equals(args[i], StringComparison.OrdinalIgnoreCase))
                    {
                        if (i <= args.Length)
                        {
                            i++;
                            TaskFilters.Add(args[i]);
                        }
                    }
                }
            }

            #endregion parse Arguments

            FileInfo fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            Program.Directory = fi.DirectoryName;
            string xmlFile = Path.Combine(Program.Directory, "PBE.xml");
            Console.WriteLine(xmlFile);
            new ExecutableContainer(XElement.Load(xmlFile)).Execute();
        }
    }
}