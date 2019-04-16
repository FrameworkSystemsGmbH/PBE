using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBE.CommandLineProcessor
{
    public class CommandLineOptions
    {
        public const char SEPARATOR = ':';
        public static readonly string[] EMPTY_ARRAY = new string[0];

        [Option('a', "auto", Required = false, HelpText = "Set automatic mode")]
        public bool Automatic { get; set; }

        [Option('c', "config", Required = false, HelpText = "Set configuration file to work with. If not set, PBE.xml in the current directory will be used.")]
        public string ConfigFilePath { get; set; }

        [Option('l', "logDir", Required = false, HelpText = "Set log file.")]
        public string LogFileDirectory { get; set; }

        [Option('f', "filter", Required = false, HelpText = "Set filter.", Separator = CommandLineOptions.SEPARATOR)]
        public IEnumerable<string> Filter { get; set; }
    }
}
