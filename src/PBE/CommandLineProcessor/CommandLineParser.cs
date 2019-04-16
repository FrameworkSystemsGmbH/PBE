using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            List<string> modifiedArgs = new List<string>(args.Length);
            List<string> filters = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                bool bIsAutomaticArg = "/AUTO".Equals(args[i], StringComparison.OrdinalIgnoreCase);
                bool bIsFilterArg = "/FILTER".Equals(args[i], StringComparison.OrdinalIgnoreCase);

                if (bIsAutomaticArg) args[i] = "--auto";

                if (!bIsFilterArg)
                {
                    modifiedArgs.Add(args[i]);
                }
                else if (i <= args.Length)
                {
                    filters.Add(args[++i]);
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
