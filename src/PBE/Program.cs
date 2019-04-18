using PBE.CommandLineProcessor;

namespace PBE
{
    public class Program
    {
        private static void Main(string[] args)
        {
            CommandLineParser.ParseOptions(args, options =>
            {
                PBEContext.Create(options);
                new ExecutableContainer(options).Execute();
            });
        }
    }
}