// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace NuGet.Signing.Test
{
    public class SignerIdentityTests : IClassFixture<SignerIdentityTestsFixture>
    {
        private readonly SignerIdentityTestsFixture _fixture;

        public SignerIdentityTests(SignerIdentityTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Create_ThrowsForNullCertificate()
        {
            Assert.Throws<ArgumentNullException>(() => SignerIdentity.Create(certificate: null));
        }

        [Fact]
        public void Create_SupportsMultipartDistinguishedName()
        {
            var signerIdentity = SignerIdentity.Create(_fixture.Certificate);

            Assert.NotNull(signerIdentity);

            Assert.Equal("CN=A, DC=B", signerIdentity.DistingishedName);
            Assert.Equal("sha512", signerIdentity.HashAlgorithmName);
            Assert.Equal("fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==", signerIdentity.PublicKeyHash);
        }

        [Fact]
        public void Parse_ThrowsForNullSignerIdentityString()
        {
            Assert.Throws<ArgumentNullException>(() => SignerIdentity.Parse(signerIdentityString: null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        [InlineData("DISTINGUISHEDNAME=CN=A;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        [InlineData("distinguishedName=;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        [InlineData("distinguishedName=CN=A;")]
        [InlineData("distinguishedName=CN=A;PUBLICKEYHASH=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        [InlineData("distinguishedName=CN=A;publicKeyHash=")]
        [InlineData("distinguishedName=CN=A;publicKeyHash=sha256:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        [InlineData("distinguishedName=CN=A;publicKeyHash=sha512:")]
        [InlineData("distinguishedName=CN=A;publicKeyHash=sha512:.")]
        [InlineData("distinguishedName=CN=A;publicKeyHash=SHA512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        public void Parse_ThrowsForInvalidSignerIdentityString(string signerIdentityString)
        {
            Assert.Throws<ArgumentException>(() => SignerIdentity.Parse(signerIdentityString));
        }

        [Theory]
        [InlineData("distinguishedName=CN=A;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==", "CN=A", "sha512", "fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        [InlineData("distinguishedName=cn=A;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==", "cn=A", "sha512", "fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        public void Parse_SupportsValidInput(string signerIdentityString, string expectedSubjectName, string expectedHashAlgorithmName, string expectedPublicKeyHash)
        {
            var signerIdentity = SignerIdentity.Parse(signerIdentityString);

            Assert.Equal(expectedSubjectName, signerIdentity.DistingishedName);
            Assert.Equal(expectedHashAlgorithmName, signerIdentity.HashAlgorithmName);
            Assert.Equal(expectedPublicKeyHash, signerIdentity.PublicKeyHash);
        }

        [Theory]
        [InlineData("distinguishedName=CN=A;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        public void ToString_GeneratesValidOutput(string expectedSignerIdentityString)
        {
            var signerIdentity = SignerIdentity.Parse(expectedSignerIdentityString);
            var actualSignerIdentityString = signerIdentity.ToString();

            Assert.Equal(expectedSignerIdentityString, actualSignerIdentityString);
        }

        [Fact]
        public void Equals_True()
        {
            const string signerIdentityString = "distinguishedName=CN=A;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==";
            var a = SignerIdentity.Parse(signerIdentityString);
            var b = SignerIdentity.Parse(signerIdentityString);

            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Theory]
        [InlineData("distinguishedName=CN=a;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==",
                    "distinguishedName=CN=A;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        [InlineData("distinguishedName=CN=A;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==",
                    "distinguishedName=CN=B;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==")]
        [InlineData("distinguishedName=CN=A;publicKeyHash=sha512:fdXqFp1ZxLtjcVsiD2M2FRvE2xjlDvmS3qrsKolRlh81IE88LxqE7gjWyFWl4t50LvePFBb2xcJ/lHQAeZIYRg==",
                    "distinguishedName=CN=A;publicKeyHash=sha512:CXPVoklMG29HcHIpDZzBZMaDbqx7KGO4rhKeyr4uVNsibgu0BLirlINgrhLQiehdgjsaDVasale4X68cSJnt6g==")]
        public void Equals_False(string signerIdentityStringA, string signerIdentityStringB)
        {
            var a = SignerIdentity.Parse(signerIdentityStringA);
            var b = SignerIdentity.Parse(signerIdentityStringB);

            Assert.False(a.Equals(b));
            Assert.False(a.Equals(null));
            Assert.False(b.Equals(null));
            Assert.False(a == b);
            Assert.False(a == null);
            Assert.False(b == null);
            Assert.True(a != b);
        }
    }

    public class SignerIdentityTestsFixture
    {
        internal X509Certificate2 Certificate { get; }

        public SignerIdentityTestsFixture()
        {
            // New-SelfSignedCertificateEx -Subject "CN=A,DC=B" -EKU "Code Signing" -KeySpec "Signature" -KeyUsage "DigitalSignature" -FriendlyName "Test" -NotAfter $([DateTime]::Now.AddYears(20))
            Certificate = TestData.GetCertificate("MultipartSubjectName.pfx");
        }
    }
}