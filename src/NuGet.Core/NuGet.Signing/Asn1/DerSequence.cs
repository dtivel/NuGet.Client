// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public sealed class DerSequence : BerSequence
    {
        public DerSequence(IEnumerable<Value> elements)
            : base(elements)
        {
        }

        internal DerSequence(byte identifier, bool isDefiniteLength, byte[] length, byte[] content)
            : base(identifier, isDefiniteLength, length, content)
        {
        }
    }
}