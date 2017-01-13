// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing
{
    internal static class PemConstants
    {
        internal const string FileSignature = "FILE SIGNATURE";
        internal const string FileSigningRequest = "FILE SIGNING REQUEST";

        internal const string LabelChar = @"\x21-\x2C\x2E-\x60\x7B-\x7E";
        internal const string Base64Char = @"A-Za-z0-9\+/";
        internal const string Whitespace = @"\x09 ";
    }
}