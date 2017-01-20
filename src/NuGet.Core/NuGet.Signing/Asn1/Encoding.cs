// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public enum Encoding
    {
        PrimitiveDefiniteLength,
        ConstructedDefiniteLength,
        ConstructedIndefiniteLength
    }
}