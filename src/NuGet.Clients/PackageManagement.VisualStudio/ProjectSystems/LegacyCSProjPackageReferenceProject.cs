using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;
using VSLangProj150;

namespace NuGet.PackageManagement.VisualStudio
{
/// <summary>
/// An implementation of <see cref="NuGetProject"/> that interfaces with VS project APIs to coordinate
/// packages in a legacy CSProj with package references.
/// </summary>
    public class LegacyCSProjPackageReferenceProject : NuGetProject, INuGetIntegratedProject
    {
        private readonly string _projectName;
        private readonly string _projectUniqueName;
        private readonly string _projectFullPath;
        private readonly PackageReferences _packageReferences;

        private readonly Func<PackageSpec> _packageSpecFactory;

        public LegacyCSProjPackageReferenceProject(
            string projectName, 
            string projectUniqueName, 
            string projectFullPath, 
            PackageReferences packageReferences,
            Func<PackageSpec> packageSpecFactory)
        {
            if (projectFullPath == null)
            {
                throw new ArgumentNullException(nameof(projectFullPath));
            }

            if (packageSpecFactory == null)
            {
                throw new ArgumentNullException(nameof(packageSpecFactory));
            }

            _projectName = projectName;
            _projectUniqueName = projectUniqueName;
            _projectFullPath = projectFullPath;
            _packageReferences = packageReferences;

            _packageSpecFactory = packageSpecFactory;

            InternalMetadata.Add(NuGetProjectMetadataKeys.Name, _projectName);
            InternalMetadata.Add(NuGetProjectMetadataKeys.UniqueName, _projectUniqueName);
            InternalMetadata.Add(NuGetProjectMetadataKeys.FullPath, _projectFullPath);
        }

        #region NuGetProject

        public override Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
        {
            //TODO: Have an API exposed (not written yet) which gives us a list of installed package names. From this we can use
            //      the TryGetReference API to fetch versions and any other required metadata.
            var list = Enumerable.Empty<PackageReference>();
            return System.Threading.Tasks.Task.FromResult(list);
        }

        public override async Task<Boolean> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            try
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                //TODO: add metadata
                _packageReferences..AddOrUpdate(packageIdentity.Id, packageIdentity.Version.ToString(), new string[] { }, new string[] { });
            }
            catch (Exception e)
            {
                nuGetProjectContext.Log(MessageLevel.Warning, e.Message, packageIdentity, _projectName);
                return false;
            }

            return true;
        }

        public override async Task<Boolean> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            try
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _packageReferences.Remove(packageIdentity.Id);
            }
            catch (Exception e)
            {
                nuGetProjectContext.Log(MessageLevel.Warning, e.Message, packageIdentity, _projectName);
                return false;
            }

            return true;
        }
#endregion
    }
}
