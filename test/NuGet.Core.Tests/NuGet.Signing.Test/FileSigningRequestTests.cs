// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Signing.Test
{
    public class FileSigningRequestTests
    {
        private static readonly byte[] _digest = Encoding.UTF8.GetBytes("a");

        [Fact]
        public void Create_ThrowsForNullDigest()
        {
            Assert.Throws<ArgumentNullException>(() => FileSigningRequest.Create(digest: null));
        }

        [Fact]
        public void Create_ThrowsForEmptyDigest()
        {
            Assert.Throws<ArgumentException>(() => FileSigningRequest.Create(digest: new byte[] { }));
        }

        [Fact]
        public async Task WriteAsync_ThrowsForNullWriter()
        {
            var fileSigningRequest = FileSigningRequest.Create(_digest);

            await Assert.ThrowsAsync<ArgumentNullException>(() => fileSigningRequest.WriteAsync(writer: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task WriteAsync_ThrowsIfCancelled()
        {
            var fileSigningRequest = FileSigningRequest.Create(_digest);

            using (var writer = new StringWriter())
            {
                await Assert.ThrowsAsync<OperationCanceledException>(() => fileSigningRequest.WriteAsync(writer, new CancellationToken(canceled: true)));
            }
        }

        [Fact]
        public async Task WriteAsync_GeneratesPemEncodedRequest()
        {
            var expectedResult = string.Format(CultureInfo.InvariantCulture, "-----BEGIN FILE SIGNING REQUEST-----{0}YQ=={0}-----END FILE SIGNING REQUEST-----", Environment.NewLine);
            var fileSigningRequest = FileSigningRequest.Create(_digest);

            using (var writer = new StringWriter())
            {
                await fileSigningRequest.WriteAsync(writer, CancellationToken.None);

                Assert.Equal(expectedResult, writer.ToString());
            }
        }
    }
}