// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace NuGet.Signing
{
    /// <summary>
    /// The exception that is thrown when a certificate is determined to be invalid for signing.
    /// </summary>
    public sealed class InvalidSigningCertificateException : Exception
    {
        /// <summary>
        /// A read-only collection of elements in the certificate chain.
        /// </summary>
        public ReadOnlyCollection<X509ChainElement> ChainElements { get; }

        /// <summary>
        /// A read-only collection of statuses for the overall certificate chain.
        /// </summary>
        public ReadOnlyCollection<X509ChainStatus> ChainStatus { get; }

        internal InvalidSigningCertificateException(
            string message,
            X509ChainStatus[] chainStatus,
            X509ChainElementCollection chainElements) : base(message)
        {
            ChainStatus = chainStatus.ToList().AsReadOnly();
            ChainElements = chainElements.OfType<X509ChainElement>().ToList().AsReadOnly();
        }
    }
}