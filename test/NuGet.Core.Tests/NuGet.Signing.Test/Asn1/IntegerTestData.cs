// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet.Signing.Asn1;
using System.Collections;
using System.Collections.Generic;

namespace NuGet.Signing.Test
{
    public class IntegerTestData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] { 0, new byte[] { Tag.Integer, 1, 0x00 } },
            new object[] { 127, new byte[] { Tag.Integer, 1, 0x7F } },
            new object[] { 128, new byte[] { Tag.Integer, 2, 0x00, 0x80 } },
            new object[] { 256, new byte[] { Tag.Integer, 2, 0x01, 0x00 } },
            new object[] { -128, new byte[] { Tag.Integer, 1, 0x80 } },
            new object[] { -129, new byte[] { Tag.Integer, 2, 0xFF, 0x7F } }
        };

        public IEnumerator<object[]> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}