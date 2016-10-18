﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using NuGet.ProjectManagement;
using NuGet.ProjectModel;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace NuGet.PackageManagement.VisualStudio
{
    /// <summary>
    /// Provides a method of creating <see cref="CpsPackageReferenceProject"/> instance.
    /// </summary>
    [Export(typeof(IProjectSystemProvider))]
    [Name(nameof(CpsPackageReferenceProjectProvider))]
    [Microsoft.VisualStudio.Utilities.Order(After = nameof(ProjectKNuGetProjectProvider))]
    public class CpsPackageReferenceProjectProvider : IProjectSystemProvider
    {
        private readonly IProjectSystemCache _projectSystemCache;

        [ImportingConstructor]
        public CpsPackageReferenceProjectProvider(IProjectSystemCache projectSystemCache)
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

            // The project must be an IVsHierarchy.
            var hierarchy = VsHierarchyUtility.ToVsHierarchy(dteProject);
            if (hierarchy == null)
            {
                return false;
            }

            if (!hierarchy.IsCapabilityMatch("CPS") || !hierarchy.IsCapabilityMatch("PackageReference"))
            {
                {
                    return false;
                }
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

            var unconfiguredProject = GetUnconfiguredProject(dteProject);

            result = new CpsPackageReferenceProject(
                dteProject.Name,
                dteProject.UniqueName,
                fullProjectPath,
                packageSpecFactory,
                dteProject,
                unconfiguredProject);

            return true;
        }

        private UnconfiguredProject GetUnconfiguredProject(EnvDTE.Project project)
        {
            IVsBrowseObjectContext context = project as IVsBrowseObjectContext;
            if (context == null && project != null)
            { // VC implements this on their DTE.Project.Object
                context = project.Object as IVsBrowseObjectContext;
            }

            return context != null ? context.UnconfiguredProject : null;
         }
    }
}
