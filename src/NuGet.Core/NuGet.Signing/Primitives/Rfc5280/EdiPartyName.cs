// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Primitives.Rfc5280
{
    /*
        // RFC 5280 section 4.2.1.6 (https://tools.ietf.org/html/rfc5280#section-4.2.1.6)
        EDIPartyName ::= SEQUENCE {
            nameAssigner            [0]     DirectoryString OPTIONAL,
            partyName               [1]     DirectoryString }
    */
    public sealed class EdiPartyName
    {
        public DirectoryString NameAssigner { get; }
        public DirectoryString PartyName { get; }

        public EdiPartyName(DirectoryString nameAssigner, DirectoryString partyName)
        {
            Assert.IsNotNull(partyName, nameof(partyName));

            NameAssigner = nameAssigner;
            PartyName = partyName;
        }
    }
}