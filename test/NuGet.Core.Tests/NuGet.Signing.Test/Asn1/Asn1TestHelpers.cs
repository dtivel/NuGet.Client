// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace NuGet.Signing.Test
{
    internal static class Asn1TestHelpers
    {
        internal static void AssertEqual(byte[] expected, byte[] actual)
        {
            var expectedString = BitConverter.ToString(expected).Replace("-", " ");
            var actualString = BitConverter.ToString(actual).Replace("-", " ");

            Assert.Equal(expectedString, actualString);
        }
    }
}