// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.InteropServices;

namespace NuGet.Signing.FuncTest
{
    internal static class NativeMethods
    {
        internal const int ERROR_FILE_NOT_FOUND_HRESULT = unchecked((int)0x80070002);

        private const int CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;
        private const uint CERT_SYSTEM_STORE_CURRENT_USER_ID = 1;

        internal const uint CERT_STORE_DELETE_FLAG = 0x10;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER = CERT_SYSTEM_STORE_CURRENT_USER_ID << CERT_SYSTEM_STORE_LOCATION_SHIFT;

        [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CertUnregisterSystemStore(string pvSystemStore, uint dwFlags);
    }
}