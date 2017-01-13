// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Security.Cryptography.X509Certificates;
using NuGet.Test.Utility;

namespace NuGet.Signing.Test
{
    internal static class TestData
    {
        internal static void CopyResourceToFile(string resourceName, string filePath)
        {
            var bytes = GetResourceBytes(resourceName);

            File.WriteAllBytes(filePath, bytes);
        }

        internal static X509Certificate2 GetCertificate(string certificateName)
        {
            var bytes = GetResourceBytes(certificateName);

            return new X509Certificate2(bytes);
        }

        internal static byte[] GetResourceBytes(string resourceName)
        {
            return ResourceTestUtility.GetResourceBytes($"NuGet.Signing.Test.compiler.resources.{resourceName}", typeof(TestData));
        }
    }
}