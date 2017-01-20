// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class BerOctetStringTests
    {
        [Fact]
        public void Constructor_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BerOctetString(value: null, encoding: Encoding.PrimitiveDefiniteLength));
        }

        [Fact]
        public void Constructor_ThrowsForInvalidEncoding()
        {
            Assert.Throws<ArgumentException>(() => new BerOctetString(new byte[] { 0 }, (Encoding)0xFF));
        }

        [Fact]
        public void Constructor_PrimitiveDefiniteLength_InitializesMembers()
        {
            var value = new byte[] { 0xAB, 0xCD };
            var octetString = new BerOctetString(value, Encoding.PrimitiveDefiniteLength);

            Assert.Equal(Tag.OctetString, octetString.Identifier);
            Assert.Equal(Class.Universal, octetString.Class);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, octetString.Encoding);
            Assert.Equal(Tag.OctetString, octetString.Tag);
            Assert.Equal(new byte[] { (byte)value.Length }, octetString.Length);
            Asn1TestHelpers.AssertEqual(value, octetString.Content);
        }

        [Fact]
        public void Constructor_ConstructedDefiniteLength_InitializesMembers()
        {
            var value = new byte[] { 0xAB, 0xCD };
            var octetString = new BerOctetString(value, Encoding.ConstructedDefiniteLength);

            Assert.Equal(Tag.OctetString | (1 << 5), octetString.Identifier);
            Assert.Equal(Class.Universal, octetString.Class);
            Assert.Equal(Encoding.ConstructedDefiniteLength, octetString.Encoding);
            Assert.Equal(Tag.OctetString, octetString.Tag);
            Assert.Equal(new byte[] { (byte)value.Length }, octetString.Length);
            Asn1TestHelpers.AssertEqual(value, octetString.Content);
        }

        [Fact]
        public void Constructor_ConstructedIndefiniteLength_InitializesMembers()
        {
            var value = new byte[] { 0xAB, 0xCD };
            var octetString = new BerOctetString(value, Encoding.ConstructedIndefiniteLength);

            Assert.Equal(Tag.OctetString | (1 << 5), octetString.Identifier);
            Assert.Equal(Class.Universal, octetString.Class);
            Assert.Equal(Encoding.ConstructedIndefiniteLength, octetString.Encoding);
            Assert.Equal(Tag.OctetString, octetString.Tag);
            Assert.Equal(new byte[] { 0x80 }, octetString.Length);
            Asn1TestHelpers.AssertEqual(new byte[] { 0xAB, 0xCD, 0x00, 0x00 }, octetString.Content);
        }
    }
}