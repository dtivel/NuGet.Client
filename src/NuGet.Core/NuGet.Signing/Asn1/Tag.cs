// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public static class Tag
    {
        public const byte Integer = 0x02;
        public const byte OctetString = 0x04;
        public const byte ObjectIdentifier = 0x06;
        public const byte Utf8String = 0x0C;
        public const byte Sequence = 0x10;
    }
}