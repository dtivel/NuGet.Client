// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Primitives.Rfc2634
{
    /*
        // RFC 2634 section 5.4.1 (https://tools.ietf.org/html/rfc2634#section-5.4.1).
        ESSCertID ::=  SEQUENCE {
            certHash                 Hash,
            issuerSerial             IssuerSerial OPTIONAL
        }

        Hash ::= OCTET STRING -- SHA1 hash of entire certificate
    */
    public sealed class EssCertId
    {
        public byte[] CertHash { get; }
        public IssuerSerial IssuerSerial { get; }

        public EssCertId(byte[] certHash, IssuerSerial issuerSerial)
        {
            Assert.IsNotNullOrEmpty(certHash, nameof(certHash));

            CertHash = certHash;
            IssuerSerial = issuerSerial;
        }
    }
}