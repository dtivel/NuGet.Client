// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Org.BouncyCastle.Asn1;

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

        internal DerSequence ToAsn1Value()
        {
            return new DerSequence(
                new DerInteger(Version),
                new DerUtf8String(PackageIdentity.Id),
                new DerUtf8String(PackageIdentity.Version.ToNormalizedString()),
                ContentDigest.ToAsn1Value());
        }

        internal static SignatureTarget Decode(DerSequence sequence)
        {
            if (sequence == null || sequence.Count != 4)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTarget);
            }

            var encodedVersion = sequence[0] as DerInteger;

            if (encodedVersion == null)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTarget);
            }

            var encodedPackageId = sequence[1] as DerUtf8String;

            if (encodedPackageId == null)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTarget);
            }

            var encodedPackageVersion = sequence[2] as DerUtf8String;

            if (encodedPackageVersion == null)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTarget);
            }

            var encodedContentDigest = sequence[3] as DerSequence;

            if (encodedContentDigest == null)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTarget);
            }

            var contentDigest = ContentDigest.Decode(encodedContentDigest);
            var version = new NuGetVersion(encodedPackageVersion.GetString());
            var packageIdentity = new PackageIdentity(encodedPackageId.GetString(), version);

            return new SignatureTarget(encodedVersion.Value.IntValue, packageIdentity, contentDigest);
        }
    }
}