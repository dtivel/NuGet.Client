// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Signing.Test
{
    public class PemParserTests
    {
        private const string _expectedData = "pear";
        private static readonly string _expectedBase64Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(_expectedData));

        [Fact]
        public async Task ParseAsync_ThrowsForNullReader()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => PemParser.ParseAsync(reader: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task ParseAsync_ThrowsIfCancelled()
        {
            using (var reader = new StringReader(string.Empty))
            {
                await Assert.ThrowsAsync<OperationCanceledException>(() => PemParser.ParseAsync(reader, new CancellationToken(canceled: true)));
            }
        }

        [Theory]
        [InlineData("-----BEGIN -----")]
        [InlineData("-----BEGIN -----\n")]
        [InlineData("-----BEGIN -----\ncGVhcg==")]
        [InlineData("-----BEGIN -----\ncGVhcg==\n")]
        [InlineData("-----BEGIN -----\ncGVhcg==\n-----END-----")]
        [InlineData("-----BEGIN -----\ncGVhcg==\n-----END  -----")]
        [InlineData("-----BEGIN -----\ncGVhcg==\n----- END -----")]
        [InlineData("-----BEGIN -----\ncGVhcg==\n----END ----")]
        public async Task ParseAsync_ThrowsForMissingPostEncapsulationBoundary(string text)
        {
            using (var reader = new StringReader(text))
            {
                await Assert.ThrowsAsync<InvalidDataException>(() => PemParser.ParseAsync(reader, CancellationToken.None));
            }
        }

        // These are all syntactically legal ABNF syntax defined in RFC 7468;
        // However, they are invalid base64 text.
        // Strictly speaking, the parser itself is not throwing here.
        [Theory]
        [InlineData("-----BEGIN -----\r\n=\r\n-----END -----")]
        [InlineData("-----BEGIN -----\r\n==\r\n-----END -----")]
        [InlineData("-----BEGIN -----\r\n=\r\n=\r\n-----END -----")]
        public async Task ParseAsync_ThrowsForInvalidBase64Text(string text)
        {
            using (var reader = new StringReader(text))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => PemParser.ParseAsync(reader, CancellationToken.None));
            }
        }

        [Theory]
        [InlineData("-----BEGIN A-----\ncGVhcg==\n-----END -----")]
        [InlineData("-----BEGIN A-----\ncGVhcg==\n-----END a-----")]
        [InlineData("-----BEGIN A-----\ncGVhcg==\n-----END B-----")]
        [InlineData("-----BEGIN A-----\ncGVhcg==\n-----END A -----")]
        public async Task ParseAsync_ThrowsForUnmatchedLabels(string text)
        {
            using (var reader = new StringReader(text))
            {
                await Assert.ThrowsAsync<InvalidDataException>(() => PemParser.ParseAsync(reader, CancellationToken.None));
            }
        }

        [Theory]
        [InlineData("-----BEGIN -----\r\ncGVhcg==\r\n-----END -----", "")]
        [InlineData("-----BEGIN !\"#$%&'()*+,./0123 4567-89:;<=>?@ABCDE FGHIJKLMNO-PQRSTUVWXYZ[\\]^_`{|}~-----\r\ncGVhcg==\r\n-----END !\"#$%&'()*+,./0123 4567-89:;<=>?@ABCDE FGHIJKLMNO-PQRSTUVWXYZ[\\]^_`{|}~-----", "!\"#$%&'()*+,./0123 4567-89:;<=>?@ABCDE FGHIJKLMNO-PQRSTUVWXYZ[\\]^_`{|}~")]
        public async Task ParseAsync_SupportsValidLabelCharacters(string text, string expectedLabel)
        {
            await ExpectOneResultAsync(text, expectedLabel, _expectedBase64Text);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\r")]
        [InlineData("\n")]
        [InlineData("\r\n\r\n")]
        [InlineData("a")]
        [InlineData("-----END -----")]
        [InlineData("-----END A-----")]
        [InlineData("-----BEGIN ----------END -----")]
        [InlineData("-----begin -----\r\ncGVhcg==\r\n-----end -----")]
        [InlineData("----BEGIN ----\r\ncGVhcg==\r\n----END ----")]
        [InlineData("-----BEGIN  -----\r\ncGVhcg==\r\n-----END  -----")]
        [InlineData("-----BEGIN -------\r\ncGVhcg==\r\n-----END -------")]
        [InlineData("------BEGIN ------\r\ncGVhcg==\r\n------END ------")]
        [InlineData("----- BEGIN -----\r\ncGVhcg==\r\n----- END -----")]
        [InlineData(" -----BEGIN A-----\r\n\r\ncGVhcg==\r\n -----END A-----")]
        [InlineData("-----BEGIN A -----\r\ncGVhcg==\r\n-----END A -----")]
        [InlineData("-----BEGIN A -----\ncGVhcg==\n-----END A-----")]
        [InlineData("-----BEGIN A------\r\ncGVhcg==\r\n-----END A------")]
        [InlineData("a-----BEGIN A-----\r\n\r\ncGVhcg==\r\na-----END A-----")]
        [InlineData("-----BEGIN a-----\r\ncGVhcg==\r\n-----END a-----")]
        public async Task ParseAsync_CanReturnEmptyResults(string text)
        {
            using (var reader = new StringReader(text))
            {
                Assert.Empty(await PemParser.ParseAsync(reader, CancellationToken.None));
            }
        }

        [Theory]
        [InlineData("-----BEGIN -----\rcGVhcg==\r-----END -----", "")]
        [InlineData("-----BEGIN -----\ncGVhcg==\n-----END -----", "")]
        [InlineData("-----BEGIN -----\r\ncGVhcg==\r\n-----END -----", "")]
        [InlineData("-----BEGIN -----\r\n\r\ncGVhcg==\r\n-----END -----", "")]
        [InlineData("-----BEGIN -----\r\n\r\ncGVhcg==\r\n-----END -----", "")]
        [InlineData("-----BEGIN A-----\r\n\r\ncGVhcg==\r\n-----END A-----", "A")]
        public async Task ParseAsync_CanReturnSingleResult(string text, string expectedLabel)
        {
            using (var reader = new StringReader(text))
            {
                var pemDatas = await PemParser.ParseAsync(reader, CancellationToken.None);

                Assert.Equal(1, pemDatas.Count());

                var pemData = pemDatas.First();

                Assert.Equal(expectedLabel, pemData.Label);
                Assert.Equal(_expectedBase64Text, pemData.Base64Text);
            }
        }

        [Fact]
        public async Task ParseAsync_CanReturnMultipleResults()
        {
            var label1 = "A";
            var label2 = "B";
            var secondExpectedBase64Text = "YXBwbGU=";
            var text = $"-----BEGIN {label1}-----\r\ncGVhcg==\r\n-----END {label1}-----\r\n-----BEGIN {label2}-----\r\n{secondExpectedBase64Text}\r\n-----END {label2}-----";

            using (var reader = new StringReader(text))
            {
                var pemDatas = await PemParser.ParseAsync(reader, CancellationToken.None);

                Assert.Equal(2, pemDatas.Count());

                var firstPemData = pemDatas.First();
                var secondPemData = pemDatas.Skip(1).First();

                Assert.Equal(label1, firstPemData.Label);
                Assert.Equal(_expectedBase64Text, firstPemData.Base64Text);

                Assert.Equal(label2, secondPemData.Label);
                Assert.Equal(secondExpectedBase64Text, secondPemData.Base64Text);
            }
        }

        [Theory]
        [InlineData("\n-----BEGIN A-----\ncGVhcg==\n-----END A-----")]
        [InlineData("a\n-----BEGIN A-----\ncGVhcg==\n-----END A-----")]
        [InlineData(" \t\v\f\r\n-----BEGIN A-----\ncGVhcg==\n-----END A-----")]
        public async Task ParseAsync_SupportsTextBeforePreEncapsulationBoundary(string text)
        {
            await ExpectOneResultAsync(text, expectedLabel: "A", expectedBase64Text: _expectedBase64Text);
        }

        [Theory]
        [InlineData("-----BEGIN A-----\ncGVhcg==\n-----END A-----\n")]
        [InlineData("-----BEGIN A-----\ncGVhcg==\n-----END A-----\na")]
        [InlineData("-----BEGIN A-----\ncGVhcg==\n-----END A-----\r\n\t\v\f\r")]
        public async Task ParseAsync_SupportsTextAfterPostEncapsulationBoundary(string text)
        {
            await ExpectOneResultAsync(text, expectedLabel: "A", expectedBase64Text: _expectedBase64Text);
        }

        [Theory]
        [InlineData("-----BEGIN -----\r-----END -----")]
        [InlineData("-----BEGIN -----\n-----END -----")]
        [InlineData("-----BEGIN -----\r\n-----END -----")]
        [InlineData("-----BEGIN -----\r\n\r\n-----END -----")]
        public async Task ParseAsync_SupportsEmptyBase64TextAndEmptyLabel(string text)
        {
            await ExpectOneResultAsync(text);
        }

        [Theory]
        [InlineData("-----BEGIN A-----\r\ncGVhcg==\r\n-----END A-----")]
        [InlineData("-----BEGIN A-----\r\nc\r\nG\r\nV\r\nh\r\nc\r\ng\r\n==\r\n-----END A-----")]
        [InlineData("-----BEGIN A-----\r\ncGVhcg=\r\n=\r\n-----END A-----")]
        [InlineData("-----BEGIN A-----\r\ncGVhcg=\t \r\n=\t \r\n-----END A-----")]
        [InlineData("-----BEGIN A-----\r\ncGVhcg\r\n==\r\n-----END A-----")]
        public async Task ParseAsync_SupportsAllBase64TextSyntaxes(string text)
        {
            await ExpectOneResultAsync(text, "A", _expectedBase64Text);
        }

        [Fact]
        public async Task ParseAsync_SupportsFinalBase64LineWithOneBase64PadCharacter()
        {
            await ExpectOneResultAsync("-----BEGIN A-----\r\nVGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZydzIGJh\r\nY2s=\r\n-----END A-----", "A", "VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZydzIGJhY2s=");
        }

        private static async Task ExpectOneResultAsync(string text, string expectedLabel = "", string expectedBase64Text = "")
        {
            using (var reader = new StringReader(text))
            {
                var pemDatas = await PemParser.ParseAsync(reader, CancellationToken.None);

                Assert.Equal(1, pemDatas.Count());

                var pemData = pemDatas.First();

                Assert.Equal(expectedLabel, pemData.Label);
                Assert.Equal(expectedBase64Text, pemData.Base64Text);
            }
        }
    }
}