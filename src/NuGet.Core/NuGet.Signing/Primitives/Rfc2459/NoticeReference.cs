// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace NuGet.Signing.Primitives.Rfc2459
{
    /*
        // RFC 2459 section 4.2.1.5 (https://tools.ietf.org/html/rfc2459#section-4.2.1.5).
        NoticeReference ::= SEQUENCE {
             organization     DisplayText,
             noticeNumbers    SEQUENCE OF INTEGER }
    */
    public sealed class NoticeReference
    {
        public DisplayText Organization { get; }
        public IEnumerable<int> NoticeNumbers { get; }

        public NoticeReference(DisplayText organization, IEnumerable<int> noticeNumbers)
        {
            Assert.IsNotNull(organization, nameof(organization));
            Assert.IsNotNull(noticeNumbers, nameof(noticeNumbers));

            Organization = organization;
            NoticeNumbers = noticeNumbers.ToList().AsReadOnly();
        }
    }
}