// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public class BerUtf8String : Utf8String
    {
        public BerUtf8String(string value, Encoding encoding)
        {
            Assert.IsNotNull(value, nameof(value));

            switch (encoding)
            {
                case Encoding.PrimitiveDefiniteLength:
                    Identifier = Asn1.Tag.Utf8String;
                    Content = System.Text.Encoding.UTF8.GetBytes(value);
                    Length = GetLengthInBytes(Content.Length);
                    break;

                case Encoding.ConstructedDefiniteLength:
                    Identifier = MakeConstructed(Asn1.Tag.Utf8String);
                    Content = System.Text.Encoding.UTF8.GetBytes(value);
                    Length = GetLengthInBytes(Content.Length);
                    break;

                case Encoding.ConstructedIndefiniteLength:
                    Identifier = MakeConstructed(Asn1.Tag.Utf8String);
                    Length = new byte[] { 0x80 };
                    Content = AppendEndOfContentsOctets(System.Text.Encoding.UTF8.GetBytes(value));
                    break;

                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.Utf8String), nameof(encoding));
            }

            Encoding = encoding;
            Class = Class.Universal;
            String = value;
        }

        internal BerUtf8String(byte identifier, Encoding encoding, byte[] length, byte[] content, string @string)
        {
            Identifier = identifier;
            Class = Class.Universal;
            Length = length;
            Content = content;
            String = @string;
            Encoding = encoding;
        }
    }
}