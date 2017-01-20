// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class DerOctetStringTests
    {
        [Fact]
        public void Constructor_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DerOctetString(value: null));
        }

        [Fact]
        public void Constructor_InitializesMembers()
        {
            var value = new byte[] { 0xAB, 0xCD };
            var octetString = new DerOctetString(value);

            Assert.Equal(Tag.OctetString, octetString.Identifier);
            Assert.Equal(Class.Universal, octetString.Class);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, octetString.Encoding);
            Assert.Equal(Tag.OctetString, octetString.Tag);
            Assert.Equal(new byte[] { (byte)value.Length }, octetString.Length);
            Asn1TestHelpers.AssertEqual(value, octetString.Content);
        }
    }
}