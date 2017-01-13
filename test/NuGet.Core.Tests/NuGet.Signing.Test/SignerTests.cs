// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Signing.Test
{
    public class SignerTests
    {
        private readonly X509Certificate2 _certificate;
        private readonly Signer _signer;

        public SignerTests()
        {
            _certificate = TestData.GetCertificate("SelfSignedCertificate.pfx");
            _signer = new Signer(_certificate, new X509Certificate2Collection(), allowUntrustedRoot: true);
        }

        [Fact]
        public void Constructor_ThrowsForNullCertificate()
        {
            Assert.Throws<ArgumentNullException>(() => new Signer(certificate: null));
        }

        [Fact]
        public async Task SignAsync_ThrowsForNullFilePath()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _signer.SignAsync(filePath: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task SignAsync_ThrowsForEmptyFilePath()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _signer.SignAsync(filePath: string.Empty, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task SignAsync_ThrowsForNonExistentFilePath()
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            await Assert.ThrowsAsync<FileNotFoundException>(() => _signer.SignAsync(filePath, CancellationToken.None));
        }

        [Fact]
        public async Task SignAsync_ThrowsIfCancelled()
        {
            var filePath = Path.Combine(Path.GetTempPath(), "a");

            await Assert.ThrowsAsync<OperationCanceledException>(() => _signer.SignAsync(filePath, new CancellationToken(canceled: true)));
        }

        [Fact]
        public async Task SignAsync_ThrowsForSelfSignedCertificate()
        {
            using (var testDirectory = TestDirectory.Create())
            {
                const string resourceName = "NuGet.Core.2.12.0.nupkg";
                var filePath = Path.Combine(testDirectory.Path, resourceName);

                TestData.CopyResourceToFile(resourceName, filePath);

                var signer = new Signer(_certificate);

                var ex = await Assert.ThrowsAsync<InvalidSigningCertificateException>(() => signer.SignAsync(filePath, CancellationToken.None));

                Assert.Equal(1, ex.ChainStatus.Count);
                Assert.Equal(X509ChainStatusFlags.UntrustedRoot, ex.ChainStatus[0].Status);
                Assert.Equal(1, ex.ChainElements.Count);
                Assert.Equal(_certificate, ex.ChainElements[0].Certificate);
            }
        }

        [Fact]
        public async Task SignAsync_ThrowsForInvalidPackageFile()
        {
            using (var testDirectory = TestDirectory.Create())
            {
                var filePath = Path.Combine(testDirectory.Path, "InvalidPackageFile.nupkg");

                File.WriteAllText(filePath, string.Empty);

                await Assert.ThrowsAsync<InvalidDataException>(() => _signer.SignAsync(filePath, CancellationToken.None));
            }
        }

        [Fact]
        public async Task SignAsync_GeneratesSignature()
        {
            using (var testDirectory = TestDirectory.Create())
            {
                const string resourceName = "NuGet.Core.2.12.0.nupkg";
                var filePath = Path.Combine(testDirectory.Path, resourceName);

                TestData.CopyResourceToFile(resourceName, filePath);

                var signature = await _signer.SignAsync(filePath, CancellationToken.None);

                Assert.Equal(_certificate.Thumbprint, signature.Signatory.Certificate.Thumbprint);
            }
        }
    }
}