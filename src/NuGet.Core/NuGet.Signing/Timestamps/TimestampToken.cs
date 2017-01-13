// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NuGet.Signing
{
    internal sealed class TimestampToken
    {
        private TimestampToken(int version, string tsaPolicyId, string hashAlgorithm, byte[] hashedMessage, DateTime timestampUtc, bool ordered, Signatory signatory)
        {
            Version = version;
            TsaPolicyId = tsaPolicyId;
            HashAlgorithm = hashAlgorithm;
            HashedMessage = hashedMessage;
            DateTime = timestampUtc;
            Ordered = ordered;
            Signatory = signatory;
        }

        public int Version { get; }
        public bool IsTrusted { get; }
        public string TsaPolicyId { get; }
        public string HashAlgorithm { get; }
        public byte[] HashedMessage { get; }
        public DateTimeOffset DateTime { get; }
        public bool Ordered { get; }
        public Signatory Signatory { get; }

        internal static TimestampToken FromTimestampInfo(CRYPT_TIMESTAMP_INFO info, Signatory signatory)
        {
            string hashAlgorithm;
            try
            {
                var oid = Oid.FromOidValue(info.HashAlgorithm.pszObjId, OidGroup.HashAlgorithm);
                hashAlgorithm = oid.FriendlyName;
            }
            catch (Exception)
            {
                hashAlgorithm = info.HashAlgorithm.pszObjId;
            }

            var hashedMessage = new byte[info.HashedMessage.cbData];
            Marshal.Copy(info.HashedMessage.pbData, hashedMessage, 0, hashedMessage.Length);

            return new TimestampToken(
                (int)info.dwVersion,
                info.pszTSAPolicyId,
                hashAlgorithm,
                hashedMessage,
                System.DateTime.FromFileTime((long)(((ulong)(uint)info.ftTime.dwHighDateTime << 32) | (uint)info.ftTime.dwLowDateTime)).ToUniversalTime(),
                info.fOrdering,
                signatory);
        }
    }
}