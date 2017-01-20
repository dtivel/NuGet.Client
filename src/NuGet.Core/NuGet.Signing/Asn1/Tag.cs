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
        public const byte PrintableString = 0x13;
        public const byte T61String = 0x14;
        public const byte TeletexString = T61String;
        public const byte Ia5String = 0x16;
        public const byte VisibleString = 0x1A;
        public const byte UniversalString = 0x1C;
        public const byte BmpString = 0x1E;
    }
}