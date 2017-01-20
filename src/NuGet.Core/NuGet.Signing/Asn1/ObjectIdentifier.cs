// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public abstract class ObjectIdentifier : Value
    {
        public Oid Oid { get; protected set; }

        public static ObjectIdentifier Read(BinaryReader reader)
        {
            Assert.IsNotNull(reader, nameof(reader));

            byte identifier;
            Class @class;
            bool isConstructed;
            byte[] length;

            ReadIdentifier(reader, Class.Universal, Asn1.Tag.ObjectIdentifier, out identifier, out @class, out isConstructed);

            if (isConstructed)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.ObjectIdentifier));
            }

            bool isDefiniteLength;
            int lengthInteger;

            ReadLength(reader, out isDefiniteLength, out length, out lengthInteger);

            byte[] content;

            ReadContent(reader, isDefiniteLength, lengthInteger, out content);

            if (content.Length != lengthInteger)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.ObjectIdentifier));
            }

            var oid = ReadOid(content);

            return new DerObjectIdentifier(identifier, length, content, oid);
        }

        private static Oid ReadOid(byte[] octets)
        {
            if (octets.Length < 2)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.ObjectIdentifier));
            }

            var segments = new List<int>();

            segments.Add(octets[0] / 40);  // First segment
            segments.Add(octets[0] % 40);  // Second segment

            // Remaining octets are encoded as base-128 digits, where the highest bit indicates if more digits exist.
            var index = 1;

            while (index < octets.Length)
            {
                var val = 0;

                do
                {
                    val = (val * 128) + (octets[index] & 0x7F); // Take low 7 bits of octet

                    ++index;
                }
                while ((octets[index - 1] & 0x80) != 0); // Loop while high bit is 1

                segments.Add(val);
            }

            return new Oid(string.Join(".", segments.Select(s => s.ToString())));
        }
    }
}