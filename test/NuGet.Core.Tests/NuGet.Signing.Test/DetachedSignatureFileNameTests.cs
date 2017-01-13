// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace NuGet.Signing.Test
{
    public class DetachedSignatureFileNameTests
    {
        private const string FileName = "NuGet.2.12.0.nupkg.sig";
        private const string PackageFileName = "NuGet.2.12.0.nupkg";

        public static IEnumerable<object[]> ValidParse => new[]
        {
            new object[] { FileName, PackageFileName, PackageFileName, null, ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.SIG", PackageFileName, PackageFileName, null, ".SIG" },
            new object[] { "NuGet.2.12.0.nupkg.a.sig", PackageFileName, PackageFileName, ".a", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.AA.sig", PackageFileName, PackageFileName, ".AA", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.a-a.sig", PackageFileName, PackageFileName, ".a-a", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.a-_.sig", PackageFileName, PackageFileName, ".a-_", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.ö.sig", PackageFileName, PackageFileName, ".ö", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.a_.sig", PackageFileName, PackageFileName, ".a_", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.a_a.sig", PackageFileName, PackageFileName, ".a_a", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.1.sig", PackageFileName, PackageFileName, ".1", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.originator.sig", PackageFileName, PackageFileName, ".originator", ".sig" },
            new object[] { "NuGet.2.12.0.nupkg.SoThisIdentifierHas32Characters.sig", PackageFileName, PackageFileName, ".SoThisIdentifierHas32Characters", ".sig" },
            new object[] { "NUGET.2.12.0.NUPKG.sig", PackageFileName, "NUGET.2.12.0.NUPKG", null, ".sig" },
        };

        public static IEnumerable<object[]> InvalidParse => new[]
        {
            new object[] { "NuGet.2.12.0.nupkg.asig" },
            new object[] { "NuGet.2.12.0.nupkg..sig" },
            new object[] { "NuGet.2.12.0.nupkg.a-.sig" },
            new object[] { "NuGet.2.12.0.nupkg.-.sig" },
            new object[] { "NuGet.2.12.0.nupkg. .sig" },
            new object[] { "NuGet.2.12.0.nupkg.[.sig" },
            new object[] { "NuGet.2.12.0.nupkg.{.sig" },
            new object[] { "NuGet.2.12.0.nupkg.`.sig" },
            new object[] { "NuGet.2.12.0.nupkg.thisidentifierhasmorethan32chars.sig" },
            new object[] { "NuGet.2.13.0.nupkg.sig" },
            new object[] { "NuGet.2.012.0.nupkg.sig" },
            new object[] { "NuGet.2.12.0.sig" },
            new object[] { "prefix-NuGet.2.12.0.nupkg.sig" },
            new object[] { " NuGet.2.12.0.nupkg.sig" },
            new object[] { "NuGet.2.12.0.nupkg.sig.suffix" },
        };

        [Theory]
        [MemberData(nameof(ValidParse))]
        public void TryParse_Valid(
            string fileName,
            string inputPackageFileName,
            string expectedPackageFileName,
            string identifer,
            string fileExtension)
        {
            // Arrange
            DetachedSignatureFileName output;

            // Act
            var actual = DetachedSignatureFileName.TryParse(fileName, inputPackageFileName, out output);

            // Assert
            Assert.True(actual);
            Assert.Equal(fileName, output.FileName);
            Assert.Equal(expectedPackageFileName, output.PackageFileName);
            Assert.Equal(identifer, output.Identifier);
            Assert.Equal(fileExtension, output.FileExtension);
        }

        [Theory]
        [MemberData(nameof(ValidParse))]
        public void Parse_Valid(
            string fileName,
            string inputPackageFileName,
            string expectedPackageFileName,
            string identifer,
            string fileExtension)
        {
            // Arrange & Act
            var actual = DetachedSignatureFileName.Parse(fileName, inputPackageFileName);

            // Assert
            Assert.Equal(fileName, actual.FileName);
            Assert.Equal(expectedPackageFileName, actual.PackageFileName);
            Assert.Equal(identifer, actual.Identifier);
            Assert.Equal(fileExtension, actual.FileExtension);
        }

        [Theory]
        [MemberData(nameof(InvalidParse))]
        public void TryParse_Invalid(string fileName)
        {
            // Arrange
            DetachedSignatureFileName output;

            // Act
            var actual = DetachedSignatureFileName.TryParse(fileName, PackageFileName, out output);

            // Assert
            Assert.False(actual);
            Assert.Null(output);
        }

        [Theory]
        [MemberData(nameof(InvalidParse))]
        public void Parse_Invalid(string fileName)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                DetachedSignatureFileName.Parse(fileName, PackageFileName));
        }

        [Theory]
        [InlineData(".a")]
        [InlineData(".This_Identifier-Has32Characters")]
        public void IsValidFileIdentifier_True(string fileIdentifier)
        {
            Assert.True(DetachedSignatureFileName.IsValidFileIdentifier(fileIdentifier));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        [InlineData(".")]
        [InlineData(". ")]
        [InlineData(".a.")]
        [InlineData(".ThisIdentifierHasMoreThan32Characters")]
        public void IsValidFileIdentifier_False(string fileIdentifier)
        {
            Assert.False(DetachedSignatureFileName.IsValidFileIdentifier(fileIdentifier));
        }

        [Theory]
        [InlineData(PackageFileName, null, ".sig", FileName)]
        [InlineData(PackageFileName, ".originator", ".sig", "NuGet.2.12.0.nupkg.originator.sig")]
        public void ToString_UsesProperties(string packageFileName, string identifier, string fileExtension, string expected)
        {
            // Arrange
            var target = new DetachedSignatureFileName(
                packageFileName,
                identifier,
                fileExtension);

            // Act
            var actual = target.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Equals_True()
        {
            // Arrange
            var a = new DetachedSignatureFileName(PackageFileName, ".a", ".sig");
            var b = new DetachedSignatureFileName(PackageFileName, ".a", ".sig");

            // Act & Assert
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_False()
        {
            // Arrange
            var a = new DetachedSignatureFileName(PackageFileName, ".a", ".sig");
            var b = new DetachedSignatureFileName(PackageFileName, null, ".sig");

            // Act & Assert
            Assert.False(a.Equals(b));
            Assert.False(a.Equals(null));
            Assert.False(a == b);
            Assert.False(a == null);
            Assert.True(a != b);
        }
    }
}