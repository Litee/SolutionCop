using System.Xml.Linq;
using SolutionCop.DefaultRules.Basic;

namespace SolutionCop.MSBuild
{
    using System.Diagnostics;
    using Microsoft.Build.Utilities;

    public class SolutionCopTask : Task
    {
        static SolutionCopTask()
        {
#if DEBUG
            // Add the Default Listener back as ReSharper removes it
            Trace.Listeners.Add(new DefaultTraceListener());
#endif
        }

        public string PathToConfig { get; set; }

        public override bool Execute()
        {
            var validationResult = new WarningLevelRule().ValidateAllProjects(new XElement("WarningLevel"), PathToConfig);
            if (validationResult.Errors.Length > 0)
            {
                foreach (var error in validationResult.Errors)
                {
                    Log.LogError("SolutionCop: " + error);
                }
                return false;
            }
            return true;
        }
    }
}