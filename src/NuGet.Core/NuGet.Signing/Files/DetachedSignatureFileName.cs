// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NuGet.Signing
{
    /// <summary>
    /// Represents a detached signature file name.
    /// </summary>
    public sealed class DetachedSignatureFileName : IEquatable<DetachedSignatureFileName>
    {
        private static readonly Regex IdentifierPattern = new Regex(@"^[.]\w+([_-]\w+)*$");

        /// <summary>
        /// Instantiates a new instance.
        /// </summary>
        /// <param name="packageFileName">The associated package file name.</param>
        /// <param name="identifier">The detached signature file identifier.</param>
        /// <param name="fileExtension">The detached signature file extension.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageFileName" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="packageFileName" /> is an empty string.</exception>
        public DetachedSignatureFileName(
            string packageFileName,
            string identifier,
            string fileExtension)
        {
            Assert.IsNotNullOrEmpty(packageFileName, nameof(packageFileName));

            ValidateFileExtension(fileExtension, shouldThrow: true);

            if (identifier != null)
            {
                ValidateIdentifier(identifier, shouldThrow: true);
            }

            PackageFileName = packageFileName;
            Identifier = identifier;
            FileExtension = fileExtension;

            var fileName = new StringBuilder();
            fileName.Append(PackageFileName);
            fileName.Append(Identifier);
            fileName.Append(FileExtension);
            FileName = fileName.ToString();
        }

        /// <summary>
        /// The detached signature file name.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The associated package file name.
        /// </summary>
        public string PackageFileName { get; }

        /// <summary>
        /// The detached signature file identifier.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// The detached signature file extension.
        /// </summary>
        public string FileExtension { get; }

        /// <summary>
        /// Gets a string representation of a detached signature file name.
        /// </summary>
        /// <returns>A string representation of a detached signature file name.</returns>
        public override string ToString()
        {
            return FileName;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(DetachedSignatureFileName other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(FileName, other.FileName);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as DetachedSignatureFileName);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(FileName);
        }

        /// <summary>
        /// Determines whether the specified object instances are considered equal.
        /// </summary>
        /// <param name="a">The first object to compare.</param>
        /// <param name="b">The second object to compare.</param>
        /// <returns><c>true</c> if the objects are considered equal; otherwise, <c>false</c>.
        /// If both objects are <c>null</c>, the method returns <c>true</c>.</returns>
        public static bool operator ==(DetachedSignatureFileName a, DetachedSignatureFileName b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the specified object instances are considered not equal.
        /// </summary>
        /// <param name="a">The first object to compare.</param>
        /// <param name="b">The second object to compare.</param>
        /// <returns><c>true</c> if the objects are considered not equal; otherwise, <c>true</c>.</returns>
        public static bool operator !=(DetachedSignatureFileName a, DetachedSignatureFileName b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Parses a detached signature file name.
        /// </summary>
        /// <param name="fileName">The file name to parse.</param>
        /// <param name="packageFileName">The associated package file name.</param>
        /// <returns>A <see cref="DetachedSignatureFileName"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileName" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fileName" /> is an invalid detached signature file name.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageFileName" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageFileName" /> is an empty string.</exception>
        public static DetachedSignatureFileName Parse(string fileName, string packageFileName)
        {
            DetachedSignatureFileName output;
            Parse(fileName, packageFileName, shouldThrow: true, output: out output);
            return output;
        }

        /// <summary>
        /// Tries to parse a detached signature file name.
        /// </summary>
        /// <param name="fileName">The file name to parse.</param>
        /// <param name="packageFileName">The associated package file name.</param>
        /// <param name="output">A <see cref="DetachedSignatureFileName"/>.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
        public static bool TryParse(string fileName, string packageFileName, out DetachedSignatureFileName output)
        {
            return Parse(fileName, packageFileName, shouldThrow: false, output: out output);
        }

        /// <summary>
        /// Determines if the provided file identifier is valid.
        /// </summary>
        /// <param name="fileIdentifier">The file identifier.</param>
        /// <returns><c>true</c> if the file identifier is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValidFileIdentifier(string fileIdentifier)
        {
            return ValidateIdentifier(fileIdentifier, shouldThrow: false);
        }

        private static bool Parse(
            string fileName,
            string packageFileName,
            bool shouldThrow,
            out DetachedSignatureFileName output)
        {
            output = null;

            if (!ValidatePackageFileName(fileName, packageFileName, shouldThrow))
            {
                return false;
            }

            // Use the casing in the detached signature file name.
            packageFileName = fileName.Substring(0, packageFileName.Length);

            var fileExtension = Path.GetExtension(fileName);

            if (!ValidateFileExtension(fileExtension, shouldThrow))
            {
                return false;
            }

            var withoutExtension = fileName.Substring(0, fileName.Length - fileExtension.Length);

            if (withoutExtension.Length == packageFileName.Length)
            {
                output = new DetachedSignatureFileName(
                    packageFileName,
                    identifier: null,
                    fileExtension: fileExtension);

                return true;
            }

            var identifier = withoutExtension.Substring(packageFileName.Length);

            if (!ValidateIdentifier(identifier, shouldThrow))
            {
                return false;
            }

            output = new DetachedSignatureFileName(
                packageFileName,
                identifier,
                fileExtension);

            return true;
        }

        private static bool ValidateIdentifier(string identifier, bool shouldThrow)
        {
            if (string.IsNullOrEmpty(identifier) ||
                identifier.Length < 2 ||
                identifier.Length > 32 ||
                !IdentifierPattern.IsMatch(identifier))
            {
                if (shouldThrow)
                {
                    throw new ArgumentException(Strings.InvalidDetachedSignatureFileIdentifier, nameof(identifier));
                }

                return false;
            }

            return true;
        }

        private static bool ValidatePackageFileName(string fileName, string packageFileName, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.IsNotNullOrEmpty(packageFileName, nameof(packageFileName));
                Assert.IsNotNullOrEmpty(fileName, nameof(fileName));
            }
            else if (fileName == null || packageFileName == null)
            {
                return false;
            }

            if (fileName.IndexOf(packageFileName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (shouldThrow)
                {
                    throw new ArgumentException(
                        Strings.SignatureFileNameMustStartWithPackageFileName,
                        nameof(packageFileName));
                }

                return false;
            }

            return true;
        }

        private static bool ValidateFileExtension(string fileExtension, bool shouldThrow)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(".sig", fileExtension))
            {
                if (shouldThrow)
                {
                    throw new ArgumentException(Strings.InvalidSignatureFileExtension, nameof(fileExtension));
                }

                return false;
            }

            return true;
        }
    }
}