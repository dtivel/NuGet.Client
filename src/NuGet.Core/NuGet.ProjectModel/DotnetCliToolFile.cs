using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace NuGet.ProjectModel
{
    public class DotnetCliToolFile
    {
        /// <summary>
        /// True if all packages were restored.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Tool id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Resolved tool version.
        /// </summary>
        public NuGetVersion Version { get; set; }

        /// <summary>
        /// Requested dependency range.
        /// </summary>
        public VersionRange RequestedRange { get; set;}

        /// <summary>
        /// Package folder and package fallback folders.
        /// These are ordered by precedence.
        /// </summary>
        public IList<string> PackageFolders { get; set; } = new List<string>();

        /// <summary>
        /// Framework -> Lib folder path
        /// </summary>
        public IDictionary<NuGetFramework, string> Targets { get; set; } = new Dictionary<NuGetFramework, string>();

        /// <summary>
        /// Restore errors and warnings.
        /// </summary>
        public IList<FileLogEntry> Log = new List<FileLogEntry>();
    }
}
