// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.Pkcs;

namespace NuGet.Signing
{
    /// <summary>
    /// Represents a package signature.
    /// </summary>
    public sealed class Signature
    {
        private readonly SignedCms _signedCms;

        /// <summary>
        /// The signature targets.
        /// </summary>
        public SignatureTargets SignatureTargets { get; }

        /// <summary>
        /// The signer.
        /// </summary>
        public Signatory Signatory { get; }

        /// <summary>
        /// The signer's identity.
        /// </summary>
        public SignerIdentity SignerIdentity { get; }

        private Signature(SignatureTargets signatureTargets, SignedCms signedCms, Signatory signatory)
        {
            SignatureTargets = signatureTargets;
            _signedCms = signedCms;
            Signatory = signatory;
            SignerIdentity = SignerIdentity.Create(signatory.Certificate);
        }

        /// <summary>
        /// Creates a signature instance from the provided SignedData CMS object and signature targets.
        /// </summary>
        /// <param name="signedCms">A SignedData CMS object.</param>
        /// <param name="signatureTargets">The signature targets.</param>
        /// <returns>A <see cref="Signature" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="signedCms" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="signedCms" /> is has no signers.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="signatureTargets" /> is <c>null</c>.</exception>
        public static Signature FromSignedCms(SignedCms signedCms, SignatureTargets signatureTargets)
        {
            Assert.IsNotNull(signedCms, nameof(signedCms));
            Assert.IsNotNull(signatureTargets, nameof(signatureTargets));

            if (signedCms.SignerInfos.Count != 1)
            {
                throw new ArgumentException(Strings.InvalidSignedDataMessage, nameof(signedCms));
            }

            if (signatureTargets.Version != SignatureTargets.CurrentVersion)
            {
                throw new ArgumentException(Strings.InvalidSignatureTargetsInvalidSignatureTargetsVersion, nameof(signatureTargets));
            }

            if (signatureTargets.SignatureTarget.Version != SignatureTarget.CurrentVersion)
            {
                throw new ArgumentException(Strings.InvalidSignatureTargetsInvalidSignatureTargetVersion, nameof(signatureTargets));
            }

            if (signatureTargets.SignatureTarget.ContentDigest.DigestAlgorithm.Value != Constants.Sha512Oid)
            {
                throw new ArgumentException(Strings.InvalidSignatureTargetsInvalidContentDigestAlgorithm, nameof(signatureTargets));
            }

            var signerInfo = signedCms.SignerInfos[0];
            var signatory = Signatory.FromSignerInfo(signerInfo);

            return new Signature(signatureTargets, signedCms, signatory);
        }

        internal byte[] Encode()
        {
            return _signedCms.Encode();
        }
    }
}