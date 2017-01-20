// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public sealed class DerObjectIdentifier : BerObjectIdentifier
    {
        public DerObjectIdentifier(Oid value)
            : base(value)
        {
        }

        internal DerObjectIdentifier(byte identifier, byte[] length, byte[] content, Oid oid)
            : base(identifier, length, content, oid)
        {
        }
    }
}