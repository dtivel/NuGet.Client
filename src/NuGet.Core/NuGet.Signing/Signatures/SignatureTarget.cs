// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Packaging.Core;

namespace NuGet.Signing
{
    /// <summary>
    /// Represents a signature target.
    /// </summary>
    public sealed class SignatureTarget
    {
        internal const int CurrentVersion = 1;

        /// <summary>
        /// The version.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// The package identity.
        /// </summary>
        public PackageIdentity PackageIdentity { get; }

        /// <summary>
        /// The content digest.
        /// </summary>
        public ContentDigest ContentDigest { get; }

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="packageIdentity">The package identity.</param>
        /// <param name="contentDigest">The content digest.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageIdentity" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="packageIdentity" /> does not have a version.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="contentDigest" /> is <c>null</c>.</exception>
        public SignatureTarget(PackageIdentity packageIdentity, ContentDigest contentDigest)
            : this(CurrentVersion, packageIdentity, contentDigest)
        {
        }

        internal SignatureTarget(int version, PackageIdentity packageIdentity, ContentDigest contentDigest)
        {
            Assert.IsNotNull(packageIdentity, nameof(packageIdentity));
            Assert.IsNotNull(contentDigest, nameof(contentDigest));

            if (!packageIdentity.HasVersion)
            {
                throw new ArgumentException(Strings.InvalidPackageIdentity, nameof(packageIdentity));
            }

            Version = version;
            PackageIdentity = packageIdentity;
            ContentDigest = contentDigest;
        }
    }
}