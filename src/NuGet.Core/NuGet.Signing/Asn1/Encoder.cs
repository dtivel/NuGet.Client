// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public sealed class Encoder : IVisitor
    {
        private readonly BinaryWriter _writer;

        public Encoder(Stream stream)
        {
            Assert.IsNotNull(stream, nameof(stream));

            _writer = new BinaryWriter(stream);
        }

        public void Visit(Value value)
        {
            Assert.IsNotNull(value, nameof(value));

            _writer.Write(value.Identifier);
            _writer.Write(value.Length);
            _writer.Write(value.Content);
            _writer.Flush();
        }

        public static byte[] Encode(Value value)
        {
            Assert.IsNotNull(value, nameof(value));

            using (var stream = new MemoryStream())
            {
                var encoder = new Encoder(stream);

                encoder.Visit(value);

                return stream.ToArray();
            }
        }
    }
}