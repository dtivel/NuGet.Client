using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using NuGet.LibraryModel;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using VSLangProj;
using VSLangProj150;


namespace NuGet.PackageManagement.VisualStudio
{
/// <summary>
/// An implementation of <see cref="NuGetProject"/> that interfaces with VS project APIs to coordinate
/// packages in a legacy CSProj with package references.
/// </summary>
    public class LegacyCSProjPackageReferenceProject : CpsPackageReferenceProjectBase, IDependencyGraphProject
    {
        private readonly EnvDTE.Project _dteProject;
        private readonly string _projectFullPath;
        private readonly PackageReferences _packageReferences;

        public LegacyCSProjPackageReferenceProject(
            EnvDTE.Project dteProject,
            PackageReferences packageReferences)
        {
            if (_dteProject == null)
            {
                throw new ArgumentNullException(nameof(dteProject));
            }

            _projectFullPath = EnvDTEProjectUtility.GetFullProjectPath(dteProject);
            _dteProject = dteProject;
            _packageReferences = packageReferences;

            var _baseIntermediatePath = _dteProject.Properties.Item("IntermediatePath").Value as string;
            PathToAssetsFile = _baseIntermediatePath;

            InternalMetadata.Add(NuGetProjectMetadataKeys.Name, _dteProject.Name);
            InternalMetadata.Add(NuGetProjectMetadataKeys.UniqueName, _dteProject.UniqueName);
            InternalMetadata.Add(NuGetProjectMetadataKeys.FullPath, _projectFullPath);
        }

        #region IDependencyGraphProject

        /// <summary>
        /// Making this timestamp as the current time means that a restore with this project in the graph
        /// will never no-op. We do this to keep this work-around implementation simple.
        /// </summary>
        public DateTimeOffset LastModified => DateTimeOffset.Now;

        public string MSBuildProjectPath => _projectFullPath;


        public IReadOnlyList<PackageSpec> GetPackageSpecsForRestore(ExternalProjectReferenceContext context)
        {
            //TODO: Tools package spec as well?
            return new[] { GetNonToolsPackageSpec() };
        }

        public Boolean IsRestoreRequired(IEnumerable<VersionFolderPathResolver> pathResolvers, ISet<PackageIdentity> packagesChecked, ExternalProjectReferenceContext context)
        {
            //TODO: Make a real evaluation here.
            return true;
        }

        public async Task<IReadOnlyList<ExternalProjectReference>> GetProjectReferenceClosureAsync(
            ExternalProjectReferenceContext context)
        {
            await TaskScheduler.Default;

            var externalProjectReferences = new HashSet<ExternalProjectReference>();

            var packageSpec = GetNonToolsPackageSpec();
            if (packageSpec != null)
            {
                var projectReferences = GetProjectReferences(packageSpec);

                var reference = new ExternalProjectReference(
                    packageSpec.RestoreMetadata.ProjectPath,
                    packageSpec,
                    packageSpec.RestoreMetadata.ProjectPath,
                    projectReferences);

                externalProjectReferences.Add(reference);
            }

            return DependencyGraphProjectCacheUtility
                .GetExternalClosure(_projectFullPath, externalProjectReferences)
                .ToList();
        }

        private static string[] GetProjectReferences(PackageSpec packageSpec)
        {
            // There is only one target framework for legacy csproj projects
            var targetFramework = packageSpec.TargetFrameworks.FirstOrDefault();
            if (targetFramework == null)
            {
                return new string[] { };
            }

            return targetFramework.Dependencies
                .Where(d => d.LibraryRange.TypeConstraint == LibraryDependencyTarget.ExternalProject)
                .Select(d => d.LibraryRange.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private PackageSpec GetNonToolsPackageSpec()
        {
            var projectReferences = GetProjectReferences().Select(ToProjectLibraryDependency);
            var packageReferences = GetInstalledPackagesAsync(CancellationToken.None).Result.Select(ToPackageLibraryDependency);

            var theOnlyTfi = new TargetFrameworkInformation()
            {
                FrameworkName = EnvDTEProjectUtility.GetTargetNuGetFramework(_dteProject),
                Dependencies = projectReferences.Concat(packageReferences).ToList()
            };

            var tfis = new TargetFrameworkInformation[] { theOnlyTfi };
            var packageSpec = new PackageSpec(tfis)
            {
                RestoreMetadata = new ProjectRestoreMetadata
                {
                    OutputType = RestoreOutputType.NETCore,
                    OriginalTargetFrameworks = tfis
                        .Select(tfi => tfi.FrameworkName.GetShortFolderName())
                        .ToList(),
                    ProjectPath = _projectFullPath
                }
            };

            return packageSpec;
        }

        private static LibraryDependency ToProjectLibraryDependency(ProjectRestoreReference item)
        {
            var dependency = new LibraryDependency
            {
                LibraryRange = new LibraryRange(
                    name: item.ProjectUniqueName,
                    versionRange: VersionRange.All,
                    typeConstraint: LibraryDependencyTarget.ExternalProject)
            };

            return dependency;
        }

        private static LibraryDependency ToPackageLibraryDependency(PackageReference item)
        {
            var dependency = new LibraryDependency
            {
                LibraryRange = new LibraryRange(
                    name: item.PackageIdentity.Id,
                    versionRange: item.AllowedVersions,
                    typeConstraint: LibraryDependencyTarget.Package)
            };

            return dependency;
        }

        private IEnumerable<ProjectRestoreReference> GetProjectReferences()
        {
            var vsProject = _dteProject.Object as VSProject;
            if (vsProject == null)
            {
                yield break;
            }

            foreach (Reference reference in vsProject.References)
            {
                yield return new ProjectRestoreReference()
                {
                    ProjectUniqueName = reference.Name,
                    ProjectPath = reference.Path
                };
            }
        }

        private Task<IEnumerable<PackageReference>> GetPackageReferences()
        {
            return GetInstalledPackagesAsync(CancellationToken.None);
        }
        #endregion

        #region NuGetProject

        public override Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
        {
            //TODO: Have an API exposed (not written yet) which gives us a list of installed package names. From this we can use
            //      the TryGetReference API to fetch versions and any other required metadata.
            //      Populate at least identity.id and allowedversions
            //TODO: Cache this information as this method gets called multiple times from various threads.
            var list = Enumerable.Empty<PackageReference>();
            return System.Threading.Tasks.Task.FromResult(list);
        }

        public override async Task<Boolean> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            try
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                //TODO: add metadata
                _packageReferences.AddOrUpdate(packageIdentity.Id, packageIdentity.Version.ToString(), new string[] { }, new string[] { });
            }
            catch (Exception e)
            {
                nuGetProjectContext.Log(MessageLevel.Warning, e.Message, packageIdentity, _dteProject.Name);
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
                nuGetProjectContext.Log(MessageLevel.Warning, e.Message, packageIdentity, _dteProject.Name);
                return false;
            }

            return true;
        }
        #endregion
    }
}
