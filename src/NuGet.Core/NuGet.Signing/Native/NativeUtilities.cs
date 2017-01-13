﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace NuGet.Signing
{
    internal static class NativeUtilities
    {
        internal static void SafeFree(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        internal static void ThrowIfFailed(bool result)
        {
            if (!result)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }
    }
}