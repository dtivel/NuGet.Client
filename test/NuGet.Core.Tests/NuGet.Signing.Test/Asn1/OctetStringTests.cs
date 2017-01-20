// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class OctetStringTests
    {
        private readonly byte[] _content = new byte[] { 0xAB, 0xCD };

        [Fact]
        public void Read_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => OctetString.Read(reader: null));
        }

        [Theory]
        [InlineData(new byte[] { Tag.Utf8String, 1, 0xAB })]
        [InlineData(new byte[] { Tag.OctetString | (1 << 7), 6, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D })]
        [InlineData(new byte[] { Tag.OctetString, 1 })]
        public void Read_ThrowsForInvalidData(byte[] data)
        {
            Assert.Throws<InvalidDataException>(() => Decode(data));
        }

        [Fact]
        public void Read_Der()
        {
            var bytes = new byte[] { Tag.OctetString, 2, 0xAB, 0xCD };
            var value = Decode(bytes);

            Assert.IsType<DerOctetString>(value);
            Assert.Equal(Tag.OctetString, value.Identifier);
            Assert.Equal(Class.Universal, value.Class);
            Assert.Equal(Tag.OctetString, value.Tag);
            Assert.Equal(new byte[] { (byte)_content.Length }, value.Length);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, value.Encoding);
            Asn1TestHelpers.AssertEqual(_content, value.Content);
        }

        [Fact]
        public void Read_Ber_ConstructedDefiniteLength()
        {
            var bytes = new byte[] { Tag.OctetString | (1 << 5), 2, 0xAB, 0xCD };
            var value = Decode(bytes);

            Assert.IsType<BerOctetString>(value);
            Assert.Equal(Tag.OctetString | (1 << 5), value.Identifier);
            Assert.Equal(Class.Universal, value.Class);
            Assert.Equal(Tag.OctetString, value.Tag);
            Assert.Equal(new byte[] { (byte)_content.Length }, value.Length);
            Assert.Equal(Encoding.ConstructedDefiniteLength, value.Encoding);
            Asn1TestHelpers.AssertEqual(_content, value.Content);
        }

        [Fact]
        public void Read_Ber_ConstructedIndefiniteLength()
        {
            var bytes = new byte[] { Tag.OctetString | (1 << 5), 0x80, 0xAB, 0xCD, 0, 0 };
            var value = Decode(bytes);

            Assert.IsType<BerOctetString>(value);
            Assert.Equal(Tag.OctetString | (1 << 5), value.Identifier);
            Assert.Equal(Class.Universal, value.Class);
            Assert.Equal(Tag.OctetString, value.Tag);
            Assert.Equal(new byte[] { 0x80 }, value.Length);
            Assert.Equal(Encoding.ConstructedIndefiniteLength, value.Encoding);
            Asn1TestHelpers.AssertEqual(_content, value.Content);
        }

        private static OctetString Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, index: 0, count: bytes.Length))
            using (var reader = new BinaryReader(stream))
            {
                return OctetString.Read(reader);
            }
        }
    }
}