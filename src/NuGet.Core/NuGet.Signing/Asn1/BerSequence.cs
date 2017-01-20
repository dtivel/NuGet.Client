// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace NuGet.Signing.Asn1
{
    // This is public only to facilitate testing.
    public class BerSequence : Sequence
    {
        public BerSequence(IEnumerable<Value> elements)
        {
            Assert.IsNotNull(elements, nameof(elements));

            Identifier = MakeConstructed(Asn1.Tag.Sequence);
            Encoding = Encoding.ConstructedDefiniteLength;
            Class = Class.Universal;
            Content = GetContent(elements);
            Length = GetLengthInBytes(Content.Length);
        }

        protected BerSequence(byte identifier, bool isDefiniteLength, byte[] length, byte[] content)
        {
            Identifier = identifier;
            Class = Class.Universal;
            Length = length;
            Content = content;
            Encoding = isDefiniteLength ? Encoding.ConstructedDefiniteLength : Encoding.ConstructedIndefiniteLength;
        }

        private static byte[] GetContent(IEnumerable<Value> elements)
        {
            using (var stream = new MemoryStream())
            {
                var encoder = new Encoder(stream);

                foreach (var value in elements)
                {
                    encoder.Visit(value);
                }

                return stream.ToArray();
            }
        }
    }
}