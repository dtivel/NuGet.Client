// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Signing
{
    /// <summary>
    /// Represents an RFC 3161-compliant timestamp service.
    /// </summary>
    public sealed class Timestamper
    {
        private readonly Uri _timestampService;

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="timestampService">The timestamping service endpoint URL.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="timestampService" /> is <c>null</c>.</exception>
        public Timestamper(Uri timestampService)
        {
            Assert.IsNotNull(timestampService, nameof(timestampService));

            if (!string.Equals(timestampService.Scheme, Uri.UriSchemeHttp)
                && !string.Equals(timestampService.Scheme, Uri.UriSchemeHttps))
            {
                throw new ArgumentException(Strings.UnsupportedUriScheme, nameof(timestampService));
            }

            _timestampService = timestampService;
        }

        /// <summary>
        /// Timestamps a signature with an RFC 3161-compliant timestamp.
        /// </summary>
        /// <param name="signature">The signature to timestamp.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous timestamp operation.
        /// The task result (<see cref="Task{TResult}.Result" />) returns a timestamped <see cref="Signature" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="signature" /> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
        public async Task<Signature> TimestampAsync(Signature signature, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(signature, nameof(signature));

            cancellationToken.ThrowIfCancellationRequested();

            using (var signatureCms = NativeCms.Decode(signature.Encode(), detached: false))
            {
                var digest = signatureCms.GetEncryptedDigest();
                var timestamp = await TimestampUtilities.RequestTimestampAsync(digest, Constants.Sha256Oid, _timestampService, cancellationToken);
                byte[] rawTimestamp;

                using (var chain = new X509Chain())
                {
                    var certificate = timestamp.SignerInfos[0].Certificate;

                    if (!chain.Build(certificate))
                    {
                        throw new CertificateChainBuildException(Strings.UnableToBuildTimestampCertificateChain, certificate);
                    }

                    using (var timestampCms = NativeCms.Decode(timestamp.Encode(), detached: false))
                    {
                        timestampCms.AddCertificates(chain.ChainElements
                            .Cast<X509ChainElement>()
                            .Where(c => !timestamp.Certificates.Contains(c.Certificate))
                            .Select(c => c.Certificate.Export(X509ContentType.Cert)));

                        rawTimestamp = timestampCms.Encode();
                    }
                }

                signatureCms.AddTimestamp(rawTimestamp);

                var newSignatureCms = signatureCms.Encode();
                var newSignature = new SignedCms();

                newSignature.Decode(newSignatureCms);

                return Signature.FromSignedCms(newSignature, signature.SignatureTargets);
            }
        }
    }
}