// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Signing.Primitives.Rfc2459
{
    /*
        // RFC 2459 section 4.2.1.5 (https://tools.ietf.org/html/rfc2459#section-4.2.1.5).
        DisplayText ::= CHOICE {
             visibleString    VisibleString  (SIZE (1..200)),
             bmpString        BMPString      (SIZE (1..200)),
             utf8String       UTF8String     (SIZE (1..200)) }
    */
    public sealed class DisplayText
    {
        public string Text { get; }
        public byte Type { get; }

        public DisplayText(string text, byte type)
        {
            Assert.IsNotNullOrEmpty(text, nameof(text));

            if (text.Length > 200)
            {
                throw new ArgumentException();
            }

            switch (type)
            {
                case Asn1Tags.VisibleString:
                case Asn1Tags.BmpString:
                case Asn1Tags.Utf8String:
                    break;

                default:
                    throw new ArgumentException();
            }

            Text = text;
            Type = type;
        }
    }
}