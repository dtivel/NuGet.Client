// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
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

        // Cache tool dependencies between frameworks
        private readonly ConcurrentDictionary<LibraryRange, Task<GraphItem<RemoteResolveResult>>> _cache
            = new ConcurrentDictionary<LibraryRange, Task<GraphItem<RemoteResolveResult>>>();

        public RemoteToolWalker(RemoteWalkContext context)
        {
            _context = context;
        }

        public async Task<IDictionary<NuGetFramework, GraphNode<RemoteResolveResult>>> WalkAsync(LibraryRange toolLibrary, CancellationToken token)
        {
            // Resolve the tool package
            var frameworkNodes = await GetRootNodes(toolLibrary, token);

            // Resolve dependencies of the tool package
            var tasks = frameworkNodes.Values.Select(root => ApplyDependencyNodes(root, token));

            // Wait for all packages to be resolved
            await Task.WhenAll(tasks);

            return frameworkNodes;
        }

        private async Task<IDictionary<NuGetFramework, GraphNode<RemoteResolveResult>>> GetRootNodes(LibraryRange toolLibrary, CancellationToken token)
        {
            var roots = new Dictionary<NuGetFramework, GraphNode<RemoteResolveResult>>();

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

                // Create nodes for each discovered target framework
                foreach (var framework in allDependencies.Keys)
                {
                    var node = new GraphNode<RemoteResolveResult>(toolLibrary);

                    node.Item = new GraphItem<RemoteResolveResult>(match.Library)
                    {
                        Data = new RemoteResolveResult()
                        {
                            Match = match,
                            Dependencies = allDependencies[framework].Select(ToToolDependency).ToArray()
                        }
                    };

                    roots.Add(framework, node);
                }
            }
            else
            {
                // Unable to find tool
                var node = new GraphNode<RemoteResolveResult>(toolLibrary)
                {
                    Item = RemoteMatchUtility.CreateUnresolvedMatch(toolLibrary)
                };

                roots.Add(FrameworkConstants.CommonFrameworks.NetCoreApp10, node);
            }

            return roots;
        }

        private async Task ApplyDependencyNodes(GraphNode<RemoteResolveResult> root, CancellationToken token)
        {
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
        }

        private async Task<GraphNode<RemoteResolveResult>> GetDependencyNode(LibraryRange library, CancellationToken token)
        {
            return new GraphNode<RemoteResolveResult>(library)
            {
                Item = await FindToolDependencyCached(
                    libraryRange: library,
                    token: token)
            };
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

        private Task<GraphItem<RemoteResolveResult>> FindToolDependencyCached(
            LibraryRange libraryRange,
            CancellationToken token)
        {
            return _cache.GetOrAdd(libraryRange, (cacheKey) => FindToolDependency(libraryRange, token));
        }

        private async Task<GraphItem<RemoteResolveResult>> FindToolDependency(LibraryRange library, CancellationToken token)
        {
            GraphItem<RemoteResolveResult> item = null;

            var match = await RemoteMatchUtility.FindLibraryMatch(
                libraryRange: library,
                context: _context,
                cancellationToken: token);

            if (match != null)
            {
                // Dependencies are ignored here
                item = new GraphItem<RemoteResolveResult>(match.Library)
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
                item = RemoteMatchUtility.CreateUnresolvedMatch(library);
            }

            return item;
        }
    }
}