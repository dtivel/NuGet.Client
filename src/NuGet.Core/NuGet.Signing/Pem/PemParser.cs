// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Signing
{
    // This is public only to enable testing.
    // This adheres to the standard parsing rules defined in RFC 7468 (https://tools.ietf.org/html/rfc7468).
    public sealed class PemParser
    {
        private const string Label = "label";

        private static readonly Regex _base64LinePattern = new Regex($"^[{PemConstants.Base64Char}]+[{PemConstants.Whitespace}]*$");
        private static readonly Regex _base64Final1Part1Pattern = new Regex($"^[{PemConstants.Base64Char}]*=[{PemConstants.Whitespace}]*$");
        private static readonly Regex _base64Final1Part2Pattern = new Regex($"=[{PemConstants.Whitespace}]*$");
        private static readonly Regex _base64Final2Pattern = new Regex($"^[{PemConstants.Base64Char}]*(=){{0,2}}[{PemConstants.Whitespace}]*$");
        private static readonly Regex _preEncapsulationBoundaryPattern = new Regex($"^\\-{{5}}BEGIN (?<{Label}>(?>[{PemConstants.LabelChar}]+(?>[\\- ][{PemConstants.LabelChar}])*)*)\\-{{5}}[{PemConstants.Whitespace}]*$");
        private static readonly Regex _postEncapsulationBoundaryPattern = new Regex($"^\\-{{5}}END (?<{Label}>(?>[{PemConstants.LabelChar}]+(?>[\\- ][{PemConstants.LabelChar}])*)*)\\-{{5}}[{PemConstants.Whitespace}]*$");
        private static readonly Regex _whitespaceLinePattern = new Regex($"^[{PemConstants.Whitespace}]*$");

        private StringBuilder _encapsulatedData = new StringBuilder();
        private string _label;
        private string _line;
        private List<PemData> _pemDatas = new List<PemData>();

        private PemParser() { }

        private void AppendLine()
        {
            _encapsulatedData.Append(_line.TrimEnd('\t', ' '));
        }

        private async Task<IEnumerable<PemData>> ParseAsyncImpl(TextReader reader, CancellationToken cancellationToken)
        {
            while ((_line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (TryReadPreEncapsulationBoundary())
                {
                    await ReadWhitespaceAsync(reader);
                    await ReadEncapsulatedDataAsync(reader);
                    ReadPostEncapsulationBoundary();

                    var data = GetBytes(_encapsulatedData.ToString());
                    var pemData = PemData.Create(data, _label);

                    _pemDatas.Add(pemData);

                    Reset();
                }
            }

            return _pemDatas;
        }

        private bool TryReadPreEncapsulationBoundary()
        {
            var match = _preEncapsulationBoundaryPattern.Match(_line);

            if (match.Success)
            {
                if (_label != null)
                {
                    throw new InvalidDataException(Strings.InvalidPemEncodedTextInvalidPreEncapsulationBoundary);
                }

                _label = match.Groups[Label].Value;

                return true;
            }

            return false;
        }

        private async Task ReadWhitespaceAsync(TextReader reader)
        {
            while ((_line = await reader.ReadLineAsync()) != null)
            {
                if (!_whitespaceLinePattern.IsMatch(_line))
                {
                    break;
                }
            }
        }

        private async Task ReadEncapsulatedDataAsync(TextReader reader)
        {
            if (_line == null)
            {
                throw new InvalidDataException(Strings.InvalidPemEncodedTextInvalidBase64Data);
            }

            var previousLine = _line;
            var isFinal = false;
            var done = false;

            do
            {
                if (done)
                {
                    break;
                }

                if (isFinal && _base64Final1Part1Pattern.IsMatch(previousLine) && _base64Final1Part2Pattern.IsMatch(_line))
                {
                    AppendLine();

                    done = true; // but read one more line
                }
                else if (_base64LinePattern.IsMatch(_line))
                {
                    AppendLine();

                    previousLine = _line;
                }
                else if (_base64Final2Pattern.IsMatch(_line))
                {
                    isFinal = true;

                    AppendLine();

                    previousLine = _line;
                }
                else
                {
                    break;
                }
            }
            while ((_line = await reader.ReadLineAsync()) != null);
        }

        private void ReadPostEncapsulationBoundary()
        {
            if (_line == null)
            {
                throw new InvalidDataException(Strings.InvalidPemEncodedTextPostEncapsulationBoundaryNotFound);
            }

            var match = _postEncapsulationBoundaryPattern.Match(_line);

            if (!match.Success)
            {
                throw new InvalidDataException(Strings.InvalidPemEncodedTextPostEncapsulationBoundaryNotFound);
            }

            if (!string.Equals(_label, match.Groups[Label].Value))
            {
                throw new InvalidDataException(Strings.InvalidPemEncodedTextMismatchedEncapsulationBoundaryLabel);
            }
        }

        private void Reset()
        {
            _encapsulatedData.Clear();
            _label = null;
        }

        public static async Task<IEnumerable<PemData>> ParseAsync(TextReader reader, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(reader, nameof(reader));

            cancellationToken.ThrowIfCancellationRequested();

            var parser = new PemParser();

            return await parser.ParseAsyncImpl(reader, cancellationToken);
        }

        private static byte[] GetBytes(string base64Text)
        {
            try
            {
                return Convert.FromBase64String(base64Text);
            }
            catch (FormatException)
            {
                throw new ArgumentException(Strings.InvalidBase64Text);
            }
        }
    }
}