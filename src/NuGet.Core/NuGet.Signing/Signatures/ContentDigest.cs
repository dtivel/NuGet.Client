// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;

namespace NuGet.Signing
{
    /// <summary>
    /// The content digest for a package signature.
    /// </summary>
    public sealed class ContentDigest
    {
        /// <summary>
        /// The object identifier for the message digest algorithm.
        /// </summary>
        public Oid DigestAlgorithm { get; }

        /// <summary>
        /// The message digest.
        /// </summary>
        public byte[] Digest { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="digestAlgorithm">The object identifier for the message digest algorithm.</param>
        /// <param name="digest">The message digest.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="digestAlgorithm" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="digest" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="digest" /> is an empty array.</exception>
        public ContentDigest(Oid digestAlgorithm, byte[] digest)
        {
            Assert.IsNotNull(digestAlgorithm, nameof(digestAlgorithm));
            Assert.IsNotNullOrEmpty(digest, nameof(digest));

            DigestAlgorithm = digestAlgorithm;
            Digest = digest;
        }

        internal DerSequence ToAsn1Value()
        {
            return new DerSequence(
                new DerObjectIdentifier(DigestAlgorithm.Value),
                new DerOctetString(Digest));
        }

        internal static ContentDigest Decode(DerSequence sequence)
        {
            if (sequence == null || sequence.Count != 2)
            {
                throw new InvalidDataException(Strings.InvalidContentDigest);
            }

            var encodedDigestAlgorithm = sequence[0] as DerObjectIdentifier;

            if (encodedDigestAlgorithm == null)
            {
                throw new InvalidDataException(Strings.InvalidContentDigest);
            }

            var encodedDigest = sequence[1] as DerOctetString;

            if (encodedDigest == null)
            {
                throw new InvalidDataException(Strings.InvalidContentDigest);
            }

            return new ContentDigest(new Oid(encodedDigestAlgorithm.Id), encodedDigest.GetOctets());
        }
    }
}