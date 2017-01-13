// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NuGet.Signing
{
    /// <summary>
    /// Utility class for finding X.509 certificates.
    /// </summary>
    public static class CertificateFinder
    {
        /// <summary>
        /// Finds X.509 certificates matching the provided find options.
        /// </summary>
        /// <param name="options">Options to narrow the certificate search.</param>
        /// <returns>A <see cref="X509Certificate2Collection" />
        /// containing matching certificates.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options" /> is <c>null</c>.</exception>
        public static X509Certificate2Collection FindCertificates(CertificateFindOptions options)
        {
            Assert.IsNotNull(options, nameof(options));

            if (string.IsNullOrEmpty(options.SecretPath))
            {
                using (var store = GetStore(options))
                {
                    OpenStore(store);

                    return FindCertificates(store.Certificates, options);
                }
            }

            return LoadCertificate(options);
        }

        // This is public only to facilitate testing.
        public static X509Certificate2Collection FindCertificates(X509Certificate2Collection certificates, CertificateFindOptions options)
        {
            Assert.IsNotNull(certificates, nameof(certificates));
            Assert.IsNotNull(options, nameof(options));

            // X509Certificate2.NotBefore and X509Certificate2.NotAfter return local DateTime values.
            var filteredCertificates = certificates
                .Find(X509FindType.FindByTimeValid, DateTime.Now, validOnly: false);

            if (!string.IsNullOrEmpty(options.SubjectName))
            {
                filteredCertificates = filteredCertificates.Find(X509FindType.FindBySubjectName, options.SubjectName, validOnly: false);
            }

            if (!string.IsNullOrEmpty(options.Fingerprint))
            {
                filteredCertificates = filteredCertificates.Find(X509FindType.FindByThumbprint, options.Fingerprint, validOnly: false);
            }

            var certificatesCollection = new X509Certificate2Collection();

            foreach (var certificate in filteredCertificates)
            {
                if (certificate.HasEKU(Constants.CodeSigningEkuOid)
                    && certificate.HasCompatibleRsaPublicKey()
                    && (certificate.HasDigitalSignatureKeyUsage() || certificate.HasNoKeyUsage()))
                {
                    certificatesCollection.Add(certificate);
                }
            }

            return certificatesCollection;
        }

        private static X509Store GetStore(CertificateFindOptions options)
        {
            if (string.IsNullOrEmpty(options.StoreName))
            {
                if (options.IsLocalMachineStore)
                {
                    return new X509Store(StoreLocation.LocalMachine);
                }

                return new X509Store();
            }

            if (options.IsLocalMachineStore)
            {
                return new X509Store(options.StoreName, StoreLocation.LocalMachine);
            }

            return new X509Store(options.StoreName, StoreLocation.CurrentUser);
        }

        private static void OpenStore(X509Store store)
        {
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            }
            catch (CryptographicException ex)
            {
                if (ex.HResult == NativeMethods.ERROR_FILE_NOT_FOUND_HRESULT)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.CertificateStoreNotFound, store.Name));
                }

                throw;
            }
        }

        private static X509Certificate2Collection LoadCertificate(CertificateFindOptions options)
        {
            var bytes = File.ReadAllBytes(options.SecretPath);
            X509Certificate2 certificate;

            try
            {
                certificate = new X509Certificate2(bytes, options.SecretPassphrase);
            }
            catch (CryptographicException ex)
            {
                if (ex.HResult == NativeMethods.ERROR_INVALID_PASSWORD_HRESULT)
                {
                    throw new ArgumentException(Strings.InvalidCertificatePassword);
                }

                throw;
            }

            return new X509Certificate2Collection(certificate);
        }
    }
}