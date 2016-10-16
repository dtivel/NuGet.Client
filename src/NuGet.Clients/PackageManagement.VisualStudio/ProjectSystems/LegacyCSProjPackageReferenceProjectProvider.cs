// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using NuGet.ProjectManagement;
using NuGet.ProjectModel;
using VSLangProj150;

namespace NuGet.PackageManagement.VisualStudio
{
    [Export(typeof(IProjectSystemProvider))]
    [Name(nameof(LegacyCSProjPackageReferenceProjectProvider))]
    [Order(After = nameof(CpsPackageReferenceProjectProvider))]
    public class LegacyCSProjPackageReferenceProjectProvider : IProjectSystemProvider
    {
        private readonly IProjectSystemCache _projectSystemCache;

        [ImportingConstructor]
        public LegacyCSProjPackageReferenceProjectProvider(IProjectSystemCache projectSystemCache)
        {
            if (projectSystemCache == null)
            {
                throw new ArgumentNullException(nameof(projectSystemCache));
            }

            _projectSystemCache = projectSystemCache;
        }
        
        public bool TryCreateNuGetProject(EnvDTE.Project dteProject, ProjectSystemProviderContext context, out NuGetProject result)
        {
            if (dteProject == null)
            {
                throw new ArgumentNullException(nameof(dteProject));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            result = null;

            // The project must be an IVsHierarchy and not CPS.
            var hierarchy = VsHierarchyUtility.ToVsHierarchy(dteProject);
            if (hierarchy == null)
            {
                return false;
            }

            if (hierarchy.IsCapabilityMatch("CPS"))
            {
                return false;
            }

            // The project must cast to VSProject4 --this indicates that it's a PackagesReference project
            var vsProject4 = dteProject.Object as VSProject4;
            if (vsProject4 == null)
            {
                return false;
            }

            var packageReferences = vsProject4.PackageReferences;
            if (packageReferences == null)
            {
                return false;
            }

            var projectNames = ProjectNames.FromDTEProject(dteProject);
            var fullProjectPath = EnvDTEProjectUtility.GetFullProjectPath(dteProject);

            Func<PackageSpec> packageSpecFactory = () =>
            {
                PackageSpec packageSpec;
                if (_projectSystemCache.TryGetProjectRestoreInfo(fullProjectPath, out packageSpec))
                {
                    return packageSpec;
                }

                return null;
            };

            result = new LegacyCSProjPackageReferenceProject(
                dteProject.Name,
                dteProject.UniqueName,
                fullProjectPath,
                packageReferences,
                packageSpecFactory);

            return true;
        }
    }
}
