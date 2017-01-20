// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Primitives.Rfc5280
{
    /*
        // RFC 5280 section 4.2.1.6 (https://tools.ietf.org/html/rfc5280#section-4.2.1.6)
        OtherName ::= SEQUENCE {
            type-id    OBJECT IDENTIFIER,
            value      [0] EXPLICIT ANY DEFINED BY type-id }
    */
    public sealed class OtherName
    {
        public string TypeId { get; }
        public byte[] Value { get; }

        public OtherName(string typeId, byte[] value)
        {
            Assert.IsNotNullOrEmpty(typeId, nameof(typeId));

            TypeId = typeId;
            Value = value;
        }
    }
}