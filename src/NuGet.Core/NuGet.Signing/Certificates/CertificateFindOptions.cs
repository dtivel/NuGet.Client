// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security;

namespace NuGet.Signing
{
    /// <summary>
    /// Find options for X.509 certificates.
    /// <seealso cref="CertificateFinder" />
    /// </summary>
    public sealed class CertificateFindOptions : IDisposable
    {
        /// <summary>
        /// The certificate file path.
        /// </summary>
        public string SecretPath { get; set; }

        /// <summary>
        /// The certificate password.
        /// </summary>
        public SecureString SecretPassphrase { get; set; }

        /// <summary>
        /// The certificate store name.
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Flag indicating if the store indicated by the <see cref="StoreName" /> property
        /// is for the local machine (<c>true</c>) or the current user (<c>false</c>).
        /// The default is <c>false</c>.
        /// </summary>
        public bool IsLocalMachineStore { get; set; }

        /// <summary>
        /// The certificate subject name.  Can be a substring.
        /// </summary>
        public string SubjectName { get; set; }

        /// <summary>
        /// The SHA-1 hash of the certificate.
        /// </summary>
        public string Fingerprint { get; set; }

        /// <summary>
        /// Disposes of resources.
        /// </summary>
        public void Dispose()
        {
            if (SecretPassphrase != null)
            {
                SecretPassphrase.Dispose();
            }
        }
    }
}