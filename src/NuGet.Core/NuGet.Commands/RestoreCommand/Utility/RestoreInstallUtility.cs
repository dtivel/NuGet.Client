using System;
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

        internal static async Task<RemoteMatch> FindLibraryMatch(
           LibraryRange libraryRange,
           NuGetFramework framework,
           GraphEdge<RemoteResolveResult> outerEdge,
           CancellationToken cancellationToken)
        {
            if (libraryRange.VersionRange == null)
            {
                return null;
            }

            // The resolution below is only for package types
            if (!libraryRange.TypeConstraintAllows(LibraryDependencyTarget.Package))
            {
                return null;
            }

            if (libraryRange.VersionRange.IsFloating)
            {
                // For snapshot dependencies, get the version remotely first.
                var remoteMatch = await FindLibraryByVersion(libraryRange, framework, _context.RemoteLibraryProviders, cancellationToken);
                if (remoteMatch != null)
                {
                    // Try to see if the specific version found on the remote exists locally. This avoids any unnecessary
                    // remote access incase we already have it in the cache/local packages folder. 
                    var localMatch = await FindLibraryByVersion(remoteMatch.Library, framework, _context.LocalLibraryProviders, cancellationToken);

                    if (localMatch != null
                        && localMatch.Library.Version.Equals(remoteMatch.Library.Version))
                    {
                        // If we have a local match, and it matches the version *exactly* then use it.
                        return localMatch;
                    }

                    // We found something locally, but it wasn't an exact match
                    // for the resolved remote match.
                }

                return remoteMatch;
            }
            else
            {
                // Check for the specific version locally.
                var localMatch = await FindLibraryByVersion(libraryRange, framework, _context.LocalLibraryProviders, cancellationToken);

                if (localMatch != null
                    && localMatch.Library.Version.Equals(libraryRange.VersionRange.MinVersion))
                {
                    // We have an exact match so use it.
                    return localMatch;
                }

                // Either we found a local match but it wasn't the exact version, or 
                // we didn't find a local match.
                var remoteMatch = await FindLibraryByVersion(libraryRange, framework, _context.RemoteLibraryProviders, cancellationToken);

                if (remoteMatch != null
                    && localMatch == null)
                {
                    // There wasn't any local match for the specified version but there was a remote match.
                    // See if that version exists locally.
                    localMatch = await FindLibraryByVersion(remoteMatch.Library, framework, _context.LocalLibraryProviders, cancellationToken);
                }

                if (localMatch != null
                    && remoteMatch != null)
                {
                    // We found a match locally and remotely, so pick the better version
                    // in relation to the specified version.
                    if (libraryRange.VersionRange.IsBetter(
                        current: localMatch.Library.Version,
                        considering: remoteMatch.Library.Version))
                    {
                        return remoteMatch;
                    }
                    else
                    {
                        return localMatch;
                    }
                }

                // Prefer local over remote generally.
                return localMatch ?? remoteMatch;
            }
        }

        private static async Task<RemoteMatch> FindLibraryByVersion(LibraryRange libraryRange, NuGetFramework framework, IEnumerable<IRemoteDependencyProvider> providers, CancellationToken token)
        {
            if (libraryRange.VersionRange.IsFloating)
            {
                // Don't optimize the non http path for floating versions or we'll miss things
                return await FindLibrary(
                    libraryRange,
                    providers,
                    provider => provider.FindLibraryAsync(
                        libraryRange,
                        framework,
                        _context.CacheContext,
                        _context.Logger,
                        token));
            }

            // Try the non http sources first
            var nonHttpMatch = await FindLibrary(
                libraryRange,
                providers.Where(p => !p.IsHttp),
                provider => provider.FindLibraryAsync(
                    libraryRange,
                    framework,
                    _context.CacheContext,
                    _context.Logger,
                    token));

            // If we found an exact match then use it
            if (nonHttpMatch != null && nonHttpMatch.Library.Version.Equals(libraryRange.VersionRange.MinVersion))
            {
                return nonHttpMatch;
            }

            // Otherwise try the http sources
            var httpMatch = await FindLibrary(
                libraryRange,
                providers.Where(p => p.IsHttp),
                provider => provider.FindLibraryAsync(
                    libraryRange,
                    framework,
                    _context.CacheContext,
                    _context.Logger,
                    token));

            // Pick the best match of the 2
            if (libraryRange.VersionRange.IsBetter(
                nonHttpMatch?.Library?.Version,
                httpMatch?.Library.Version))
            {
                return httpMatch;
            }

            return nonHttpMatch;
        }

        private static async Task<RemoteMatch> FindLibrary(
            LibraryRange libraryRange,
            IEnumerable<IRemoteDependencyProvider> providers,
            Func<IRemoteDependencyProvider, Task<LibraryIdentity>> action)
        {
            var tasks = new List<Task<RemoteMatch>>();
            foreach (var provider in providers)
            {
                Func<Task<RemoteMatch>> taskWrapper = async () =>
                {
                    var library = await action(provider);
                    if (library != null)
                    {
                        return new RemoteMatch
                        {
                            Provider = provider,
                            Library = library
                        };
                    }

                    return null;
                };

                tasks.Add(taskWrapper());
            }

            RemoteMatch bestMatch = null;

            while (tasks.Count > 0)
            {
                var task = await Task.WhenAny(tasks);
                tasks.Remove(task);
                var match = await task;

                // If we found an exact match then use it.
                // This allows us to shortcircuit slow feeds even if there's an exact match
                if (!libraryRange.VersionRange.IsFloating &&
                    match?.Library?.Version != null &&
                    libraryRange.VersionRange.IsMinInclusive &&
                    match.Library.Version.Equals(libraryRange.VersionRange.MinVersion))
                {
                    return match;
                }

                // Otherwise just find the best out of the matches
                if (libraryRange.VersionRange.IsBetter(
                    current: bestMatch?.Library?.Version,
                    considering: match?.Library?.Version))
                {
                    bestMatch = match;
                }
            }

            return bestMatch;
        }
    }
}
