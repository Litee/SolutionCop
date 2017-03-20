namespace SolutionCop.CommandLine
{
    using SolutionCop.Core;

    internal interface ICommandLineParameters
    {
        string PathToSolution { get; set; }

        string PathToConfigFile { get; set; }

        ReportFormatType ReportFormat { get; set; }
    }
}