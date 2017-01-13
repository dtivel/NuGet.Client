// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NuGet.Signing
{
    internal static class CryptoExtensions
    {
        internal static bool HasEKU(this X509Certificate2 certificate, string requiredEku)
        {
            return certificate.Extensions
                .OfType<X509EnhancedKeyUsageExtension>()
                .SelectMany(ext => ext.EnhancedKeyUsages.Cast<Oid>())
                .Any(eku => string.Equals(eku.Value, requiredEku, StringComparison.OrdinalIgnoreCase));
        }

        internal static bool HasDigitalSignatureKeyUsage(this X509Certificate2 certificate)
        {
            return certificate.Extensions
                .OfType<X509KeyUsageExtension>()
                .Any(ext => ext.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature));
        }

        internal static bool HasNoKeyUsage(this X509Certificate2 certificate)
        {
            return !certificate.Extensions
                .OfType<X509KeyUsageExtension>()
                .Any();
        }

        internal static bool HasCompatibleRsaPublicKey(this X509Certificate2 certificate)
        {
            var publicKey = certificate.GetRSAPublicKey();

            return publicKey != null
                && publicKey.KeySize >= 2048;
        }
    }
}