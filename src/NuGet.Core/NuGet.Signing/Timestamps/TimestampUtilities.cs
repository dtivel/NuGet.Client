// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Pkcs;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Signing
{
    internal static class TimestampUtilities
    {
        internal static Task<SignedCms> RequestTimestampAsync(byte[] data, string hashAlgorithmOid, Uri timestampingAuthorityUrl, CancellationToken cancellationToken)
        {
            return Task.Run(() => RequestTimestamp(data, hashAlgorithmOid, timestampingAuthorityUrl), cancellationToken);
        }

        private static SignedCms RequestTimestamp(byte[] data, string hashAlgorithmOid, Uri timestampingAuthorityUrl)
        {
            var para = new CRYPT_TIMESTAMP_PARA()
            {
                fRequestCerts = true
            };

            var unmanagedContext = SafeCryptMemHandle.InvalidHandle;

            NativeUtilities.ThrowIfFailed(NativeMethods.CryptRetrieveTimeStamp(
                wszUrl: timestampingAuthorityUrl.ToString(),
                dwRetrievalFlags: NativeMethods.TIMESTAMP_VERIFY_CONTEXT_SIGNATURE,
                dwTimeout: 5 * 1000 /* 5 second timeout */,
                pszHashId: hashAlgorithmOid,
                pPara: ref para,
                pbData: data,
                cbData: (uint)data.Length,
                ppTsContext: out unmanagedContext,
                ppTsSigner: IntPtr.Zero,
                phStore: IntPtr.Zero));

            byte[] encodedResponse;

            using (unmanagedContext)
            {
                var context = Marshal.PtrToStructure<CRYPT_TIMESTAMP_CONTEXT>(unmanagedContext.DangerousGetHandle());

                encodedResponse = new byte[context.cbEncoded];

                Marshal.Copy(context.pbEncoded, encodedResponse, 0, (int)context.cbEncoded);
            }

            var signedCms = new SignedCms();

            signedCms.Decode(encodedResponse);

            return signedCms;
        }

        internal static TimestampToken VerifyTimestamp(byte[] data, SignedCms timestampCms)
        {
            var signatory = Signatory.FromSignerInfo(timestampCms.SignerInfos[0]);
            var trusted = signatory.Certificate.Verify();
            var contentInfo = timestampCms.Encode();
            var unmanagedContext = SafeCryptMemHandle.InvalidHandle;

            NativeUtilities.ThrowIfFailed(NativeMethods.CryptVerifyTimeStampSignature(
                pbTSContentInfo: contentInfo,
                cbTSContentInfo: (uint)contentInfo.Length,
                pbData: data,
                cbData: (uint)data.Length,
                hAdditionalStore: IntPtr.Zero,
                ppTsContext: out unmanagedContext,
                ppTsSigner: IntPtr.Zero,
                phStore: IntPtr.Zero));

            using (unmanagedContext)
            {
                var context = Marshal.PtrToStructure<CRYPT_TIMESTAMP_CONTEXT>(unmanagedContext.DangerousGetHandle());
                var info = Marshal.PtrToStructure<CRYPT_TIMESTAMP_INFO>(context.pTimeStamp);

                return TimestampToken.FromTimestampInfo(info, signatory);
            }
        }
    }
}