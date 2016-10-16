// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;

namespace NuGet.DependencyResolver
{
    public class RemoteToolWalker
    {
        private readonly RemoteWalkContext _context;

        public RemoteToolWalker(RemoteWalkContext context)
        {
            _context = context;
        }

        public async Task<GraphNode<RemoteResolveResult>> WalkAsync(LibraryRange toolLibrary, CancellationToken token)
        {
            // Resolve the tool package
            var root = await GetRootNode(toolLibrary, token);

            // Resolve dependencies
            var tasks = root.Item.Data.Dependencies
                .Select(dependency => GetDependencyNode(dependency.LibraryRange, token))
                .ToList();

            while (tasks.Count > 0)
            {
                // Wait for any node to finish resolving
                var task = await Task.WhenAny(tasks);

                // Extract the resolved node
                tasks.Remove(task);
                var dependencyNode = await task;
                dependencyNode.OuterNode = root;

                root.InnerNodes.Add(dependencyNode);
            }

            return root;
        }

        private async Task<GraphNode<RemoteResolveResult>> GetRootNode(LibraryRange toolLibrary, CancellationToken token)
        {
            var node = new GraphNode<RemoteResolveResult>(toolLibrary);

            var match = await RemoteMatchUtility.FindLibraryMatch(
                libraryRange: toolLibrary,
                context: _context,
                cancellationToken: token);

            if (match != null)
            {
                // Read all dependencies from the deps files
                var allDependencies = await match.Provider.GetToolDependenciesAsync(
                    match: match.Library,
                    cacheContext: _context.CacheContext,
                    logger: _context.Logger,
                    cancellationToken: token);

                // De-duplicate the dependency list and filter to netcoreapp
                // Convert identities to ranges allowing only that exact version.
                var dependencies = allDependencies.Where(pair =>
                    StringComparer.OrdinalIgnoreCase.Equals(
                        pair.Key.Framework,
                        FrameworkConstants.FrameworkIdentifiers.NetCoreApp))
                    .SelectMany(pair => pair.Value)
                    .Distinct()
                    .Select(ToToolDependency)
                    .ToList();

                node.Item = new GraphItem<RemoteResolveResult>(match.Library)
                {
                    Data = new RemoteResolveResult()
                    {
                        Match = match,
                        Dependencies = dependencies
                    }
                };
            }
            else
            {
                // Unable to find tool
                node.Item = RemoteMatchUtility.CreateUnresolvedMatch(toolLibrary);
            }

            return node;
        }

        private async Task<GraphNode<RemoteResolveResult>> GetDependencyNode(LibraryRange library, CancellationToken token)
        {
            var node = new GraphNode<RemoteResolveResult>(library);

            var match = await RemoteMatchUtility.FindLibraryMatch(
                libraryRange: library,
                context: _context,
                cancellationToken: token);

            if (match != null)
            {
                // Dependencies are ignored here
                node.Item = new GraphItem<RemoteResolveResult>(match.Library)
                {
                    Data = new RemoteResolveResult()
                    {
                        Match = match,
                        Dependencies = Enumerable.Empty<LibraryDependency>()
                    }
                };
            }
            else
            {
                // Unable to find package
                node.Item = RemoteMatchUtility.CreateUnresolvedMatch(library);
            }

            return node;
        }

        /// <summary>
        /// Identity -> Dependency with a range allowing a single version.
        /// </summary>
        private static LibraryDependency ToToolDependency(LibraryIdentity library)
        {
            return new LibraryDependency()
            {
                LibraryRange = new LibraryRange(
                            name: library.Name,
                            versionRange: new VersionRange(
                                minVersion: library.Version,
                                includeMinVersion: true,
                                maxVersion: library.Version,
                                includeMaxVersion: true),
                            typeConstraint: LibraryDependencyTarget.Package)
            };
        }
    }
}
