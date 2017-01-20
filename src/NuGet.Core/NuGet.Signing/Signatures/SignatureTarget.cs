// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using NuGet.Packaging.Core;
using NuGet.Signing.Asn1;
using NuGet.Versioning;

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

        internal DerSequence AsAsn1Value()
        {
            /*
                ASN.1 structure:

                SignatureTarget ::= SEQUENCE {
                  version            INTEGER { v1(1) },
                  packageId          UTF8String,
                  packageVersion     UTF8String,
                  contentDigest      ContentDigest }
            */

            var elements = new Value[]
            {
                new DerInteger(Version),
                new DerUtf8String(PackageIdentity.Id),
                new DerUtf8String(PackageIdentity.Version.ToNormalizedString()),
                ContentDigest.AsAsn1Value()
            };

            return new DerSequence(elements);
        }

        internal static SignatureTarget Decode(byte[] bytes)
        {
            Assert.IsNotNullOrEmpty(bytes, nameof(bytes));

            using (var stream = new MemoryStream(bytes, index: 0, count: bytes.Length, writable: false))
            using (var reader = new BinaryReader(stream))
            {
                var versionElement = Integer.Read(reader);

                if (versionElement.Encoding != Encoding.PrimitiveDefiniteLength)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidEncoding);
                }

                var packageIdElement = Utf8String.Read(reader);

                if (packageIdElement.Encoding != Encoding.PrimitiveDefiniteLength)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidEncoding);
                }

                var packageVersionElement = Utf8String.Read(reader);

                if (packageVersionElement.Encoding != Encoding.PrimitiveDefiniteLength)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidEncoding);
                }

                var contentDigestElement = Sequence.Read(reader);

                if (contentDigestElement.Encoding != Encoding.ConstructedDefiniteLength)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidEncoding);
                }

                if (versionElement.BigInteger < int.MinValue || versionElement.BigInteger > int.MaxValue)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidSignatureTargetVersion);
                }

                var version = (int)versionElement.BigInteger;
                var packageId = packageIdElement.String;
                var packageVersion = packageVersionElement.String;
                var packageIdentity = new PackageIdentity(packageId, new NuGetVersion(packageVersion));
                var contentDigest = ContentDigest.Decode(contentDigestElement.Content);

                return new SignatureTarget(version, packageIdentity, contentDigest);
            }
        }
    }
}