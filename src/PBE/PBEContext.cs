using PBE.CommandLineProcessor;
using System;
using System.Collections.Generic;
using System.IO;

namespace PBE
{
    public class PBEContext
    {
        public static PBEContext CurrentContext { get; private set; }

        public static void Create(CommandLineOptions options)
        {
            CurrentContext = new PBEContext(options);
        }

        public readonly HashSet<string> TaskFilters;

        private PBEContext(CommandLineOptions options)
        {
            this.AutomaticMode = options.Automatic;
            this.TaskFilters = new HashSet<string>(options.Filter);
            this.ConfigFile = GetPath(options.ConfigFilePath, "PBE.xml", true);

            // LogFile Directory aus den Options. Ansonsten wird das Verzeichnis der XML-Datei gezogen.
            this.LogFileDirectory = GetPath(options.LogFileDirectory, Path.GetDirectoryName(this.ConfigFile), false);

            // Wenn im LogDir ein Template liegt, dieses nehmen, ansonsten aus dem Programm-Verzeichnis.
            this.LogFileTemplate = GetPath(Path.Combine(this.LogFileDirectory, "LogTemplate.htm"), "LogTemplate.htm", true);
        }

        private string GetPath(string optionPath, string defaultPath, bool bIsFile)
        {
            string fileName = string.IsNullOrWhiteSpace(optionPath)
                ? defaultPath : optionPath;

            FileSystemInfo fsi = null;
            if (File.Exists(fileName))
                fsi = new FileInfo(fileName);
            else if (Directory.Exists(fileName))
                fsi = new DirectoryInfo(fileName);

            if (fsi == null)
                throw new FileNotFoundException($"File not found or not accessible: {fileName}", fileName);

            if (bIsFile && !(fsi is FileInfo))
                throw new InvalidOperationException($"'{fileName}' is not a file");

            return fsi.FullName;
        }

        public bool AutomaticMode { get; private set; }
        public string Filter { get; private set; }
        public string ConfigFile { get; private set; }
        public string LogFileDirectory { get; private set; }
        public string LogFileTemplate { get; private set; }
    }
}