// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public abstract class Value
    {
        public byte Identifier { get; protected set; }
        public Class Class { get; protected set; }
        public byte[] Length { get; protected set; }
        public byte[] Content { get; protected set; }
        public Encoding Encoding { get; protected set; }

        public byte Tag
        {
            get { return GetTag(Identifier); }
        }

        public void Accept(IVisitor visitor)
        {
            Assert.IsNotNull(visitor, nameof(visitor));

            visitor.Visit(this);
        }

        protected static byte[] AppendEndOfContentsOctets(byte[] source)
        {
            var length = source.Length + 2;
            var newArray = new byte[length];

            source.CopyTo(newArray, 0);

            return newArray;
        }

        protected static List<byte> GenerateBaseNDigits(long value, int @base)
        {
            var digits = new List<byte>();

            while ((value > (@base - 1)) || (value < -(@base - 1)))
            {
                var digit = (int)(value % @base);
                value = value / @base;

                // Insert at the front so we "unreverse" the digits as we calculate them
                digits.Insert(0, (byte)digit);
            }

            digits.Insert(0, (byte)value);

            return digits;
        }

        protected static byte[] GetLengthInBytes(int length)
        {
            var lengthInBytes = new List<byte>();

            if (length <= 127)
            {
                lengthInBytes.Add((byte)(length & 0x7F));
            }
            else
            {
                var digits = GenerateBaseNDigits(length, @base: 256);

                // Write the number of length digits
                lengthInBytes.Add((byte)(digits.Count | 0x80));

                // Write the length digits
                lengthInBytes.AddRange(digits.ToArray());
            }

            return lengthInBytes.ToArray();
        }

        protected static byte GetTag(byte identifier)
        {
            // This clears bits 6-8.
            return (byte)(identifier & 0x1F);
        }

        protected static byte MakeConstructed(byte tag)
        {
            return (byte)(tag | 0x20);
        }

        protected static byte PeekByte(BinaryReader reader)
        {
            var @byte = reader.ReadByte();

            --reader.BaseStream.Position;

            return @byte;
        }

        protected static void ReadIdentifier(BinaryReader reader, Class expectedClass, byte expectedTag, out byte identifier, out Class @class, out bool isConstructed)
        {
            identifier = PeekByte(reader);

            @class = GetClass(identifier);

            if (@class != expectedClass)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, expectedTag));
            }

            var tag = GetTag(identifier);

            if (tag != expectedTag)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidAsn1Encoding, expectedTag));
            }

            isConstructed = (identifier & (1 << 5)) != 0;

            reader.ReadByte();
        }

        protected static void ReadLength(BinaryReader reader, out bool isDefiniteLength, out byte[] length, out int lengthInteger)
        {
            var @byte = reader.ReadByte();
            var value = (byte)(@byte & 0x7F);

            if ((@byte & 0x80) == 0)
            {
                length = new byte[] { value };
                lengthInteger = value;
                isDefiniteLength = true;
            }
            else if (value != 0)
            {
                var bytes = new List<byte>() { @byte };

                // Bit 8 set and definite length value is actually the number of length octets left.
                // Each byte is a base-256 "digit".
                var lengthBytes = reader.ReadBytes(value);
                bytes.AddRange(lengthBytes);

                length = bytes.ToArray();
                lengthInteger = lengthBytes.Aggregate(
                    seed: 0,
                    func: (l, r) => (l * 256) + r);

                isDefiniteLength = true;
            }
            else
            {
                length = new byte[] { @byte };
                lengthInteger = 0;
                isDefiniteLength = false;
            }
        }

        protected static void ReadContent(BinaryReader reader, bool isDefiniteLength, int lengthInteger, out byte[] content)
        {
            if (isDefiniteLength)
            {
                content = reader.ReadBytes(lengthInteger);
            }
            else
            {
                // The end of indefinite length content is signalled by two successive null octets (0x00 0x00).
                var bytes = new List<byte>();
                var firstNullFound = false;

                while (true)
                {
                    var @byte = reader.ReadByte();

                    if (@byte == 0)
                    {
                        if (firstNullFound)
                        {
                            bytes.RemoveAt(bytes.Count - 1);
                            break;
                        }

                        firstNullFound = true;
                    }
                    else
                    {
                        firstNullFound = false;
                    }

                    bytes.Add(@byte);
                }

                content = bytes.ToArray();
            }
        }

        private static Class GetClass(byte identifier)
        {
            // Get bits 7 and 8.
            switch (identifier >> 6)
            {
                case 3:
                    return Class.Private;
                case 2:
                    return Class.ContextSpecific;
                case 1:
                    return Class.Application;
                case 0:
                default:
                    return Class.Universal;
            }
        }
    }
}