// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuGet.ProjectManagement.Projects
{
    public abstract class ProjectKNuGetProjectBase : NuGetProject, INuGetIntegratedProject
    {
        public Task<bool> ExecuteInitScriptAsync(
            PackageIdentity identity,
            string packageInstallPath,
            INuGetProjectContext projectContext,
            bool throwOnFailure)
        {
            // No-op for ProjectK projects
            return Task.FromResult(true);
        }
    }
}
