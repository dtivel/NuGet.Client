// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGet.Signing
{
    internal static class Asn1Utilities
    {
        private const int _versionLength = 1;

        private static class Tags
        {
            internal const byte Integer = 0x02;
            internal const byte OctetString = 0x04;
            internal const byte ObjectIdentifier = 0x06;
            internal const byte Utf8String = 0x0C;
            internal const byte Sequence = 0x30;
        }

        /*

        ASN.1 structure:

            SignatureTargets ::= SEQUENCE {
              version            INTEGER { v1(1) },
              signatureTargets   SignatureTarget }

            SignatureTarget ::= SEQUENCE {
              version            INTEGER { v1(1) },
              packageId          UTF8String,
              packageVersion     UTF8String,
              contentDigest      ContentDigest }

            ContentDigest ::= SEQUENCE {
              digestAlgorithm    OBJECT IDENTIFIER
              digest             OCTET STRING }

         */
        internal static byte[] Encode(SignatureTargets signatureTargets)
        {
            Assert.IsNotNull(signatureTargets, nameof(signatureTargets));

            using (var signatureTarget = Encode(signatureTargets.SignatureTarget))
            using (var stream = new MemoryStream())
            {
                using (var writer = CreateBinaryWriter(stream))
                {
                    WriteVersion(writer, signatureTargets.Version);
                    WriteSequence(writer, signatureTarget);
                }

                using (var encodedStream = EncodeSequence(stream))
                {
                    return encodedStream.ToArray();
                }
            }
        }

        internal static SignatureTargets Decode(byte[] bytes)
        {
            Assert.IsNotNullOrEmpty(bytes, nameof(bytes));

            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                return ReadSignatureTargets(reader);
            }
        }

        private static SignatureTargets ReadSignatureTargets(BinaryReader reader)
        {
            var content = ReadSequenceContent(reader);

            return DecodeSignatureTargets(content);
        }

        private static SignatureTargets DecodeSignatureTargets(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                var version = ReadVersion(reader);
                var content = ReadSequenceContent(reader);
                var signatureTarget = DecodeSignatureTarget(content);

                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsUnexpectedDataAfterSignatureTarget);
                }

                return new SignatureTargets(version, signatureTarget);
            }
        }

        private static SignatureTarget DecodeSignatureTarget(byte[] bytes)
        {
            var content = GetSequenceContent(bytes);

            using (var stream = new MemoryStream(content))
            using (var reader = new BinaryReader(stream))
            {
                var version = ReadVersion(reader);
                var packageId = ReadUtf8String(reader);
                var packageVersion = ReadNuGetVersion(reader);
                var packageIdentity = new PackageIdentity(packageId, packageVersion);

                content = ReadSequenceContent(reader);

                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(Strings.InvalidSignatureTargetsUnexpectedDataAfterContentDigest);
                }

                var contentDigest = DecodeContentDigest(content);

                return new SignatureTarget(version, packageIdentity, contentDigest);
            }
        }

        private static ContentDigest DecodeContentDigest(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                var digestAlgorithm = ReadOid(reader);
                var digest = ReadOctetString(reader);

                return new ContentDigest(digestAlgorithm, digest);
            }
        }

        private static MemoryStream EncodeSequence(MemoryStream content)
        {
            content.Position = 0;

            var stream = new MemoryStream();

            using (var writer = CreateBinaryWriter(stream))
            {
                WriteSequence(writer, content);
            }

            return stream;
        }

        private static MemoryStream Encode(SignatureTarget signatureTarget)
        {
            using (var contentDigest = Encode(signatureTarget.ContentDigest))
            using (var stream = new MemoryStream())
            {
                using (var writer = CreateBinaryWriter(stream))
                {
                    WriteVersion(writer, signatureTarget.Version);
                    WriteUtf8String(writer, signatureTarget.PackageIdentity.Id);
                    WriteUtf8String(writer, signatureTarget.PackageIdentity.Version.ToNormalizedString());
                    CopyTo(contentDigest, writer);
                }

                return EncodeSequence(stream);
            }
        }

        private static MemoryStream Encode(ContentDigest contentDigest)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = CreateBinaryWriter(stream))
                {
                    WriteOid(writer, contentDigest.DigestAlgorithm);
                    WriteOctetString(writer, contentDigest.Digest);
                }

                return EncodeSequence(stream);
            }
        }

        private static byte[] ReadSequenceContent(BinaryReader reader)
        {
            if (reader.ReadByte() != Tags.Sequence)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargetsSequenceNotFound);
            }

            var length = ReadLength(reader);

            return reader.ReadBytes(length);
        }

        private static Oid ReadOid(BinaryReader reader)
        {
            if (reader.ReadByte() != Tags.ObjectIdentifier)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargetsObjectIdentifierNotFound);
            }

            var length = ReadLength(reader);
            var octets = reader.ReadBytes(length);

            Debug.Assert(length >= 2);

            var segments = new List<int>();

            segments.Add(octets[0] / 40); // First segment
            segments.Add(octets[0] % 40); // Second segment

            // Remaining octets are encoded as base-128 digits, where the highest bit indicates if more digits exist
            int idx = 1;
            while (idx < octets.Length)
            {
                var val = 0;

                do
                {
                    val = (val * 128) + (octets[idx] & 0x7F); // Take low 7 bits of octet
                    idx++;
                } while ((octets[idx - 1] & 0x80) != 0); // Loop while high bit is 1

                segments.Add(val);
            }

            return new Oid(string.Join(".", segments.Select(s => s.ToString())));
        }

        private static byte[] ReadOctetString(BinaryReader reader)
        {
            if (reader.ReadByte() != Tags.OctetString)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargetsOctetSequenceNotFound);
            }

            var length = ReadLength(reader);

            return reader.ReadBytes(length);
        }

        private static NuGetVersion ReadNuGetVersion(BinaryReader reader)
        {
            var value = ReadUtf8String(reader);
            NuGetVersion version;

            if (NuGetVersion.TryParse(value, out version))
            {
                return version;
            }

            throw new InvalidDataException(Strings.InvalidSignatureTargetsInvalidVersionString);
        }

        private static string ReadUtf8String(BinaryReader reader)
        {
            if (reader.ReadByte() != Tags.Utf8String)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargetsUtf8StringNotFound);
            }

            var length = ReadLength(reader);
            var content = reader.ReadBytes(length);

            return Encoding.UTF8.GetString(content);
        }

        private static int ReadVersion(BinaryReader reader)
        {
            if (reader.ReadByte() != Tags.Integer)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargetsIntegerNotFound);
            }

            if (reader.ReadByte() != _versionLength)
            {
                throw new InvalidDataException(Strings.InvalidSignatureTargetsUnexpectedVersionLength);
            }

            return reader.ReadByte();
        }

        private static int ReadLength(BinaryReader reader)
        {
            var lowLen = reader.ReadByte();
            var len = lowLen & 0x7F;

            if ((lowLen & 0x80) != 0 && len != 0) // Bit 8 set and not indeterminate length?
            {
                // Len is actually the number of length octets left, each one is a base 256 "digit"
                var lengthBytes = reader.ReadBytes(len);
                len = lengthBytes.Aggregate(
                    seed: 0,
                    func: (l, r) => (l * 256) + r);
            }

            return len;
        }

        private static void WriteOid(BinaryWriter writer, Oid digestAlgorithm)
        {
            // Parse the segments
            // (We are extremely confident that the segments are valid numbers)
            var segments = digestAlgorithm.Value
                .Split('.')
                .Select(s => int.Parse(s))
                .ToList();

            // Again, we are confident the segments list will have at least 2 items.
            Debug.Assert(segments.Count >= 2);

            // Determine the first byte of the value
            byte firstOctet = (byte)((40 * segments[0]) + segments[1]);

            // Calculate the remaining bytes
            var oidBytes = segments.Skip(2).SelectMany(segment =>
            {
                var digits = GenerateBaseNDigits(segment, @base: 128);
                for (int i = 0; i < digits.Count - 1; i++)
                {
                    digits[i] = (byte)(digits[i] | 0x80); // Set first bit to 1 to indicate more digits are coming
                }
                return digits;
            }).ToArray();

            writer.Write(Tags.ObjectIdentifier);
            WriteLength(writer, oidBytes.Length + 1);
            writer.Write(firstOctet);
            writer.Write(oidBytes);
        }

        private static void WriteOctetString(BinaryWriter writer, byte[] value)
        {
            writer.Write(Tags.OctetString);
            WriteLength(writer, value.Length);
            writer.Write(value);
        }

        private static void WriteLength(BinaryWriter writer, int length)
        {
            if (length <= 127)
            {
                writer.Write((byte)(length & 0x7F));
            }
            else
            {
                var digits = GenerateBaseNDigits(length, @base: 256);

                // Write the number of length digits
                writer.Write((byte)(digits.Count | 0x80));

                // Write the length digits
                writer.Write(digits.ToArray());
            }
        }

        private static void WriteVersion(BinaryWriter writer, int value)
        {
            writer.Write(Tags.Integer);
            writer.Write((byte)_versionLength);
            writer.Write((byte)value);
        }

        private static void WriteSequence(BinaryWriter writer, MemoryStream content)
        {
            writer.Write(Tags.Sequence);
            WriteLength(writer, (int)content.Length);
            CopyTo(content, writer);
        }

        private static void WriteUtf8String(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);

            writer.Write(Tags.Utf8String);
            WriteLength(writer, bytes.Length);
            writer.Write(bytes);
        }

        private static List<byte> GenerateBaseNDigits(long value, int @base)
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

        private static byte[] GetSequenceContent(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                return ReadSequenceContent(reader);
            }
        }

        private static BinaryWriter CreateBinaryWriter(MemoryStream stream)
        {
            // This is BinaryWriter's default encoding.
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

            return new BinaryWriter(stream, encoding, leaveOpen: true);
        }

        private static void CopyTo(MemoryStream source, BinaryWriter destination)
        {
            // Ensure all buffered data has been written.
            destination.Flush();

            // Start copying from the beginning.
            source.Position = 0;

            source.CopyTo(destination.BaseStream);
        }
    }
}