// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public class BerOctetString : OctetString
    {
        public BerOctetString(byte[] value, Encoding encoding)
        {
            Assert.IsNotNull(value, nameof(value));

            switch (encoding)
            {
                case Encoding.PrimitiveDefiniteLength:
                    Identifier = Asn1.Tag.OctetString;
                    Length = GetLengthInBytes(value.Length);
                    Content = value;
                    break;

                case Encoding.ConstructedDefiniteLength:
                    Identifier = MakeConstructed(Asn1.Tag.OctetString);
                    Length = GetLengthInBytes(value.Length);
                    Content = value;
                    break;

                case Encoding.ConstructedIndefiniteLength:
                    Identifier = MakeConstructed(Asn1.Tag.OctetString);
                    Length = new byte[] { 0x80 };
                    Content = AppendEndOfContentsOctets(value);
                    break;

                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.OctetString), nameof(encoding));
            }

            Encoding = encoding;
            Class = Class.Universal;
        }

        internal BerOctetString(byte identifier, Encoding encoding, byte[] length, byte[] content)
        {
            Identifier = identifier;
            Class = Class.Universal;
            Length = length;
            Content = content;
            Encoding = encoding;
        }
    }
}