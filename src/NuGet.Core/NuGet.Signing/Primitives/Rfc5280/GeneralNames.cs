// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace NuGet.Signing.Primitives.Rfc5280
{
    /*
        // RFC 5280 section 4.2.1.6 (https://tools.ietf.org/html/rfc5280#section-4.2.1.6)
        GeneralNames ::= SEQUENCE SIZE (1..MAX) OF GeneralName
    */
    public sealed class GeneralNames
    {
        private readonly List<GeneralName> _generalNames;

        public GeneralNames(IEnumerable<GeneralName> generalNames)
        {
            Assert.IsNotNullOrEmpty(generalNames, nameof(generalNames));

            _generalNames = new List<GeneralName>(generalNames);
        }
    }
}