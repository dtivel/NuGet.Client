// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Org.BouncyCastle.Asn1;

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

        internal DerSequence ToAsn1Value()
        {
            return new DerSequence(
                new DerInteger(Version),
                SignatureTarget.ToAsn1Value());
        }

        internal static SignatureTargets Decode(byte[] data)
        {
            var sequence = Asn1Object.FromByteArray(data) as DerSequence;

            if (sequence == null || sequence.Count != 2)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargets);
            }

            var encodedVersion = sequence[0] as DerInteger;

            if (encodedVersion == null)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargets);
            }

            var encodedSignatureTargets = sequence[1] as DerSequence;

            if (encodedSignatureTargets == null)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargets);
            }

            var signatureTarget = SignatureTarget.Decode(encodedSignatureTargets);

            return new SignatureTargets(encodedVersion.Value.IntValue, signatureTarget);
        }
    }
}