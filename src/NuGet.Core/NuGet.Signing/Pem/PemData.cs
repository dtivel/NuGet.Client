// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Signing
{
    /// <summary>
    /// PEM-encoded data.
    /// </summary>
    public sealed class PemData
    {
        private static readonly Regex _labelPattern = new Regex($"(?>[{PemConstants.LabelChar}]+(?>[\\- ][{PemConstants.LabelChar}])*)*");

        public string Base64Text { get; private set; }
        public string Label { get; private set; }

        private PemData(string base64Text, string label)
        {
            Base64Text = base64Text;
            Label = label;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="data">Binary data to base64 encode.</param>
        /// <param name="label">An encapsulation boundary label.</param>
        /// <returns>A <see cref="PemData" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="label" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="label" /> is an invalid encapsulation boundary label.</exception>
        public static PemData Create(byte[] data, string label)
        {
            Assert.IsNotNull(data, nameof(data));
            Assert.IsNotNull(label, nameof(label));

            if (!_labelPattern.IsMatch(label))
            {
                throw new ArgumentException(Strings.InvalidLabel, nameof(label));
            }

            var base64Text = Convert.ToBase64String(data);

            return new PemData(base64Text, label);
        }

        /// <summary>
        /// Writes PEM data to the provided writer.
        /// </summary>
        /// <param name="writer">A text writer.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer" /> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
        public async Task WriteAsync(TextWriter writer, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(writer, nameof(writer));

            cancellationToken.ThrowIfCancellationRequested();

            var builder = new StringBuilder();

            for (var i = 0; i < Base64Text.Length; i += 64)
            {
                var length = Math.Min(64, Base64Text.Length - i);

                builder.AppendLine(Base64Text.Substring(i, length));
            }

            var text = string.Format(CultureInfo.InvariantCulture, "-----BEGIN {0}-----{1}{2}-----END {0}-----", Label, Environment.NewLine, builder.ToString());

            await writer.WriteAsync(text);
        }
    }
}