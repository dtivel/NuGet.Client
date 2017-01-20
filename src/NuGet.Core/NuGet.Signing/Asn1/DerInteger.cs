// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Numerics;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public sealed class DerInteger : BerInteger
    {
        public DerInteger(BigInteger value)
            : base(value)
        {
        }

        internal DerInteger(byte identifier, byte[] length, byte[] content, BigInteger integer)
            : base(identifier, length, content, integer)
        {
        }
    }
}