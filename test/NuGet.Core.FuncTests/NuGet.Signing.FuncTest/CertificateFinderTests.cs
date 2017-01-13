// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Signing.FuncTest
{
    public class CertificateFinderTests
    {
        [Fact]
        public void FindCertificates_ThrowsForUnknownStore()
        {
            var options = new CertificateFindOptions() { StoreName = "Nonexistent certificate store" };

            Assert.Throws<ArgumentException>(() => CertificateFinder.FindCertificates(options));
        }

        [Fact]
        public void FindCertificates_SupportsCurrentUserCertificateStore()
        {
            using (var store = new CertificateStore(StoreLocation.CurrentUser))
            {
                var options = new CertificateFindOptions() { StoreName = store.X509Store.Name };
                var certificates = CertificateFinder.FindCertificates(options);

                Assert.Equal(1, certificates.Count);
            }
        }

        [Fact]
        public void FindCertificates_SupportsLocalMachineCertificateStore()
        {
            using (var store = new CertificateStore(StoreLocation.LocalMachine))
            {
                var options = new CertificateFindOptions() { StoreName = store.X509Store.Name, IsLocalMachineStore = true };
                var certificates = CertificateFinder.FindCertificates(options);

                Assert.Equal(1, certificates.Count);
            }
        }
    }

    internal sealed class CertificateStore : IDisposable
    {
        private readonly string _storeName;

        internal X509Store X509Store { get; }

        internal CertificateStore(StoreLocation location)
        {
            _storeName = $"Temporary Test Store for NuGet Package Signing ({DateTimeOffset.UtcNow.ToString()})";

            DeleteStoreIfExists();

            X509Store = new X509Store(_storeName, location);

            X509Store.Open(OpenFlags.ReadWrite);
            X509Store.Add(LoadCertificate("Valid_RSA2048.pfx"));
        }

        public void Dispose()
        {
            if (X509Store != null)
            {
                X509Store.Dispose();
            }

            DeleteStoreIfExists();
        }

        private void DeleteStoreIfExists()
        {
            if (!NativeMethods.CertUnregisterSystemStore(_storeName, NativeMethods.CERT_SYSTEM_STORE_CURRENT_USER | NativeMethods.CERT_STORE_DELETE_FLAG))
            {
                int hr = Marshal.GetHRForLastWin32Error();

                if (hr != NativeMethods.ERROR_FILE_NOT_FOUND_HRESULT)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }

        private static X509Certificate2 LoadCertificate(string certificateFileName)
        {
            var bytes = ResourceTestUtility.GetResourceBytes($"NuGet.Signing.FuncTest.compiler.resources.{certificateFileName}", typeof(CertificateStore));

            return new X509Certificate2(bytes);
        }
    }
}