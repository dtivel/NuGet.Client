using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.DependencyResolver;
using NuGet.LibraryModel;

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
            throw new NotImplementedException();
        }
    }
}
