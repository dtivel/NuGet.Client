// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using NuGet.Signing.Asn1;

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

        internal DerSequence AsAsn1Value()
        {
            /*
                ASN.1 structure:

                ContentDigest ::= SEQUENCE {
                  digestAlgorithm    OBJECT IDENTIFIER
                  digest             OCTET STRING }
            */

            var elements = new Value[]
            {
                new DerObjectIdentifier(DigestAlgorithm),
                new DerOctetString(Digest)
            };

            return new DerSequence(elements);
        }

        internal static ContentDigest Decode(byte[] bytes)
        {
            Assert.IsNotNullOrEmpty(bytes, nameof(bytes));

            using (var stream = new MemoryStream(bytes, index: 0, count: bytes.Length, writable: false))
            using (var reader = new BinaryReader(stream))
            {
                var digestAlgorithmElement = ObjectIdentifier.Read(reader);

                if (digestAlgorithmElement.Encoding != Encoding.PrimitiveDefiniteLength)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidEncoding);
                }

                var digestElement = OctetString.Read(reader);

                if (digestElement.Encoding != Encoding.PrimitiveDefiniteLength)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidEncoding);
                }

                var digestAlgorithm = digestAlgorithmElement.Oid;
                var digest = digestElement.Content;

                return new ContentDigest(digestAlgorithm, digest);
            }
        }
    }
}