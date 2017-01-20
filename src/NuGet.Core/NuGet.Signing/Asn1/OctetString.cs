// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public abstract class OctetString : Value
    {
        public static OctetString Read(BinaryReader reader)
        {
            Assert.IsNotNull(reader, nameof(reader));

            byte identifier;
            Class @class;
            bool isConstructed;
            byte[] length;

            ReadIdentifier(reader, Class.Universal, Asn1.Tag.OctetString, out identifier, out @class, out isConstructed);

            bool isDefiniteLength;
            int lengthInteger;

            ReadLength(reader, out isDefiniteLength, out length, out lengthInteger);

            byte[] content;

            ReadContent(reader, isDefiniteLength, lengthInteger, out content);

            if (isDefiniteLength && content.Length != lengthInteger)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, Asn1.Tag.OctetString));
            }

            if (isConstructed)
            {
                var encoding = isDefiniteLength ? Encoding.ConstructedDefiniteLength : Encoding.ConstructedIndefiniteLength;

                return new BerOctetString(identifier, encoding, length, content);
            }

            return new DerOctetString(identifier, length, content);
        }
    }
}