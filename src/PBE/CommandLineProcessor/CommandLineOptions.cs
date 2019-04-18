using CommandLine;
using System.Collections.Generic;

namespace PBE.CommandLineProcessor
{
    public class CommandLineOptions
    {
        public const char SEPARATOR = ':';

        [Option('a', "auto", Required = false, HelpText = "Automatic mode. Starts processing without prompts.")]
        public bool Automatic { get; set; }

        [Option('c', "config", Required = false, Default = "PBE.xml", HelpText = "Set configuration file to work with.")]
        public string ConfigFilePath { get; set; }

        [Option('l', "logDir", Required = false, HelpText = "Set directory for log file.")]
        public string LogFileDirectory { get; set; }

        [Option('f', "filter", Required = false, HelpText = "Set filter. Multiple filters are separated with ':'. Example: --filter Task1:Task2", Separator = CommandLineOptions.SEPARATOR)]
        public IEnumerable<string> Filter { get; set; }
    }
}