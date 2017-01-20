// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Cryptography;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public class BerObjectIdentifier : ObjectIdentifier
    {
        public BerObjectIdentifier(Oid value)
        {
            Assert.IsNotNull(value, nameof(value));

            byte[] length;
            byte[] content;

            ReadOid(value.Value, out length, out content);

            Identifier = Asn1.Tag.ObjectIdentifier;
            Length = length;
            Content = content;
            Encoding = Encoding.PrimitiveDefiniteLength;
            Class = Class.Universal;
            Oid = value;
        }

        protected BerObjectIdentifier(byte identifier, byte[] length, byte[] content, Oid oid)
        {
            Assert.IsNotNullOrEmpty(length, nameof(length));
            Assert.IsNotNullOrEmpty(content, nameof(content));
            Assert.IsNotNull(oid, nameof(oid));

            Identifier = identifier;
            Class = Class.Universal;
            Length = length;
            Content = content;
            Encoding = Encoding.PrimitiveDefiniteLength;
            Oid = oid;
        }

        private static void ReadOid(string oid, out byte[] length, out byte[] content)
        {
            var segments = oid
                .Split('.')
                .Select(s => int.Parse(s))
                .ToList();

            var firstOctet = (byte)((40 * segments[0]) + segments[1]);
            var oidBytes = segments.Skip(2).SelectMany(segment =>
            {
                var digits = GenerateBaseNDigits(segment, @base: 128);

                for (var i = 0; i < digits.Count - 1; i++)
                {
                    digits[i] = (byte)(digits[i] | 0x80); // Set first bit to 1 to indicate more digits are coming
                }

                return digits;
            }).ToList();

            oidBytes.Insert(index: 0, item: firstOctet);

            content = oidBytes.ToArray();
            length = GetLengthInBytes(content.Length);
        }
    }
}