// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;

namespace NuGet.ProjectModel
{
    public class DotnetCliToolPathResolver
    {
        /// <summary>
        /// Gives the full path to the tool file.
        /// </summary>
        public static string GetFilePath(string projectOutputDirectory, string packageId)
        {
            return Path.GetFullPath(Path.Combine(projectOutputDirectory, GetFileName(packageId)));
        }

        /// <summary>
        /// Gives the tool file name. Ex: toola.dotnetclitool.json
        /// </summary>
        public static string GetFileName(string packageId)
        {
            return $"{packageId.ToLowerInvariant()}.dotnetclitool.json";
        }
    }
}