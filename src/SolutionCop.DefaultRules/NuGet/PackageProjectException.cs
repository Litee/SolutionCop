namespace SolutionCop.DefaultRules.NuGet
{
    using System;
    using System.Xml.Linq;

    internal sealed class PackageProjectException
    {
        public const string ExceptionTagName = "Exception";
        public const string PackageExceptionTagName = "Package";
        public const string ProjectExceptionTagName = "Project";

        private static readonly string AvailableChildrenList = string.Join(", ", PackageExceptionTagName, ProjectExceptionTagName);

        public PackageProjectException(string projectId, string packageId)
        {
            ProjectId = projectId;
            PackageId = packageId;
        }

        public string ProjectId { get; }

        public string PackageId { get; }

        public static XElement Generate(string projectName, string packageId)
        {
            var result = new XElement(ExceptionTagName);

            if (!ReferenceEquals(projectName, null))
            {
                result.Add(new XElement(ProjectExceptionTagName, projectName));
            }

            if (!ReferenceEquals(packageId, null))
            {
                result.Add(new XElement(PackageExceptionTagName, packageId));
            }

            return result;
        }

        public static PackageProjectException Parse(XElement exceptionElement)
        {
            var currentTagName = exceptionElement.Name.LocalName;

            if (!string.Equals(currentTagName, ExceptionTagName, StringComparison.OrdinalIgnoreCase))
            {
                var message = $"Exception tag should have name {ExceptionTagName}. Current tag is {currentTagName}";

                throw new ArgumentException(message, nameof(currentTagName));
            }

            string packageId = null;
            string projectId = null;

            foreach (var child in exceptionElement.Descendants())
            {
                var nodeName = child.Name.LocalName;
                var nodeValue = child.Value;

                if (string.Equals(nodeName, PackageExceptionTagName, StringComparison.OrdinalIgnoreCase))
                {
                    packageId = nodeValue;
                }
                else if (string.Equals(nodeName, ProjectExceptionTagName, StringComparison.OrdinalIgnoreCase))
                {
                    projectId = nodeValue;
                }
                else
                {
                    var messager = $"Unknown child of {currentTagName}: '{nodeValue}'. Supported nodes are {AvailableChildrenList}";

                    throw new ArgumentException(messager);
                }
            }

            if (string.IsNullOrEmpty(packageId) && string.IsNullOrEmpty(projectId))
            {
                var message = $"At least one of the following exceptions should be declared: {AvailableChildrenList}";

                throw new InvalidOperationException(message);
            }

            return new PackageProjectException(projectId, packageId);
        }

        public bool Matches(string packageId, string projectName)
        {
            var matchesPackage = ReferenceEquals(PackageId, null) ||
                                 string.Equals(packageId, PackageId, StringComparison.OrdinalIgnoreCase);

            var matchesProject = ReferenceEquals(ProjectId, null) ||
                                 string.Equals(projectName, ProjectId, StringComparison.OrdinalIgnoreCase);

            return matchesPackage && matchesProject;
        }
    }
}