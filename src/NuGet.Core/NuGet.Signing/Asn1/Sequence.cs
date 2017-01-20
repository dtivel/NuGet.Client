// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public abstract class Sequence : Value
    {
        public static Sequence Read(BinaryReader reader)
        {
            Assert.IsNotNull(reader, nameof(reader));

            byte identifier;
            Class @class;
            bool isConstructed;
            byte[] length;

            ReadIdentifier(reader, Class.Universal, Asn1.Tag.Sequence, out identifier, out @class, out isConstructed);

            if (!isConstructed)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.Sequence));
            }

            bool isDefiniteLength;
            int lengthInteger;

            ReadLength(reader, out isDefiniteLength, out length, out lengthInteger);

            if (!isDefiniteLength)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.Sequence));
            }

            byte[] content;

            ReadContent(reader, isDefiniteLength, lengthInteger, out content);

            if (content.Length != lengthInteger)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.Sequence));
            }

            return new DerSequence(identifier, isDefiniteLength, length, content);
        }
    }
}