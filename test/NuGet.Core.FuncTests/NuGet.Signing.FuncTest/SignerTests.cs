// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Signing.FuncTest
{
    public class SignerTests
    {
        private readonly Signer _signer;

        public SignerTests()
        {
            var certificate = TestData.GetCertificate("SelfSignedCertificate.pfx");

            _signer = new Signer(certificate, new X509Certificate2Collection(), allowUntrustedRoot: true);
        }

        [Fact]
        public async Task SignAsync_SignatureRoundTrips()
        {
            using (var testDirectory = TestDirectory.Create())
            {
                var packageFilePath = Path.Combine(testDirectory.Path, "NuGet.Core.2.12.0.nupkg");
                TestData.CopyToFile("NuGet.Core.2.12.0.nupkg", packageFilePath);

                var expectedSignature = await _signer.SignAsync(packageFilePath, CancellationToken.None);

                using (var writer = new StringWriter())
                {
                    await PemEncoder.EncodeAsync(expectedSignature, writer, CancellationToken.None);

                    using (var reader = new StringReader(writer.ToString()))
                    {
                        var detachedSignatureFile = await DetachedSignatureFile.ReadAsync(reader, CancellationToken.None);
                        var actualSignature = detachedSignatureFile.Signatures.Single();

                        Assert.Equal(expectedSignature.SignerIdentity, actualSignature.SignerIdentity);
                        Assert.Equal(expectedSignature.SignatureTargets.Version, actualSignature.SignatureTargets.Version);

                        var expectedTarget = expectedSignature.SignatureTargets.SignatureTarget;
                        var actualTarget = actualSignature.SignatureTargets.SignatureTarget;

                        Assert.Equal(expectedTarget.Version, actualTarget.Version);
                        Assert.Equal(expectedTarget.PackageIdentity, actualTarget.PackageIdentity);
                        Assert.Equal(expectedTarget.ContentDigest.DigestAlgorithm.Value, actualTarget.ContentDigest.DigestAlgorithm.Value);
                        Assert.Equal(Convert.ToBase64String(expectedTarget.ContentDigest.Digest), Convert.ToBase64String(actualTarget.ContentDigest.Digest));
                    }
                }
            }
        }
    }
}