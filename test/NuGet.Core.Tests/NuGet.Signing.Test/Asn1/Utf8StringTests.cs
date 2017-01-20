// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class Utf8StringTests
    {
        private const string _string = "peach";
        private readonly byte[] _content = System.Text.Encoding.UTF8.GetBytes(_string);

        [Fact]
        public void Read_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => Utf8String.Read(reader: null));
        }

        [Theory]
        [InlineData(new byte[] { Tag.Integer, 1, 0x01 })]
        [InlineData(new byte[] { Tag.Utf8String | (1 << 7), 5, 0x70, 0x65, 0x61, 0x63, 0x68 })]
        [InlineData(new byte[] { Tag.Utf8String, 1 })]
        public void Read_ThrowsForInvalidData(byte[] data)
        {
            Assert.Throws<InvalidDataException>(() => Decode(data));
        }

        [Fact]
        public void Read_Der()
        {
            var bytes = new byte[] { Tag.Utf8String, 5, 0x70, 0x65, 0x61, 0x63, 0x68 };
            var value = Decode(bytes);

            Assert.IsType<DerUtf8String>(value);
            Assert.Equal(Tag.Utf8String, value.Identifier);
            Assert.Equal(Class.Universal, value.Class);
            Assert.Equal(Tag.Utf8String, value.Tag);
            Assert.Equal(new byte[] { (byte)_content.Length }, value.Length);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, value.Encoding);
            Asn1TestHelpers.AssertEqual(_content, value.Content);
            Assert.Equal(_string, value.String);
        }

        [Fact]
        public void Read_Ber_ConstructedDefiniteLength()
        {
            var bytes = new byte[] { Tag.Utf8String | (1 << 5), 5, 0x70, 0x65, 0x61, 0x63, 0x68 };
            var value = Decode(bytes);

            Assert.IsType<BerUtf8String>(value);
            Assert.Equal(Tag.Utf8String | (1 << 5), value.Identifier);
            Assert.Equal(Class.Universal, value.Class);
            Assert.Equal(Tag.Utf8String, value.Tag);
            Assert.Equal(new byte[] { (byte)_content.Length }, value.Length);
            Assert.Equal(Encoding.ConstructedDefiniteLength, value.Encoding);
            Asn1TestHelpers.AssertEqual(_content, value.Content);
            Assert.Equal(_string, value.String);
        }

        [Fact]
        public void Read_Ber_ConstructedIndefiniteLength()
        {
            var bytes = new byte[] { Tag.Utf8String | (1 << 5), 0x80, 0x70, 0x65, 0x61, 0x63, 0x68, 0, 0 };
            var value = Decode(bytes);

            Assert.IsType<BerUtf8String>(value);
            Assert.Equal(Tag.Utf8String | (1 << 5), value.Identifier);
            Assert.Equal(Class.Universal, value.Class);
            Assert.Equal(Tag.Utf8String, value.Tag);
            Assert.Equal(new byte[] { 0x80 }, value.Length);
            Assert.Equal(Encoding.ConstructedIndefiniteLength, value.Encoding);
            Asn1TestHelpers.AssertEqual(_content, value.Content);
            Assert.Equal(_string, value.String);
        }

        private static Utf8String Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, index: 0, count: bytes.Length))
            using (var reader = new BinaryReader(stream))
            {
                return Utf8String.Read(reader);
            }
        }
    }
}