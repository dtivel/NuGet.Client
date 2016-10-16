﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.DependencyResolver;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuGet.Commands
{
    internal static class RestoreInstallUtility
    {
        internal static async Task InstallPackagesAsync(
            RestoreRequest request,
            IEnumerable<RemoteMatch> packagesToInstall,
            HashSet<LibraryIdentity> allInstalledPackages,
            CancellationToken token)
        {
            if (request.MaxDegreeOfConcurrency <= 1)
            {
                foreach (var match in packagesToInstall)
                {
                    await InstallPackageAsync(request, match, token);
                }
            }
            else
            {
                var bag = new ConcurrentBag<RemoteMatch>(packagesToInstall);
                var tasks = Enumerable.Range(0, request.MaxDegreeOfConcurrency)
                    .Select(async _ =>
                    {
                        RemoteMatch match;
                        while (bag.TryTake(out match))
                        {
                            await InstallPackageAsync(request, match, token);
                        }
                    });
                await Task.WhenAll(tasks);
            }
        }

        internal static async Task InstallPackageAsync(
            RestoreRequest request,
            RemoteMatch installItem,
            CancellationToken token)
        {
            var packageIdentity = new PackageIdentity(installItem.Library.Name, installItem.Library.Version);

            var versionFolderPathContext = new VersionFolderPathContext(
                packageIdentity,
                request.PackagesDirectory,
                request.Log,
                request.PackageSaveMode,
                request.XmlDocFileSaveMode);

            await PackageExtractor.InstallFromSourceAsync(
                stream => installItem.Provider.CopyToAsync(
                    installItem.Library,
                    stream,
                    request.CacheContext,
                    request.Log,
                    token),
                versionFolderPathContext,
                token);
        }
    }
}
