// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing
{
    /// <summary>
    /// Package signing constants.
    /// </summary>
    public static class Constants
    {
        internal const string DefaultDigestAlgorithmName = "sha512";
        internal const string DefaultHashAlgorithmName = "sha512";

        // RFC 5652 "signing-time" attribute, https://tools.ietf.org/html/rfc5652#section-11.3
        internal const string SigningTimeOid = "1.2.840.113549.1.9.5";

        // RFC 5126 (CAdES), http://tools.ietf.org/html/rfc5126#section-6.1.1
        internal const string SignatureTimeStampTokenAttributeOid = "1.2.840.113549.1.9.16.2.14";

        /// <summary>
        /// The object identifier for the SHA-256 hash algorithm.
        /// </summary>
        /// <remarks>
        /// See RFC 8017 appendix B.1 (https://tools.ietf.org/html/rfc8017#appendix-B.1).
        /// </remarks>
        public const string Sha256Oid = "2.16.840.1.101.3.4.2.1";

        /// <summary>
        /// The object identifier for the SHA-512 hash algorithm.
        /// </summary>
        /// <remarks>
        /// See RFC 8017 appendix B.1 (https://tools.ietf.org/html/rfc8017#appendix-B.1).
        /// </remarks>
        public const string Sha512Oid = "2.16.840.1.101.3.4.2.3";

        // RFC 8017 signature algorithms, https://tools.ietf.org/html/rfc8017#appendix-A.2.4
        internal const string Sha512RsaOid = "1.2.840.113549.1.1.13";

        // RFC 5280 codeSigning attribute, https://tools.ietf.org/html/rfc5280#section-4.2.1.12
        internal const string CodeSigningEkuOid = "1.3.6.1.5.5.7.3.3";

        // RFC 5652 "id-data", https://tools.ietf.org/html/rfc5652#section-4
        internal const string Pkcs7DataOid = "1.2.840.113549.1.7.1";
    }
}