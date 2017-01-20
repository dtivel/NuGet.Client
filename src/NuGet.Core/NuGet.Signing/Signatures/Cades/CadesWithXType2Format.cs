﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography.X509Certificates;

namespace NuGet.Signing.Signatures.Cades
{
    /// <summary>
    /// Digital signature that conforms to the CAdES EXtended Electronic Signature
    /// with Time Type 2 (CAdES-X Type 2) format.
    /// </summary>
    /// <remarks>
    /// See ETSI TS 101 733 V2.2.1 (2013-04), section 4.4.3.3 for details.
    /// </remarks>
    internal class CadesWithXType2Format : CadesWithCFormat
    {
        internal CadesWithXType2Format(X509Certificate2 certificate, byte[] messageContent)
            : base(certificate, messageContent)
        {
        }
    }
}