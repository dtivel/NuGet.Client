// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Signing.Test
{
    public class SigningUtilitiesTests
    {
        [Fact]
        public async Task ComputeDigestAsync_ThrowsForNullFilePath()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => SigningUtilities.ComputeDigestAsync(filePath: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task ComputeDigestAsync_ThrowsForEmptyFilePath()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => SigningUtilities.ComputeDigestAsync(filePath: string.Empty, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task ComputeDigestAsync_ThrowsForNonExistentFilePath()
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            await Assert.ThrowsAsync<FileNotFoundException>(() => SigningUtilities.ComputeDigestAsync(filePath, CancellationToken.None));
        }

        [Fact]
        public async Task ComputeDigestAsync_ThrowsIfCancelled()
        {
            using (var testDirectory = TestDirectory.Create())
            {
                var filePath = Path.Combine(testDirectory.Path, "a");

                File.WriteAllText(filePath, string.Empty);

                await Assert.ThrowsAsync<OperationCanceledException>(() => SigningUtilities.ComputeDigestAsync(filePath, new CancellationToken(canceled: true)));
            }
        }

        [Fact]
        public async Task ComputeDigestAsync_SupportsEmptyFile()
        {
            const string expectedBase64Value = "z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg/SpIdNs6c5H0NE8XYXysP+DGNKHfuwvY7kxvUdBeoGlODJ6+SfaPg==";

            using (var testDirectory = TestDirectory.Create())
            {
                var filePath = Path.Combine(testDirectory.Path, "a");

                File.WriteAllText(filePath, string.Empty);

                var digest = await SigningUtilities.ComputeDigestAsync(filePath, CancellationToken.None);
                var actualBase64Value = Convert.ToBase64String(digest);

                Assert.Equal(expectedBase64Value, actualBase64Value);
            }
        }

        [Fact]
        public async Task ComputeDigestAsync()
        {
            const string expectedBase64Value = "NJAwUJVdN8HOjha9VNbopjFMaPVZlAPYFef4CpiYGvVEYmafbYo5CB9KtPFXF5pG7Tj7jBb4/axBJpxZKGEY2Q==";

            using (var testDirectory = TestDirectory.Create())
            {
                var filePath = Path.Combine(testDirectory.Path, "a");

                File.WriteAllText(filePath, "peach");

                var digest = await SigningUtilities.ComputeDigestAsync(filePath, CancellationToken.None);
                var actualBase64Value = Convert.ToBase64String(digest);

                Assert.Equal(expectedBase64Value, actualBase64Value);
            }
        }
    }
}