// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public sealed class DerUtf8String : BerUtf8String
    {
        public DerUtf8String(string value)
            : base(value, Encoding.PrimitiveDefiniteLength)
        {
        }

        internal DerUtf8String(byte identifier, byte[] length, byte[] content, string value)
            : base(identifier, Encoding.PrimitiveDefiniteLength, length, content, value)
        {
        }
    }
}