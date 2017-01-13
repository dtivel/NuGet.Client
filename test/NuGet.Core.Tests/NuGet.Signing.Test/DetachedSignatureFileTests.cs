// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Signing.Test
{
    public class DetachedSignatureFileTests
    {
        [Fact]
        public async Task ReadAsync_ThrowsForNullReader()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DetachedSignatureFile.ReadAsync(reader: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task ReadAsync_ThrowsForNullFilePath()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DetachedSignatureFile.ReadAsync(filePath: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task ReadAsync_ThrowsForEmptyFilePath()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => DetachedSignatureFile.ReadAsync(filePath: string.Empty, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task ReadAsync_ThrowsForFileNotFound()
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            await Assert.ThrowsAsync<FileNotFoundException>(() => DetachedSignatureFile.ReadAsync(filePath, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task ReadAsync_ThrowsIfCancelled_WithReader()
        {
            using (var reader = new StringReader(string.Empty))
            {
                await Assert.ThrowsAsync<OperationCanceledException>(() => DetachedSignatureFile.ReadAsync(reader, new CancellationToken(canceled: true)));
            }
        }

        [Fact]
        public async Task ReadAsync_ThrowsIfCancelled_WithFilePath()
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            await Assert.ThrowsAsync<OperationCanceledException>(() => DetachedSignatureFile.ReadAsync(filePath, new CancellationToken(canceled: true)));
        }

        [Fact]
        public async Task ReadAsync_DecodesSignature_WithReader()
        {
            var bytes = TestData.GetResourceBytes("NuGet.Core.2.12.0.nupkg.sig");
            var content = Encoding.UTF8.GetString(bytes);

            using (var reader = new StringReader(content))
            {
                var file = await DetachedSignatureFile.ReadAsync(reader, CancellationToken.None);

                Assert.Equal(1, file.Signatures.Count());

                var signature = file.Signatures.First();

                Assert.Equal(1, signature.SignatureTargets.Version);

                var signatureTarget = signature.SignatureTargets.SignatureTarget;

                Assert.Equal(1, signatureTarget.Version);
                Assert.Equal("NuGet.Core", signatureTarget.PackageIdentity.Id);
                Assert.Equal("2.12.0", signatureTarget.PackageIdentity.Version.ToNormalizedString());
                Assert.Equal(Constants.Sha512Oid, signatureTarget.ContentDigest.DigestAlgorithm.Value);
                Assert.Equal("kSDD1VFIq7BBrimjMRk9HeeQlZteyknbHgz3AT4AUVUCZ8BZaCSzO1A5UTVPLLbSHryjozPcEmNYSds/IDAIjw==", Convert.ToBase64String(signatureTarget.ContentDigest.Digest));
            }
        }

        [Fact]
        public async Task ReadAsync_DecodesSignature_WithFilePath()
        {
            const string resourceName = "NuGet.Core.2.12.0.nupkg.sig";

            using (var testDirectory = TestDirectory.Create())
            {
                var filePath = Path.Combine(testDirectory.Path, resourceName);

                TestData.CopyResourceToFile(resourceName, filePath);

                var file = await DetachedSignatureFile.ReadAsync(filePath, CancellationToken.None);

                Assert.Equal(1, file.Signatures.Count());

                var signature = file.Signatures.First();

                Assert.Equal(1, signature.SignatureTargets.Version);

                var signatureTarget = signature.SignatureTargets.SignatureTarget;

                Assert.Equal(1, signatureTarget.Version);
                Assert.Equal("NuGet.Core", signatureTarget.PackageIdentity.Id);
                Assert.Equal("2.12.0", signatureTarget.PackageIdentity.Version.ToNormalizedString());
                Assert.Equal(Constants.Sha512Oid, signatureTarget.ContentDigest.DigestAlgorithm.Value);
                Assert.Equal("kSDD1VFIq7BBrimjMRk9HeeQlZteyknbHgz3AT4AUVUCZ8BZaCSzO1A5UTVPLLbSHryjozPcEmNYSds/IDAIjw==", Convert.ToBase64String(signatureTarget.ContentDigest.Digest));
            }
        }
    }
}