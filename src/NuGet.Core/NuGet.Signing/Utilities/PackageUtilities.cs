// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NuGet.Signing
{
    internal static class PackageUtilities
    {
        internal static PackageIdentity ExtractPackageIdentity(string filePath)
        {
            Assert.IsNotNullOrEmpty(filePath, nameof(filePath));

            using (var reader = new PackageArchiveReader(filePath))
            {
                return reader.GetIdentity();
            }
        }
    }
}