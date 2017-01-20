// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class IntegerTests
    {
        [Fact]
        public void Read_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => Integer.Read(reader: null));
        }

        [Fact]
        public void Read_SupportsZero()
        {
            var value = Encoder.Encode(new DerInteger(0));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Integer, 1, 0x00 }, value);
        }

        [Fact]
        public void Read_SupportsLargePositiveNumber()
        {
            var value = Encoder.Encode(new DerInteger(ulong.MaxValue));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Integer, 9, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, value);
        }

        [Fact]
        public void Read_SupportsLargeNegativeNumber()
        {
            var value = Encoder.Encode(new DerInteger(long.MinValue));

            Asn1TestHelpers.AssertEqual(new byte[] { Tag.Integer, 8, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, value);
        }

        [Theory]
        [InlineData(new byte[] { Tag.Utf8String, 1, 0xAB })]
        [InlineData(new byte[] { Tag.Integer | (1 << 7), 1, 0x01 })]
        [InlineData(new byte[] { Tag.Integer, 1 })]
        public void Read_ThrowsForInvalidData(byte[] data)
        {
            Assert.Throws<InvalidDataException>(() => Decode(data));
        }

        [Theory]
        [ClassData(typeof(IntegerTestData))]
        public void Read(int expectedValue, byte[] data)
        {
            var value = Decode(data);

            Assert.IsType<DerInteger>(value);
            Assert.Equal(Tag.Integer, value.Identifier);
            Assert.Equal(Class.Universal, value.Class);
            Assert.Equal(Tag.Integer, value.Tag);
            Assert.Equal(new byte[] { data[1] }, value.Length);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, value.Encoding);

            var expectedContent = new List<byte>(data).Skip(2).ToArray();
            Asn1TestHelpers.AssertEqual(expectedContent, value.Content);
            Assert.Equal(expectedValue, value.BigInteger);
        }

        private static Integer Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, index: 0, count: bytes.Length))
            using (var reader = new BinaryReader(stream))
            {
                return Integer.Read(reader);
            }
        }
    }
}