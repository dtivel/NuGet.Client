// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class DerUtf8StringTests
    {
        [Fact]
        public void Constructor_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DerUtf8String(value: null));
        }

        [Fact]
        public void Constructor_PrimitiveDefiniteLength_InitializesMembers()
        {
            const string value = "peach";
            var content = System.Text.Encoding.UTF8.GetBytes(value);
            var utf8String = new BerUtf8String(value, Encoding.PrimitiveDefiniteLength);

            Assert.Equal(Tag.Utf8String, utf8String.Identifier);
            Assert.Equal(Class.Universal, utf8String.Class);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, utf8String.Encoding);
            Assert.Equal(Tag.Utf8String, utf8String.Tag);
            Assert.Equal(new byte[] { (byte)content.Length }, utf8String.Length);
            Asn1TestHelpers.AssertEqual(content, utf8String.Content);
            Assert.Equal(value, utf8String.String);
        }
    }
}