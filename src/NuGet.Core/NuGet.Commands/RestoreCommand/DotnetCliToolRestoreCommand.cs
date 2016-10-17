using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.DependencyResolver;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Packaging;
using NuGet.Repositories;

namespace NuGet.Commands
{
    internal class DotnetCliToolRestoreCommand
    {
        private readonly ILogger _logger;
        private readonly RestoreRequest _request;

        public DotnetCliToolRestoreCommand(RestoreRequest request)
        {
            _logger = request.Log;
            _request = request;
        }

        public async Task<Tuple<bool, List<RestoreTargetGraph>>> TryRestore(
            LibraryRange toolRange,
            HashSet<LibraryIdentity> allInstalledPackages,
            NuGetv3LocalRepository userPackageFolder,
            IReadOnlyList<NuGetv3LocalRepository> fallbackPackageFolders,
            RemoteToolWalker remoteWalker,
            RemoteWalkContext context,
            CancellationToken token)
        {
            var toolNode = await remoteWalker.GetNodeAsync(toolRange, token);

            var toolOnlyGraph = RestoreTargetGraph.Create(
                new[] { toolNode },
                context,
                _request.Log,
                NuGetFramework.AgnosticFramework);

            await InstallPackagesAsync(new[] { toolOnlyGraph }, allInstalledPackages, token);

            var localRepositories = new List<NuGetv3LocalRepository>();
            localRepositories.Add(userPackageFolder);
            localRepositories.AddRange(fallbackPackageFolders);

            foreach (var local in localRepositories)
            {
  
            }

            return null;
        }

        private async Task InstallPackagesAsync(IEnumerable<RestoreTargetGraph> graphs,
            HashSet<LibraryIdentity> allInstalledPackages,
            CancellationToken token)
        {
            var packagesToInstall = graphs.SelectMany(g => g.Install.Where(match => allInstalledPackages.Add(match.Library)));

            await RestoreInstallUtility.InstallPackagesAsync(
                _request,
                packagesToInstall,
                allInstalledPackages,
                token);
        }
    }
}
