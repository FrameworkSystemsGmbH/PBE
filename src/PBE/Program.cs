using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

using PBE.CommandLineProcessor;

namespace PBE
{
    public class Program
    {
        private static void Main(string[] args)
        {
            CommandLineParser.ParseOptions(args, RunPBE);
        }

        private static void RunPBE(CommandLineOptions options)
        {
            PBEContext.Create(options);
            new ExecutableContainer(options).Execute();
        }
    }
}