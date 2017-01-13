// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Xunit;

namespace NuGet.Signing.Test
{
    public class PemEncoderTests
    {
        [Fact]
        public async Task EncodeAsync_ThrowsForNullSignature()
        {
            using (var writer = new StringWriter())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => PemEncoder.EncodeAsync(signature: null, writer: writer, cancellationToken: CancellationToken.None));
            }
        }

        [Fact]
        public async Task EncodeAsync_ThrowsForNullWriter()
        {
            var signature = await CreateSignature();

            await Assert.ThrowsAsync<ArgumentNullException>(() => PemEncoder.EncodeAsync(signature, writer: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task EncodeAsync_ThrowsIfCancelled()
        {
            var signature = await CreateSignature();

            using (var writer = new StringWriter())
            {
                await Assert.ThrowsAsync<OperationCanceledException>(() => PemEncoder.EncodeAsync(signature, writer, new CancellationToken(canceled: true)));
            }
        }

        [Fact]
        public async Task EncodeAsync_GeneratesPemEncodedText()
        {
            var signature = await CreateSignature();

            using (var writer = new StringWriter())
            {
                await PemEncoder.EncodeAsync(signature, writer, CancellationToken.None);

                using (var reader = new StringReader(writer.ToString()))
                {
                    var pemDatas = await PemParser.ParseAsync(reader, CancellationToken.None);

                    Assert.Equal(1, pemDatas.Count());
                    Assert.Equal("FILE SIGNATURE", pemDatas.First().Label);
                }
            }
        }

        private static async Task<Signature> CreateSignature()
        {
            var certificate = TestData.GetCertificate("SelfSignedCertificate.pfx");
            byte[] digest;

            using (var stream = new MemoryStream())
            {
                digest = await SigningUtilities.ComputeDigestAsync(stream, CancellationToken.None);
            }

            var packageIdentity = new PackageIdentity("NuGet.Core", new NuGetVersion("2.12.0"));
            var contentDigest = new ContentDigest(new Oid(Constants.Sha512Oid), digest);
            var signatureTarget = new SignatureTarget(packageIdentity, contentDigest);
            var signatureTargets = new SignatureTargets(signatureTarget);
            var contentInfo = new ContentInfo(digest);
            var signedCms = new SignedCms(contentInfo);
            var cmsSigner = new CmsSigner(certificate);

            signedCms.ComputeSignature(cmsSigner);

            return Signature.FromSignedCms(signedCms, signatureTargets);
        }
    }
}