// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;

namespace NuGet.Signing
{
    /// <summary>
    /// Signing utilities.
    /// </summary>
    public static class SigningUtilities
    {
        /// <summary>
        /// Computes a message digest for the provided file.
        /// </summary>
        /// <param name="filePath">The path of the file for which a digest should be computed.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous compute operation.
        /// The task result (<see cref="Task{TResult}.Result" />) returns the digest.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="filePath" /> is an empty string.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
        public static async Task<byte[]> ComputeDigestAsync(string filePath, CancellationToken cancellationToken)
        {
            Assert.IsNotNullOrEmpty(filePath, nameof(filePath));

            cancellationToken.ThrowIfCancellationRequested();

            using (var stream = File.OpenRead(filePath))
            {
                return await ComputeDigestAsync(stream, cancellationToken);
            }
        }

        // Public only for testing purposes
        public static async Task<byte[]> ComputeDigestAsync(Stream stream, CancellationToken cancellationToken)
        {
            using (var hashFunc = new Sha512HashFunction())
            {
                var buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, offset: 0, count: buffer.Length, cancellationToken: cancellationToken)) > 0)
                {
                    hashFunc.Update(buffer, offset: 0, count: bytesRead);
                }

                return hashFunc.GetHash();
            }
        }

        /// <summary>
        /// Reads signatures from a detached signature file.
        /// </summary>
        /// <param name="detachedSignatureFilePath">The path of the detached signature file.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous read operation.
        /// The task result (<see cref="Task{TResult}.Result" />) returns an enumerable collection of <see cref="Signature"/> objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="detachedSignatureFilePath" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="detachedSignatureFilePath" /> is an empty string.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
        public static async Task<IEnumerable<Signature>> ReadSignaturesAsync(string detachedSignatureFilePath, CancellationToken cancellationToken)
        {
            Assert.IsNotNullOrEmpty(detachedSignatureFilePath, nameof(detachedSignatureFilePath));

            cancellationToken.ThrowIfCancellationRequested();

            var file = await ReadDetachedSignatureFileAsync(detachedSignatureFilePath, cancellationToken);

            return file.Signatures;
        }

        private static Task<DetachedSignatureFile> ReadDetachedSignatureFileAsync(string filePath, CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(filePath))
            {
                return DetachedSignatureFile.ReadAsync(reader, cancellationToken);
            }
        }
    }
}