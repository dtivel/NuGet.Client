// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace NuGet.Signing.Primitives.Rfc2459
{
    /*
        // RFC 2459 appendix A.2 (https://tools.ietf.org/html/rfc2459#appendix-A.2).
        PolicyInformation ::= SEQUENCE {
             policyIdentifier   CertPolicyId,
             policyQualifiers   SEQUENCE SIZE (1..MAX) OF
                     PolicyQualifierInfo OPTIONAL }

        CertPolicyId ::= OBJECT IDENTIFIER
    */
    public sealed class PolicyInformation
    {
        public string PolicyIdentifier { get; }
        public IEnumerable<PolicyQualifierInfo> PolicyQualifiers { get; }

        public PolicyInformation(string policyIdentifier, IEnumerable<PolicyQualifierInfo> policyQualifiers)
        {
            Assert.IsNotNullOrEmpty(policyIdentifier, nameof(policyIdentifier));

            PolicyIdentifier = policyIdentifier;

            if (policyQualifiers != null)
            {
                Assert.IsNotNullOrEmpty(policyQualifiers, nameof(policyQualifiers));

                PolicyQualifiers = policyQualifiers.ToList().AsReadOnly();
            }
        }
    }
}