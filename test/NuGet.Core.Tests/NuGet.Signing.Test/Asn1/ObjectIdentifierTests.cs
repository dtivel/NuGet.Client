// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class ObjectIdentifierTests
    {
        [Fact]
        public void Read_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectIdentifier.Read(reader: null));
        }

        [Theory]
        [InlineData(new byte[] { Tag.Utf8String, 1, 0xAB })]
        [InlineData(new byte[] { Tag.ObjectIdentifier | (1 << 7), 6, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D })]
        [InlineData(new byte[] { Tag.ObjectIdentifier, 1 })]
        public void Read_ThrowsForInvalidData(byte[] data)
        {
            Assert.Throws<InvalidDataException>(() => Decode(data));
        }

        [Fact]
        public void Read()
        {
            var content = new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D };
            var bytes = new byte[] { Tag.ObjectIdentifier, 6, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D };
            var value = Decode(bytes);

            Assert.IsType<DerObjectIdentifier>(value);
            Assert.Equal(Tag.ObjectIdentifier, value.Identifier);
            Assert.Equal(Class.Universal, value.Class);
            Assert.Equal(Tag.ObjectIdentifier, value.Tag);
            Assert.Equal(new byte[] { (byte)content.Length }, value.Length);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, value.Encoding);
            Asn1TestHelpers.AssertEqual(content, value.Content);
            Assert.Equal("1.2.840.113549", value.Oid.Value);
        }

        private static ObjectIdentifier Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, index: 0, count: bytes.Length))
            using (var reader = new BinaryReader(stream))
            {
                return ObjectIdentifier.Read(reader);
            }
        }
    }
}