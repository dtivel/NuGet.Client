// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Numerics;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public class BerInteger : Integer
    {
        public BerInteger(BigInteger value)
        {
            Identifier = Asn1.Tag.Integer;
            Content = ToByteArray(value);
            Length = new byte[] { (byte)Content.Length };
            Encoding = Encoding.PrimitiveDefiniteLength;
            Class = Class.Universal;
            BigInteger = value;
        }

        protected BerInteger(byte identifier, byte[] length, byte[] content, BigInteger integer)
        {
            Assert.IsNotNullOrEmpty(length, nameof(length));
            Assert.IsNotNullOrEmpty(content, nameof(content));
            Assert.IsNotNull(integer, nameof(integer));

            Identifier = identifier;
            Class = Class.Universal;
            Length = length;
            Content = content;
            BigInteger = integer;
        }
    }
}