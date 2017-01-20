// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet.Signing.Primitives.Rfc5280;

namespace NuGet.Signing.Primitives.Rfc2634
{
    /*
        // RFC 2634 section 5.4.1 (https://tools.ietf.org/html/rfc2634#section-5.4.1).
        IssuerSerial ::= SEQUENCE {
            issuer                   GeneralNames,
            serialNumber             CertificateSerialNumber
        }

        // RFC 5280 section 4.1 (https://tools.ietf.org/html/rfc5280#section-4.1).
        CertificateSerialNumber  ::=  INTEGER
    */
    public sealed class IssuerSerial
    {
        public GeneralNames Issuer { get; }
        public byte[] SerialNumber { get; }

        public IssuerSerial(GeneralNames issuer, byte[] serialNumber)
        {
            Assert.IsNotNull(issuer, nameof(issuer));
            Assert.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            Issuer = issuer;
            SerialNumber = serialNumber;
        }
    }
}