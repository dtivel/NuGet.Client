// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Primitives.Rfc2459
{
    /*
        // RFC 2459 section 4.2.1.5 (https://tools.ietf.org/html/rfc2459#section-4.2.1.5).
        PolicyQualifierInfo ::= SEQUENCE {
            policyQualifierId  PolicyQualifierId,
            qualifier          ANY DEFINED BY policyQualifierId }

        -- policyQualifierIds for Internet policy qualifiers

        id-qt          OBJECT IDENTIFIER ::=  { id-pkix 2 }
        id-qt-cps      OBJECT IDENTIFIER ::=  { id-qt 1 }
        id-qt-unotice  OBJECT IDENTIFIER ::=  { id-qt 2 }

        -- Implementations that recognize additional policy qualifiers shall
        -- augment the following definition for PolicyQualifierId

        PolicyQualifierId ::=
            OBJECT IDENTIFIER ( id-qt-cps | id-qt-unotice )

        -- CPS pointer qualifier
        CPSuri ::= IA5String
    */
    public sealed class PolicyQualifierInfo
    {
        private const string QtOid = "1.3.6.1.5.5.7.2";
        public const string QtCpsOid = "1.3.6.1.5.5.7.2.1";
        public const string QtUnoticeOid = "1.3.6.1.5.5.7.2.2";

        public string PolicyQualifierId { get; }
        public string CpsQualifier { get; }
        public UserNotice UserNoticeQualifier { get; }

        public PolicyQualifierInfo(string cpsUri)
        {
            Assert.IsNotNullOrEmpty(cpsUri, nameof(cpsUri));

            PolicyQualifierId = QtCpsOid;
            CpsQualifier = cpsUri;
        }

        public PolicyQualifierInfo(UserNotice userNotice)
        {
            Assert.IsNotNull(userNotice, nameof(userNotice));

            PolicyQualifierId = QtUnoticeOid;
            UserNoticeQualifier = userNotice;
        }
    }
}