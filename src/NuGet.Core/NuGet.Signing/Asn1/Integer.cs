// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public abstract class Integer : Value
    {
        public BigInteger BigInteger { get; protected set; }

        public static Integer Read(BinaryReader reader)
        {
            Assert.IsNotNull(reader, nameof(reader));

            byte identifier;
            Class @class;
            bool isConstructed;
            byte[] length;

            ReadIdentifier(reader, Class.Universal, Asn1.Tag.Integer, out identifier, out @class, out isConstructed);

            if (isConstructed)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.Integer));
            }

            bool isDefiniteLength;
            int lengthInteger;

            ReadLength(reader, out isDefiniteLength, out length, out lengthInteger);

            byte[] content;

            ReadContent(reader, isDefiniteLength, lengthInteger, out content);

            if (content.Length != lengthInteger)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.Integer));
            }

            var integer = ToBigInteger(content);

            return new DerInteger(identifier, length, content, integer);
        }

        protected static byte[] ToByteArray(BigInteger value)
        {
            var bytes = value.ToByteArray();

            // Convert little endian to big endian.
            Array.Reverse(bytes);

            return bytes;
        }

        protected static BigInteger ToBigInteger(byte[] bytes)
        {
            var copy = new byte[bytes.Length];

            Array.Copy(bytes, copy, bytes.Length);

            // Convert big endian to little endian.
            Array.Reverse(copy);

            return new BigInteger(copy);
        }
    }
}