// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography.Pkcs;
using NuGet.Signing.Primitives.Rfc2634;

namespace NuGet.Signing.Primitives.Rfc5035
{
    /*
        // RFC 5035 section 4 (https://tools.ietf.org/html/rfc5035#section-4).
        ESSCertIDv2 ::=  SEQUENCE {
            hashAlgorithm           AlgorithmIdentifier
                                    DEFAULT {algorithm id-sha256},
            certHash                Hash,
            issuerSerial            IssuerSerial OPTIONAL
        }

        Hash ::= OCTET STRING


        // RFC 3280 appendix A.1 (https://tools.ietf.org/html/rfc3280#appendix-A.1).
        AlgorithmIdentifier  ::=  SEQUENCE  {
             algorithm               OBJECT IDENTIFIER,
             parameters              ANY DEFINED BY algorithm OPTIONAL  }
                                        -- contains a value of the type
                                        -- registered for use with the
                                        -- algorithm object identifier value
    */
    public sealed class EssCertIdV2
    {
        public AlgorithmIdentifier HashAlgorithm { get; }
        public byte[] CertHash { get; }
        public IssuerSerial IssuerSerial { get; }

        public EssCertIdV2(AlgorithmIdentifier hashAlgorithm, byte[] certHash, IssuerSerial issuerSerial)
        {
            Assert.IsNotNull(hashAlgorithm, nameof(hashAlgorithm));
            Assert.IsNotNullOrEmpty(certHash, nameof(certHash));

            HashAlgorithm = hashAlgorithm;
            CertHash = certHash;
            IssuerSerial = issuerSerial;
        }
    }
}