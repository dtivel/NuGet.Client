// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Primitives
{
    internal static class Asn1Tags
    {
        internal const byte Integer = 0x02;
        internal const byte OctetString = 0x04;
        internal const byte ObjectIdentifier = 0x06;
        internal const byte Utf8String = 0x0C;
        internal const byte PrintableString = 0x13;
        internal const byte T61String = 0x14;
        internal const byte TeletexString = T61String;
        internal const byte Ia5String = 0x16;
        internal const byte VisibleString = 0x1A;
        internal const byte UniversalString = 0x1C;
        internal const byte BmpString = 0x1E;
        internal const byte Sequence = 0x30;
    }
}