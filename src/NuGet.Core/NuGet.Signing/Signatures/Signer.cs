// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Signing
{
    /// <summary>
    /// Represents an entity that can sign.
    /// </summary>
    public sealed class Signer
    {
        private readonly bool _allowUntrustedRoot;
        private readonly X509Certificate2 _certificate;
        private readonly X509Certificate2Collection _additionalCertificates;

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="certificate">A valid X.509 signing certificate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="certificate" /> is <c>null</c>.</exception>
        public Signer(X509Certificate2 certificate) : this(certificate, new X509Certificate2Collection(), allowUntrustedRoot: false)
        {
        }

        // This is public only to facilitate testing.
        public Signer(X509Certificate2 certificate, X509Certificate2Collection additionalCertificates, bool allowUntrustedRoot)
        {
            Assert.IsNotNull(certificate, nameof(certificate));
            Assert.IsNotNull(additionalCertificates, nameof(additionalCertificates));

            _certificate = certificate;
            _additionalCertificates = additionalCertificates;
            _allowUntrustedRoot = allowUntrustedRoot;
        }

        /// <summary>
        /// Signs a file.
        /// </summary>
        /// <param name="filePath">The path to the file to sign.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous sign operation.
        /// The task result (<see cref="Task{TResult}.Result" />) returns a <see cref="Signature"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="filePath" /> is an empty string.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file indicated by <paramref name="filePath" /> does not exist.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
        public async Task<Signature> SignAsync(string filePath, CancellationToken cancellationToken)
        {
            Assert.IsNotNullOrEmpty(filePath, nameof(filePath));

            cancellationToken.ThrowIfCancellationRequested();

            var targets = await CreateSignatureTargetsAsync(filePath, cancellationToken);

            return await SignAsync(targets, cancellationToken);
        }

        private async Task<Signature> SignAsync(SignatureTargets targets, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(targets, nameof(targets));

            X509Chain chain;
            var isValid = CertificateUtilities.IsCertificateValid(_certificate, out chain, _allowUntrustedRoot);

            using (chain)
            {
                if (!isValid)
                {
                    throw new InvalidSigningCertificateException(Strings.InvalidSigningCertificate, chain.ChainStatus, chain.ChainElements);
                }
            }

            var content = Asn1Utilities.Encode(targets);
            var contentInfo = new ContentInfo(content);

            var signer = new CmsSigner(SubjectIdentifierType.SubjectKeyIdentifier, _certificate);
            var signingTime = new Pkcs9SigningTime();

            signer.SignedAttributes.Add(
                new CryptographicAttributeObject(
                    signingTime.Oid,
                    new AsnEncodedDataCollection(signingTime)));

            signer.IncludeOption = X509IncludeOption.WholeChain;

            foreach (var additionalCertificate in _additionalCertificates)
            {
                if (!additionalCertificate.Equals(_certificate))
                {
                    signer.Certificates.Add(additionalCertificate);
                }
            }

            var cms = new SignedCms(contentInfo);

            cms.ComputeSignature(signer);

            return await Task.FromResult(Signature.FromSignedCms(cms, targets));
        }

        private static async Task<SignatureTargets> CreateSignatureTargetsAsync(string packageFilePath, CancellationToken cancellationToken)
        {
            var packageIdentity = PackageUtilities.ExtractPackageIdentity(packageFilePath);
            var digest = await SigningUtilities.ComputeDigestAsync(packageFilePath, cancellationToken);
            var packageFileName = Path.GetFileName(packageFilePath);
            var contentDigest = new ContentDigest(new Oid(Constants.Sha512Oid), digest);
            var target = new SignatureTarget(packageIdentity, contentDigest);

            return new SignatureTargets(target);
        }
    }
}