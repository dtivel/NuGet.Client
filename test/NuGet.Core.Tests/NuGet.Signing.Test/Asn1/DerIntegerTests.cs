// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using NuGet.Signing.Asn1;
using Xunit;

namespace NuGet.Signing.Test
{
    public class DerIntegerTests
    {
        [Theory]
        [ClassData(typeof(IntegerTestData))]
        public void Constructor_InitializesMembers(int value, byte[] expectedResult)
        {
            var integer = new DerInteger(value);

            Assert.Equal(Tag.Integer, integer.Identifier);
            Assert.Equal(Class.Universal, integer.Class);
            Assert.Equal(Encoding.PrimitiveDefiniteLength, integer.Encoding);
            Assert.Equal(Tag.Integer, integer.Tag);
            Assert.Equal(new[] { expectedResult[1] }, integer.Length);

            var expectedContent = new List<byte>(expectedResult).Skip(2).ToArray();
            Asn1TestHelpers.AssertEqual(expectedContent, integer.Content);
            Assert.Equal(value, integer.BigInteger);
        }
    }
}