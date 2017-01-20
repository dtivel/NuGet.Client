// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Signing.Primitives.Rfc5280
{
    /*
        // RFC 5280 section 4.2.1.6 (https://tools.ietf.org/html/rfc5280#section-4.2.1.6)
        DirectoryString ::= CHOICE {
             teletexString           TeletexString (SIZE (1..MAX)),
             printableString         PrintableString (SIZE (1..MAX)),
             universalString         UniversalString (SIZE (1..MAX)),
             utf8String              UTF8String (SIZE (1..MAX)),
             bmpString               BMPString (SIZE (1..MAX)) }
    */
    public sealed class DirectoryString
    {
        public string Text { get; }
        public byte Type { get; }

        public DirectoryString(string text, byte type)
        {
            Assert.IsNotNullOrEmpty(text, nameof(text));

            switch (type)
            {
                case Asn1Tags.TeletexString:
                case Asn1Tags.PrintableString:
                case Asn1Tags.UniversalString:
                case Asn1Tags.Utf8String:
                case Asn1Tags.BmpString:
                    break;

                default:
                    throw new ArgumentException();
            }

            Text = text;
            Type = type;
        }
    }
}