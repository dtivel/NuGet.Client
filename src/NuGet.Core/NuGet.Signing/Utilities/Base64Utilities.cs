// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Signing
{
    internal static class Base64Utilities
    {
        internal static bool IsBase64Text(string text)
        {
            try
            {
                return Convert.FromBase64String(text) != null;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}