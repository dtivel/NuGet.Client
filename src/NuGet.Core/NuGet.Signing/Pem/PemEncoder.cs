// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Signing
{
    /// <summary>
    /// Encodes data with PEM encoding.
    /// </summary>
    public static class PemEncoder
    {
        /// <summary>
        /// PEM-encodes a signature and writes the result to the provided writer.
        /// </summary>
        /// <param name="signature">A signature.</param>
        /// <param name="writer">A text writer.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous encode operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="signature" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer" /> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
        public static async Task EncodeAsync(Signature signature, TextWriter writer, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(signature, nameof(signature));
            Assert.IsNotNull(writer, nameof(writer));

            cancellationToken.ThrowIfCancellationRequested();

            var data = signature.Encode();
            var pemData = PemData.Create(data, PemConstants.FileSignature);

            await writer.WriteLineAsync();
            await pemData.WriteAsync(writer, cancellationToken);
        }
    }
}