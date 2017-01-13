// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;

namespace NuGet.Signing
{
    /// <summary>
    /// The exception that is thrown when a certificate chain cannot be built.
    /// </summary>
    public sealed class CertificateChainBuildException : Exception
    {
        internal X509Certificate2 Certificate { get; }

        internal CertificateChainBuildException(string message, X509Certificate2 certificate)
             : base(message)
        {
            Certificate = certificate;
        }
    }
}