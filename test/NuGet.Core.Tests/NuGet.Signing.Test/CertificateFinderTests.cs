// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Signing.Test
{
    public class CertificateFinderTests : IClassFixture<CertificateFinderTestsFixture>
    {
        private readonly CertificateFinderTestsFixture _fixture;

        public CertificateFinderTests(CertificateFinderTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void FindCertificates_ThrowsForNullOptions()
        {
            Assert.Throws<ArgumentNullException>(() => CertificateFinder.FindCertificates(options: null));
        }

        [Fact]
        public void FindCertificates_ThrowsForNullCertificates()
        {
            Assert.Throws<ArgumentNullException>(() => CertificateFinder.FindCertificates(certificates: null, options: new CertificateFindOptions()));
        }

        [Fact]
        public void FindCertificates_ThrowsForNullOptions2()
        {
            Assert.Throws<ArgumentNullException>(() => CertificateFinder.FindCertificates(new X509Certificate2Collection(), options: null));
        }

        [Fact]
        public void FindCertificates_SupportsEmptyCertificateCollection()
        {
            var certificates = CertificateFinder.FindCertificates(new X509Certificate2Collection(), new CertificateFindOptions());

            Assert.Equal(0, certificates.Count);
        }

        [Fact]
        public void FindCertificates_FindsCertificatesWithNoAdditionalCriteria()
        {
            var certificates = CertificateFinder.FindCertificates(_fixture.Certificates, new CertificateFindOptions());

            Assert.Equal(3, certificates.Count);
        }

        [Fact]
        public void FindCertificates_FindsCertificatesBySubjectName()
        {
            var options = new CertificateFindOptions() { SubjectName = "Test" };
            var certificates = CertificateFinder.FindCertificates(_fixture.Certificates, options);

            Assert.Equal(3, certificates.Count);
        }

        [Fact]
        public void FindCertificates_FindsCertificateBySubjectNameCaseInsensitively()
        {
            var options = new CertificateFindOptions() { SubjectName = "test (rsa 2048)" };
            var certificates = CertificateFinder.FindCertificates(_fixture.Certificates, options);

            Assert.Equal(1, certificates.Count);
        }

        [Fact]
        public void FindCertificates_FindsNothingBySubjectName()
        {
            var options = new CertificateFindOptions() { SubjectName = "nonexistent subject name" };
            var certificates = CertificateFinder.FindCertificates(_fixture.Certificates, options);

            Assert.Equal(0, certificates.Count);
        }

        [Fact]
        public void FindCertificates_FindsCertificateByFingerprint()
        {
            var options = new CertificateFindOptions() { Fingerprint = "DBBCA846DEB903E718EDB6669891F9AE9C04D208" };
            var certificates = CertificateFinder.FindCertificates(_fixture.Certificates, options);

            Assert.Equal(1, certificates.Count);
        }

        [Fact]
        public void FindCertificates_FindsCertificateByFingerprintCaseInsensitively()
        {
            var options = new CertificateFindOptions() { Fingerprint = "f7c10677e4df094cbf67c7fc13599a72513df240" };
            var certificates = CertificateFinder.FindCertificates(_fixture.Certificates, options);

            Assert.Equal(1, certificates.Count);
        }

        [Fact]
        public void FindCertificates_FindsNothingByFingerprint()
        {
            var options = new CertificateFindOptions() { Fingerprint = "nonexistent fingerprint" };
            var certificates = CertificateFinder.FindCertificates(_fixture.Certificates, options);

            Assert.Equal(0, certificates.Count);
        }

        [Fact]
        public void FindCertificates_ThrowsForInvalidPassword()
        {
            var bytes = TestData.GetResourceBytes("SelfSignedCertificateWithPassword.pfx");

            using (var testDirectory = TestDirectory.Create())
            {
                var filePath = Path.Combine(testDirectory.Path, "cert.pfx");

                File.WriteAllBytes(filePath, bytes);

                var options = new CertificateFindOptions() { SecretPath = filePath };

                Assert.Throws<ArgumentException>(() => CertificateFinder.FindCertificates(options));
            }
        }

        [Fact]
        public void FindCertificates_FindsCertificateWithPassword()
        {
            var bytes = TestData.GetResourceBytes("SelfSignedCertificateWithPassword.pfx");

            using (var testDirectory = TestDirectory.Create())
            {
                var filePath = Path.Combine(testDirectory.Path, "cert.pfx");

                File.WriteAllBytes(filePath, bytes);

                var secretPassphrase = new SecureString();

                foreach (var c in "password")
                {
                    secretPassphrase.AppendChar(c);
                }

                using (var options = new CertificateFindOptions()
                {
                    SecretPath = filePath,
                    SecretPassphrase = secretPassphrase
                })
                {
                    var certificates = CertificateFinder.FindCertificates(options);

                    Assert.Equal(1, certificates.Count);
                }
            }
        }
    }

    public class CertificateFinderTestsFixture
    {
        internal X509Certificate2Collection Certificates { get; }

        public CertificateFinderTestsFixture()
        {
            Certificates = new X509Certificate2Collection();

            /*
             *  Valid certificates
             */

            // New-SelfSignedCertificateEx -Subject "CN=Test (RSA 2048)" -KeySpec "Signature" -KeyUsage "DigitalSignature" -EKU "Code Signing" -FriendlyName "Test"-AlgorithmName "RSA" -KeyLength 2048 -SignatureAlgorithm "SHA512" -NotBefore $([DateTime]::Now.AddYears(-1)) -NotAfter $([DateTime]::Now.AddYears(50)) -Exportable -Path .\Valid_RSA2048.pfx
            Certificates.Add(TestData.GetCertificate("Valid_RSA2048.pfx"));

            // New-SelfSignedCertificateEx -Subject "CN=Test (RSA 4096)" -KeySpec "Signature" -KeyUsage "DigitalSignature" -EKU "Code Signing" -FriendlyName "Test"-AlgorithmName "RSA" -KeyLength 4096 -SignatureAlgorithm "SHA512" -NotBefore $([DateTime]::Now.AddYears(-1)) -NotAfter $([DateTime]::Now.AddYears(50)) -Exportable -Path .\Valid_RSA4096.pfx
            Certificates.Add(TestData.GetCertificate("Valid_RSA4096.pfx"));

            // New-SelfSignedCertificateEx -Subject "CN=Test (SHA-256 Signature Algorithm)" -KeySpec "Signature" -KeyUsage "DigitalSignature" -EKU "Code Signing" -FriendlyName "Test"-AlgorithmName "RSA" -KeyLength 2048 -SignatureAlgorithm "SHA256" -NotBefore $([DateTime]::Now.AddYears(-1)) -NotAfter $([DateTime]::Now.AddYears(50)) -Exportable -Path .\Valid_SHA256SignatureAlgorithm.pfx
            Certificates.Add(TestData.GetCertificate("Valid_SHA256SignatureAlgorithm.pfx"));

            /*
             *  Invalid certificates
             */

            // New-SelfSignedCertificateEx -Subject "CN=Test (Invalid EKU)" -KeySpec "Signature" -KeyUsage "DigitalSignature" -FriendlyName "Test"-AlgorithmName "RSA" -KeyLength 2048 -SignatureAlgorithm "SHA512" -NotBefore $([DateTime]::Now.AddYears(-1)) -NotAfter $([DateTime]::Now.AddYears(50)) -Exportable -Path .\Invalid_Eku.pfx
            Certificates.Add(TestData.GetCertificate("Invalid_Eku.pfx"));

            // New-SelfSignedCertificateEx -Subject "CN=Test (Invalid KeyLength)" -KeySpec "Signature" -KeyUsage "DigitalSignature" -EKU "Code Signing" -FriendlyName "Test"-AlgorithmName "RSA" -KeyLength 1024 -SignatureAlgorithm "SHA512" -NotBefore $([DateTime]::Now.AddYears(-1)) -NotAfter $([DateTime]::Now.AddYears(50)) -Exportable -Path .\Invalid_KeyLength.pfx
            Certificates.Add(TestData.GetCertificate("Invalid_KeyLength.pfx"));

            // New-SelfSignedCertificateEx -Subject "CN=Test (Invalid KeyUsage)" -KeySpec "Signature" -KeyUsage "DataEncipherment" -EKU "Code Signing" -FriendlyName "Test"-AlgorithmName "RSA" -KeyLength 2048 -SignatureAlgorithm "SHA512" -NotBefore $([DateTime]::Now.AddYears(-1)) -NotAfter $([DateTime]::Now.AddYears(50)) -Exportable -Path .\Invalid_KeyUsage.pfx
            Certificates.Add(TestData.GetCertificate("Invalid_KeyUsage.pfx"));

            // New-SelfSignedCertificateEx -Subject "CN=Test (Invalid NotAfter)" -KeySpec "Signature" -KeyUsage "DigitalSignature" -EKU "Code Signing" -FriendlyName "Test"-AlgorithmName "RSA" -KeyLength 2048 -SignatureAlgorithm "SHA512" -NotBefore $([DateTime]::Now.AddYears(-2)) -NotAfter $([DateTime]::Now.AddYears(-1)) -Exportable -Path .\Invalid_NotAfter.pfx
            Certificates.Add(TestData.GetCertificate("Invalid_NotAfter.pfx"));

            // New-SelfSignedCertificateEx -Subject "CN=Test (Invalid NotBefore)" -KeySpec "Signature" -KeyUsage "DigitalSignature" -EKU "Code Signing" -FriendlyName "Test"-AlgorithmName "RSA" -KeyLength 2048 -SignatureAlgorithm "SHA512" -NotBefore $([DateTime]::Now.AddYears(49)) -NotAfter $([DateTime]::Now.AddYears(50)) -Exportable -Path .\Invalid_NotBefore.pfx
            Certificates.Add(TestData.GetCertificate("Invalid_NotBefore.pfx"));
        }
    }
}