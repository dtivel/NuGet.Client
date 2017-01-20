// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using NuGet.Signing.Asn1;

namespace NuGet.Signing
{
    /// <summary>
    /// Represents signature targets.
    /// </summary>
    public sealed class SignatureTargets
    {
        internal const int CurrentVersion = 1;

        /// <summary>
        /// The version.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// The signature target.
        /// </summary>
        public SignatureTarget SignatureTarget { get; }

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="signatureTarget">A signature target.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="signatureTarget" /> is <c>null</c>.</exception>
        public SignatureTargets(SignatureTarget signatureTarget)
            : this(CurrentVersion, signatureTarget)
        {
        }

        internal SignatureTargets(int version, SignatureTarget signatureTarget)
        {
            Assert.IsNotNull(signatureTarget, nameof(signatureTarget));

            Version = version;
            SignatureTarget = signatureTarget;
        }

        internal DerSequence AsAsn1Value()
        {
            /*
                ASN.1 structure:

                SignatureTargets ::= SEQUENCE {
                  version            INTEGER { v1(1) },
                  signatureTargets   SignatureTarget }
            */

            var elements = new Value[]
            {
                new DerInteger(Version),
                SignatureTarget.AsAsn1Value()
            };

            return new DerSequence(elements);
        }

        internal static SignatureTargets Decode(byte[] bytes)
        {
            Assert.IsNotNullOrEmpty(bytes, nameof(bytes));

            using (var stream = new MemoryStream(bytes, index: 0, count: bytes.Length, writable: false))
            using (var reader = new BinaryReader(stream))
            {
                var signatureTargetsElement = Sequence.Read(reader);

                using (var innerStream = new MemoryStream(signatureTargetsElement.Content, index: 0, count: signatureTargetsElement.Content.Length, writable: false))
                using (var innerReader = new BinaryReader(innerStream))
                {
                    var versionElement = Integer.Read(innerReader);
                    var signatureTargetElement = Sequence.Read(innerReader);

                    if (versionElement.BigInteger < int.MinValue || versionElement.BigInteger > int.MaxValue)
                    {
                        throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidSignatureTargetsVersion);
                    }

                    var version = (int)versionElement.BigInteger;
                    var signatureTarget = SignatureTarget.Decode(signatureTargetElement.Content);

                    return new SignatureTargets(version, signatureTarget);
                }
            }
        }
    }
}