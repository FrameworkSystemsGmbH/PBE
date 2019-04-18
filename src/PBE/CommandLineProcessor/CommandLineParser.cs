using CommandLine;
using System;
using System.Collections.Generic;

namespace PBE.CommandLineProcessor
{
    public static class CommandLineParser
    {
        public static void ParseOptions(string[] args, Action<CommandLineOptions> action)
        {
            // Parse legacy options
            string[] modifiedArgs = ModifyLegacyOptions(args);

            CommandLineOptions options = new CommandLineOptions();

            CommandLine.Parser.Default
                .ParseArguments<CommandLineOptions>(modifiedArgs)
                .WithParsed<CommandLineOptions>(o => action(o));
        }

        private static string[] ModifyLegacyOptions(string[] args)
        {
            List<string> modifiedArgs = new List<string>();
            List<string> filters = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                if ("AUTO".Equals(args[i], StringComparison.OrdinalIgnoreCase))
                {
                    modifiedArgs.Add("--auto");
                }
                else if ("/FILTER".Equals(args[i], StringComparison.OrdinalIgnoreCase))
                {
                    if (i < args.Length-1)
                    {
                        filters.Add(args[++i]);
                    }
                }
                else
                {
                    modifiedArgs.Add(args[i]);
                }
            }

            if (filters.Count > 0)
            {
                modifiedArgs.Add("--filter");
                modifiedArgs.Add(string.Join(CommandLineOptions.SEPARATOR.ToString(), filters));
            }
            return modifiedArgs.ToArray();
        }
    }
}