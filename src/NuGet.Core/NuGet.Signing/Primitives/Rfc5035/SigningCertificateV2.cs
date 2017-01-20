// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using NuGet.Signing.Primitives.Rfc2459;

namespace NuGet.Signing.Primitives.Rfc5035
{
    /*
        // RFC 5035 section 3 (https://tools.ietf.org/html/rfc5035#section-3).
        SigningCertificateV2 ::=  SEQUENCE {
            certs        SEQUENCE OF ESSCertIDv2,
            policies     SEQUENCE OF PolicyInformation OPTIONAL
        }
    */
    public sealed class SigningCertificateV2
    {
        public const string Oid = "1.2.840.113549.1.9.16.2.47";

        public IEnumerable<EssCertIdV2> Certs { get; }
        public IEnumerable<PolicyInformation> Policies { get; }

        public SigningCertificateV2(IEnumerable<EssCertIdV2> certs)
            : this(certs, policies: null)
        {
        }

        public SigningCertificateV2(IEnumerable<EssCertIdV2> certs, IEnumerable<PolicyInformation> policies)
        {
            Assert.IsNotNull(certs, nameof(certs));

            Certs = certs.ToList().AsReadOnly();

            if (policies != null)
            {
                Policies = policies.ToList().AsReadOnly();
            }
        }
    }
}