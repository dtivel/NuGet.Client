// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using NuGet.Signing.Primitives.Rfc2459;

namespace NuGet.Signing.Primitives.Rfc2634
{
    /*
        // RFC 2634 section 5.4 (https://tools.ietf.org/html/rfc2634#section-5.4).
        SigningCertificate ::=  SEQUENCE {
            certs        SEQUENCE OF ESSCertID,
            policies     SEQUENCE OF PolicyInformation OPTIONAL
        }
    */
    public sealed class SigningCertificate
    {
        public const string Oid = "1.2.840.113549.1.9.16.2.12";

        public IEnumerable<EssCertId> Certs { get; }
        public IEnumerable<PolicyInformation> Policies { get; }

        public SigningCertificate(IEnumerable<EssCertId> certs, IEnumerable<PolicyInformation> policies)
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