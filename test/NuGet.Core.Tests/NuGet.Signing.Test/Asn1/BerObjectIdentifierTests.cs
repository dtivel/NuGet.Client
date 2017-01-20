// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class BerObjectIdentifierTests
    {
        [Fact]
        public void Constructor_ThrowsForNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BerObjectIdentifier(value: null));
        }

        [Fact]
        public void Constructor_InitializesMembers()
        {
            var value = new Oid("1.2.840.113549.1.9.5");
            var oid = new BerObjectIdentifier(value);

            Assert.Equal(Tag.ObjectIdentifier, oid.Identifier);
            Assert.Equal(Class.Universal, oid.Class);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, oid.Encoding);
            Assert.Equal(Tag.ObjectIdentifier, oid.Tag);
            Assert.Equal(new byte[] { 9 }, oid.Length);

            Asn1TestHelpers.AssertEqual(new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x05 }, oid.Content);
            Assert.Equal(value.Value, oid.Oid.Value);
        }
    }
}