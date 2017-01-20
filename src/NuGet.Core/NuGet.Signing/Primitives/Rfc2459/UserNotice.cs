// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Primitives.Rfc2459
{
    /*
        // RFC 2459 section 4.2.1.5 (https://tools.ietf.org/html/rfc2459#section-4.2.1.5).
        -- user notice qualifier
        UserNotice ::= SEQUENCE {
             noticeRef        NoticeReference OPTIONAL,
             explicitText     DisplayText OPTIONAL}
    */
    public sealed class UserNotice
    {
        public NoticeReference NoticeRef { get; }
        public DisplayText ExplicitText { get; }

        public UserNotice(NoticeReference noticeRef, DisplayText explicitText)
        {
            NoticeRef = noticeRef;
            ExplicitText = explicitText;
        }
    }
}