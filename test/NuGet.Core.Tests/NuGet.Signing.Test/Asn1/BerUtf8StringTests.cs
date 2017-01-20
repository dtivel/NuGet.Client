// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class BerUtf8StringTests
    {
        private const string _value = "peach";
        private readonly byte[] _content = System.Text.Encoding.UTF8.GetBytes(_value);

        [Fact]
        public void Constructor_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BerUtf8String(value: null, encoding: Encoding.PrimitiveDefiniteLength));
        }

        [Fact]
        public void Constructor_ThrowsForInvalidEncoding()
        {
            Assert.Throws<ArgumentException>(() => new BerUtf8String("peach", encoding: (Encoding)0xFF));
        }

        [Fact]
        public void Constructor_PrimitiveDefiniteLength_InitializesMembers()
        {
            var utf8String = new BerUtf8String(_value, Encoding.PrimitiveDefiniteLength);

            Assert.Equal(Tag.Utf8String, utf8String.Identifier);
            Assert.Equal(Class.Universal, utf8String.Class);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, utf8String.Encoding);
            Assert.Equal(Tag.Utf8String, utf8String.Tag);
            Assert.Equal(new byte[] { (byte)_content.Length }, utf8String.Length);
            Asn1TestHelpers.AssertEqual(_content, utf8String.Content);
            Assert.Equal(_value, utf8String.String);
        }

        [Fact]
        public void Constructor_ConstructedDefiniteLength_InitializesMembers()
        {
            var utf8String = new BerUtf8String(_value, Encoding.ConstructedDefiniteLength);

            Assert.Equal(Tag.Utf8String | (1 << 5), utf8String.Identifier);
            Assert.Equal(Class.Universal, utf8String.Class);
            Assert.Equal(Encoding.ConstructedDefiniteLength, utf8String.Encoding);
            Assert.Equal(Tag.Utf8String, utf8String.Tag);
            Assert.Equal(new byte[] { (byte)_content.Length }, utf8String.Length);
            Asn1TestHelpers.AssertEqual(_content, utf8String.Content);
            Assert.Equal(_value, utf8String.String);
        }

        [Fact]
        public void Constructor_ConstructedIndefiniteLength_InitializesMembers()
        {
            var utf8String = new BerUtf8String(_value, Encoding.ConstructedIndefiniteLength);

            Assert.Equal(Tag.Utf8String | (1 << 5), utf8String.Identifier);
            Assert.Equal(Class.Universal, utf8String.Class);
            Assert.Equal(Encoding.ConstructedIndefiniteLength, utf8String.Encoding);
            Assert.Equal(Tag.Utf8String, utf8String.Tag);
            Assert.Equal(new byte[] { 0x80 }, utf8String.Length);

            var expectedContent = new byte[_content.Length + 2];

            _content.CopyTo(expectedContent, index: 0);

            Asn1TestHelpers.AssertEqual(expectedContent, utf8String.Content);
            Assert.Equal(_value, utf8String.String);
        }
    }
}