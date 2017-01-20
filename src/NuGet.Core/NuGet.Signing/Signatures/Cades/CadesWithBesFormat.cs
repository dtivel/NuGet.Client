// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using NuGet.Signing.Primitives.Rfc2634;
using NuGet.Signing.Primitives.Rfc5035;
using NuGet.Signing.Primitives.Rfc5280;

namespace NuGet.Signing.Signatures.Cades
{
    /// <summary>
    /// Digital signature that conforms to the CAdES Basic Electronic Signature (CAdES-BES) format.
    /// </summary>
    /// <remarks>
    /// See ETSI TS 101 733 V2.2.1 (2013-04), section 4.3.1 for details.
    /// </remarks>
    internal class CadesWithBesFormat
    {
        protected X509Certificate2 Certificate { get; }
        protected CmsSigner Signer { get; }
        protected byte[] MessageContent { get; }

        internal CadesWithBesFormat(X509Certificate2 certificate, byte[] messageContent)
        {
            Assert.IsNotNull(certificate, nameof(certificate));
            Assert.IsNotNull(messageContent, nameof(messageContent));

            Certificate = certificate;
            MessageContent = messageContent;
            Signer = new CmsSigner(SubjectIdentifierType.SubjectKeyIdentifier, certificate);
        }

        internal virtual IEnumerable<CryptographicAttributeObject> GetSignedAttributes()
        {
            var attributes = new List<CryptographicAttributeObject>();

            var oid = new Oid(Constants.Sha512Oid);
            var hashAlgorithm = new AlgorithmIdentifier(oid);
            var hash = CertificateUtilities.ComputeFingerprint(Certificate);
            var issuerSerial = new IssuerSerial(new GeneralNames(new[] { new GeneralName() }), Certificate.GetSerialNumber());
            var essCertIdV2 = new EssCertIdV2(hashAlgorithm, hash, issuerSerial: null);
            var signingCertificate = new SigningCertificateV2(new[] { essCertIdV2 });

            //var attribute = new CryptographicAttributeObject(new Oid(SigningCertificateV2.Oid), signingCertificate);

            return attributes;
        }

        internal virtual IEnumerable<CryptographicAttributeObject> GetUnsignedAttributes()
        {
            return Enumerable.Empty<CryptographicAttributeObject>();
        }
    }
}