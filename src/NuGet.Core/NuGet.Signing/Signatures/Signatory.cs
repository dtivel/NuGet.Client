// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace NuGet.Signing
{
    /// <summary>
    /// Represents an existing signer for a signature.
    /// </summary>
    public sealed class Signatory
    {
        /// <summary>
        /// The X.509 certificate used to generate the signature.
        /// </summary>
        public X509Certificate2 Certificate { get; }

        internal DateTimeOffset? SigningTime { get; }

        private Signatory(X509Certificate2 certificate, DateTimeOffset? signingTime)
        {
            Certificate = certificate;
            SigningTime = signingTime;
        }

        internal static Signatory FromSignerInfo(SignerInfo signerInfo)
        {
            Assert.IsNotNull(signerInfo, nameof(signerInfo));

            DateTimeOffset? signingTime = null;
            var attribute = signerInfo
                .SignedAttributes
                .Cast<CryptographicAttributeObject>()
                .Where(a => a.Oid.Value.Equals(Constants.SigningTimeOid, StringComparison.OrdinalIgnoreCase))
                .Select(a => new Pkcs9SigningTime(a.Values.Cast<AsnEncodedData>().First().RawData))
                .FirstOrDefault();

            if (attribute != null)
            {
                signingTime = attribute.SigningTime.ToUniversalTime();
            }

            return new Signatory(signerInfo.Certificate, signingTime);
        }
    }
}