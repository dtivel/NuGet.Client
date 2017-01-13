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
    public class PemDataTests
    {
        private const string _base64Text = "cGVhY2g=";
        private static readonly byte[] _data = Convert.FromBase64String(_base64Text);
        private const string _label = "B";

        [Fact]
        public void Create_ThrowsForNullData()
        {
            Assert.Throws<ArgumentNullException>(() => PemData.Create(data: null, label: _label));
        }

        [Fact]
        public void Create_ThrowsForNullLabel()
        {
            Assert.Throws<ArgumentNullException>(() => PemData.Create(_data, label: null));
        }

        [Fact]
        public void Create_InitializesPropertiesFromArguments()
        {
            var pemData = PemData.Create(_data, _label);

            Assert.Equal(_base64Text, pemData.Base64Text);
            Assert.Equal(_label, pemData.Label);
        }

        [Fact]
        public async Task WriteAsync_ThrowsForNullWriter()
        {
            var pemData = PemData.Create(_data, _label);

            await Assert.ThrowsAsync<ArgumentNullException>(() => pemData.WriteAsync(writer: null, cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task WriteAsync_ThrowsIfCancelled()
        {
            var pemData = PemData.Create(_data, _label);

            using (var writer = new StringWriter())
            {
                await Assert.ThrowsAsync<OperationCanceledException>(() => pemData.WriteAsync(writer, new CancellationToken(canceled: true)));
            }
        }

        [Fact]
        public async Task WriteAsync_WrapsBase64TextAt64Characters()
        {
            var data = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            var pemData = PemData.Create(data, _label);

            using (var writer = new StringWriter())
            {
                await pemData.WriteAsync(writer, CancellationToken.None);

                var expectedResult = string.Format(CultureInfo.InvariantCulture, "-----BEGIN B-----{0}TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2Np{0}bmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFi{0}b3JlIGV0IGRvbG9yZSBtYWduYSBhbGlxdWEuIFV0IGVuaW0gYWQgbWluaW0gdmVu{0}aWFtLCBxdWlzIG5vc3RydWQgZXhlcmNpdGF0aW9uIHVsbGFtY28gbGFib3JpcyBu{0}aXNpIHV0IGFsaXF1aXAgZXggZWEgY29tbW9kbyBjb25zZXF1YXQuIER1aXMgYXV0{0}ZSBpcnVyZSBkb2xvciBpbiByZXByZWhlbmRlcml0IGluIHZvbHVwdGF0ZSB2ZWxp{0}dCBlc3NlIGNpbGx1bSBkb2xvcmUgZXUgZnVnaWF0IG51bGxhIHBhcmlhdHVyLiBF{0}eGNlcHRldXIgc2ludCBvY2NhZWNhdCBjdXBpZGF0YXQgbm9uIHByb2lkZW50LCBz{0}dW50IGluIGN1bHBhIHF1aSBvZmZpY2lhIGRlc2VydW50IG1vbGxpdCBhbmltIGlk{0}IGVzdCBsYWJvcnVtLg=={0}-----END B-----", Environment.NewLine);

                Assert.Equal(expectedResult, writer.ToString());
            }
        }
    }
}