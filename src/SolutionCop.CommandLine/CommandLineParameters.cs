using CommandLine;
using CommandLine.Text;

namespace SolutionCop.CommandLine
{
    class CommandLineParameters
    {
        [Option('s', "solution", Required = true)]
        public string PathToSolution { get; set; }

        [Option('c', "config")]
        public string PathToConfigFile { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
