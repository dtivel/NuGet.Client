// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography.X509Certificates;
using NuGet.Common;

namespace NuGet.Signing
{
    internal static class CertificateUtilities
    {
        internal static byte[] ComputeFingerprint(X509Certificate2 certificate)
        {
            Assert.IsNotNull(certificate, nameof(certificate));

            var bytes = certificate.GetRawCertData();

            using (var hashFunc = new Sha512HashFunction())
            {
                hashFunc.Update(bytes, offset: 0, count: bytes.Length);

                return hashFunc.GetHash();
            }
        }

        internal static bool IsCertificateValid(X509Certificate2 certificate, out X509Chain chain, bool allowUntrustedRoot = false)
        {
            Assert.IsNotNull(certificate, nameof(certificate));

            chain = new X509Chain();

            if (allowUntrustedRoot)
            {
                chain.ChainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
            }

            return chain.Build(certificate);
        }
    }
}