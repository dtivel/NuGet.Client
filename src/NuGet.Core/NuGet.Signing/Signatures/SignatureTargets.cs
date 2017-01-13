// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

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
    }
}