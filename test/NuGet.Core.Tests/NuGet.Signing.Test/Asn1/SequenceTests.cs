// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class SequenceTests
    {
        [Fact]
        public void Read_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => Sequence.Read(reader: null));
        }

        [Theory]
        [InlineData(new byte[] { Tag.Integer, 1, 0x01 })]
        [InlineData(new byte[] { Tag.Sequence | 0xA0, 5, 0x70, 0x65, 0x61, 0x63, 0x68 })]
        [InlineData(new byte[] { Tag.Sequence, 1 })]
        public void Read_ThrowsForInvalidData(byte[] data)
        {
            Assert.Throws<InvalidDataException>(() => Decode(data));
        }

        [Fact]
        public void Read()
        {
            var bytes = new byte[] { Tag.Sequence | (1 << 5), 0x0A, 0x80, 0x05, 0x70, 0x65, 0x61, 0x63, 0x68, 0x81, 0x01, 0x11 };
            var value = Decode(bytes);

            Assert.IsType<DerSequence>(value);
            Assert.Equal(0x30, value.Identifier);
            Assert.Equal(Tag.Sequence, value.Tag);
            Assert.Equal(new byte[] { 10 }, value.Length);
            Assert.Equal(Encoding.ConstructedDefiniteLength, value.Encoding);
            Asn1TestHelpers.AssertEqual(new byte[] { 0x80, 0x05, 0x70, 0x65, 0x61, 0x63, 0x68, 0x81, 0x01, 0x11 }, value.Content);
        }

        private static Sequence Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, index: 0, count: bytes.Length))
            using (var reader = new BinaryReader(stream))
            {
                return Sequence.Read(reader);
            }
        }
    }
}