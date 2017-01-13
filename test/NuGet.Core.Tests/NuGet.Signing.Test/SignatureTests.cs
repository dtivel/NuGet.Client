// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Threading;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Xunit;

namespace NuGet.Signing.Test
{
    public class SignatureTests : IClassFixture<SignatureTestsFixture>
    {
        private readonly SignatureTestsFixture _fixture;

        public SignatureTests(SignatureTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void FromSignedCms_ThrowsForNullSignedCms()
        {
            Assert.Throws<ArgumentNullException>(() => Signature.FromSignedCms(signedCms: null, signatureTargets: _fixture.SignatureTargets));
        }

        [Fact]
        public void FromSignedCms_ThrowsForNullSignatureTargets()
        {
            Assert.Throws<ArgumentNullException>(() => Signature.FromSignedCms(_fixture.SignedCms, signatureTargets: null));
        }

        [Fact]
        public void FromSignedCms_CreatesSignature()
        {
            var signature = Signature.FromSignedCms(_fixture.SignedCms, _fixture.SignatureTargets);

            Assert.NotNull(signature);
            Assert.NotNull(signature.Signatory);
            Assert.NotNull(signature.SignatureTargets);
            Assert.Same(_fixture.SignatureTargets, signature.SignatureTargets);
            Assert.Equal("distinguishedName=CN=Test;publicKeyHash=sha512:CXPVoklMG29HcHIpDZzBZMaDbqx7KGO4rhKeyr4uVNsibgu0BLirlINgrhLQiehdgjsaDVasale4X68cSJnt6g==", signature.SignerIdentity.ToString());
        }
    }

    public class SignatureTestsFixture
    {
        internal SignatureTargets SignatureTargets { get; }
        internal SignedCms SignedCms { get; }

        public SignatureTestsFixture()
        {
            var certificate = TestData.GetCertificate("SelfSignedCertificate.pfx");
            byte[] digest;

            using (var stream = new MemoryStream())
            {
                digest = SigningUtilities.ComputeDigestAsync(stream, CancellationToken.None).GetAwaiter().GetResult();
            }

            var packageIdentity = new PackageIdentity("NuGet.Core", new NuGetVersion("2.12.0"));
            var contentDigest = new ContentDigest(new Oid(Constants.Sha512Oid), digest);
            var signatureTarget = new SignatureTarget(packageIdentity, contentDigest);
            var signatureTargets = new SignatureTargets(signatureTarget);
            var contentInfo = new ContentInfo(digest);
            var signedCms = new SignedCms(contentInfo);
            var cmsSigner = new CmsSigner(certificate);

            signedCms.ComputeSignature(cmsSigner);

            SignatureTargets = signatureTargets;
            SignedCms = signedCms;
        }
    }
}