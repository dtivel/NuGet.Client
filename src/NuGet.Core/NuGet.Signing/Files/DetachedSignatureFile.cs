// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Signing
{
    /// <summary>
    /// Provides a read-only view of a detached signature file and the signatures it contains.
    /// <seealso cref="Signature" />
    /// </summary>
    public sealed class DetachedSignatureFile
    {
        private List<Signature> _signatures;

        /// <summary>
        /// The signatures contained within the detached signature file.
        /// </summary>
        public IEnumerable<Signature> Signatures
        {
            get { return _signatures.AsReadOnly(); }
        }

        private DetachedSignatureFile(List<Signature> signatures)
        {
            Assert.IsNotNull(signatures, nameof(signatures));

            _signatures = signatures;
        }

        /// <summary>
        /// Reads a detached signature file using the provided text reader.
        /// </summary>
        /// <param name="reader">A text reader for an existing detached signature file.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous read operation.
        /// The task result (<see cref="Task{TResult}.Result" />) returns a <see cref="DetachedSignatureFile"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader" /> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
        public static async Task<DetachedSignatureFile> ReadAsync(TextReader reader, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(reader, nameof(reader));

            cancellationToken.ThrowIfCancellationRequested();

            var pemDatas = await PemParser.ParseAsync(reader, cancellationToken);

            pemDatas = pemDatas.Where(pem => string.Equals(pem.Label, PemConstants.FileSignature, StringComparison.Ordinal));

            var signatures = new List<Signature>();

            foreach (var pemData in pemDatas)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var signature = DecodeSignature(pemData.Base64Text);

                signatures.Add(signature);
            }

            return new DetachedSignatureFile(signatures);
        }

        /// <summary>
        /// Reads a detached signature file.
        /// </summary>
        /// <param name="filePath">The path for an existing detached signature file.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous read operation.
        /// The task result (<see cref="Task{TResult}.Result" />) returns a <see cref="DetachedSignatureFile"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="filePath" /> is an empty string.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file indicated by <paramref name="filePath" /> does not exist.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
        public static async Task<DetachedSignatureFile> ReadAsync(string filePath, CancellationToken cancellationToken)
        {
            Assert.IsNotNullOrEmpty(filePath, nameof(filePath));

            cancellationToken.ThrowIfCancellationRequested();

            using (var reader = new StreamReader(filePath))
            {
                return await ReadAsync(reader, cancellationToken);
            }
        }

        private static Signature DecodeSignature(string base64Text)
        {
            var bytes = Convert.FromBase64String(base64Text);
            var signedCms = new SignedCms();

            signedCms.Decode(bytes);

            if (!string.Equals(signedCms.ContentInfo.ContentType.Value, Constants.Pkcs7DataOid, StringComparison.Ordinal))
            {
                throw new InvalidDataException(Strings.InvalidSignatureContentType);
            }

            var signatureTargets = Asn1Utilities.Decode(signedCms.ContentInfo.Content);

            return Signature.FromSignedCms(signedCms, signatureTargets);
        }
    }
}