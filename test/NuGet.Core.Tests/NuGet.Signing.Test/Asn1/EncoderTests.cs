// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class EncoderTests
    {
        private readonly byte[] _byteContent = new byte[] { 0xAB, 0xCD };
        private readonly Oid _oidContent = new Oid("1.2.840.113549.1.9.5");
        private readonly string _utf8StringContent = "peach";
        private readonly byte[] _sequenceContent = new byte[] { 0x80, 0x05, 0x70, 0x65, 0x61, 0x63, 0x68, 0x81, 0x01, 0x11 };

        [Fact]
        public void Encode_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => Encoder.Encode(value: null));
        }

        [Theory]
        [ClassData(typeof(IntegerTestData))]
        public void Encode_DerInteger(int value, byte[] expectedResult)
        {
            var actualResult = Encoder.Encode(new DerInteger(value));

            Asn1TestHelpers.AssertEqual(expectedResult, actualResult);
        }

        [Theory]
        [ClassData(typeof(IntegerTestData))]
        public void Encode_BerInteger_PrimitiveDefiniteLength(int value, byte[] expectedResult)
        {
            var actualResult = Encoder.Encode(new BerInteger(value));

            Asn1TestHelpers.AssertEqual(expectedResult, actualResult);
        }

        [Fact]
        public void Encode_DerOctetString()
        {
            var actualResult = Encoder.Encode(new DerOctetString(_byteContent));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.OctetString, 2, 0xAB, 0xCD }, actualResult);
        }

        [Fact]
        public void Encode_BerOctetString_PrimitiveDefiniteLength()
        {
            var actualResult = Encoder.Encode(new BerOctetString(_byteContent, Encoding.PrimitiveDefiniteLength));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.OctetString, 2, 0xAB, 0xCD }, actualResult);
        }

        [Fact]
        public void Encode_BerOctetString_ConstructedDefiniteLength()
        {
            var actualResult = Encoder.Encode(new BerOctetString(_byteContent, Encoding.ConstructedDefiniteLength));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.OctetString | (1 << 5), 2, 0xAB, 0xCD }, actualResult);
        }

        [Fact]
        public void Encode_BerOctetString_ConstructedIndefiniteLength()
        {
            var actualResult = Encoder.Encode(new BerOctetString(_byteContent, Encoding.ConstructedIndefiniteLength));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.OctetString | (1 << 5), 0x80, 0xAB, 0xCD, 0, 0 }, actualResult);
        }

        [Fact]
        public void Encode_DerObjectIdentifer()
        {
            var actualResult = Encoder.Encode(new DerObjectIdentifier(_oidContent));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.ObjectIdentifier, 9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x05 }, actualResult);
        }

        [Fact]
        public void Encode_BerObjectIdentifer()
        {
            var actualResult = Encoder.Encode(new BerObjectIdentifier(_oidContent));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.ObjectIdentifier, 9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x05 }, actualResult);
        }

        [Fact]
        public void Encode_DerUtf8String()
        {
            var actualResult = Encoder.Encode(new DerUtf8String(_utf8StringContent));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Utf8String, 5, 0x70, 0x65, 0x61, 0x63, 0x68 }, actualResult);
        }

        [Fact]
        public void Encode_BerUtf8String_PrimitiveDefiniteLength()
        {
            var actualResult = Encoder.Encode(new BerUtf8String(_utf8StringContent, Encoding.PrimitiveDefiniteLength));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Utf8String, 5, 0x70, 0x65, 0x61, 0x63, 0x68 }, actualResult);
        }

        [Fact]
        public void Encode_BerUtf8String_ConstructedDefiniteLength()
        {
            var actualResult = Encoder.Encode(new BerUtf8String(_utf8StringContent, Encoding.ConstructedDefiniteLength));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Utf8String | (1 << 5), 5, 0x70, 0x65, 0x61, 0x63, 0x68 }, actualResult);
        }

        [Fact]
        public void Encode_BerUtf8String_ConstructedIndefiniteLength()
        {
            var actualResult = Encoder.Encode(new BerUtf8String(_utf8StringContent, Encoding.ConstructedIndefiniteLength));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Utf8String | (1 << 5), 0x80, 0x70, 0x65, 0x61, 0x63, 0x68, 0x00, 0x00 }, actualResult);
        }

        [Fact]
        public void Encode_DerSequence()
        {
            var elements = new Value[] { new DerUtf8String("peach"), new DerInteger(1999) };
            var actualResult = Encoder.Encode(new DerSequence(elements));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Sequence | (1 << 5), 11, 0x0C, 0x05, 0x70, 0x65, 0x61, 0x63, 0x68, 0x02, 0x02, 0x07, 0xCF }, actualResult);
        }

        [Fact]
        public void Encode_BerSequence()
        {
            var elements = new Value[] { new DerUtf8String("peach"), new DerInteger(1999) };
            var actualResult = Encoder.Encode(new BerSequence(elements));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Sequence | (1 << 5), 11, 0x0C, 0x05, 0x70, 0x65, 0x61, 0x63, 0x68, 0x02, 0x02, 0x07, 0xCF }, actualResult);
        }

        [Fact]
        public void Visit_ThrowsForNull()
        {
            var encoder = new Encoder(MemoryStream.Null);

            Assert.Throws<ArgumentNullException>(() => encoder.Visit(value: null));
        }

        [Fact]
        public void Visit()
        {
            using (var stream = new MemoryStream())
            {
                var value = new DerUtf8String(_utf8StringContent);
                var encoder = new Encoder(stream);

                encoder.Visit(value);

                var actualResult = stream.ToArray();

                Asn1TestHelpers.AssertEqual(new byte[] { Tag.Utf8String, 5, 0x70, 0x65, 0x61, 0x63, 0x68 }, actualResult);
            }
        }
    }
}