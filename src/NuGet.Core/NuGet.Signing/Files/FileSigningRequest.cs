// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Signing
{
    /// <summary>
    /// A file signing request.
    /// </summary>
    public sealed class FileSigningRequest
    {
        private readonly PemData _pemData;

        private FileSigningRequest(PemData pemData)
        {
            _pemData = pemData;
        }

        /// <summary>
        /// Writes a file signing request to the provided writer.
        /// </summary>
        /// <param name="writer">A text writer instance for a new file.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer"/> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken"/> is cancelled.</exception>
        public async Task WriteAsync(TextWriter writer, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(writer, nameof(writer));

            cancellationToken.ThrowIfCancellationRequested();

            await _pemData.WriteAsync(writer, cancellationToken);
        }

        /// <summary>
        /// Creates a file signing request with the provided message digest.
        /// </summary>
        /// <param name="digest">A message digest.</param>
        /// <returns>A <see cref="FileSigningRequest" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="digest"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="digest"/> is an empty array.</exception>
        public static FileSigningRequest Create(byte[] digest)
        {
            Assert.IsNotNullOrEmpty(digest, nameof(digest));

            var pemData = PemData.Create(digest, PemConstants.FileSigningRequest);

            return new FileSigningRequest(pemData);
        }
    }
}