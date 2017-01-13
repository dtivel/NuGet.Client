// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using NuGet.Common;

namespace NuGet.Signing
{
    /// <summary>
    /// A signer identity.
    /// </summary>
    public sealed class SignerIdentity
    {
        private readonly int _hashCode;
        private const string _distinguishedNamePropertyName = "distinguishedName";
        private const string _publicKeyHashFieldName = "publicKeyHash";
        private const string _hashAlgorithmNameFieldName = "hashAlgorithmName";

        /// <summary>
        /// The X.500 distinguished name.
        /// </summary>
        public string DistingishedName { get; }

        /// <summary>
        /// The public key hash algorithm name.
        /// </summary>
        public string HashAlgorithmName { get; }

        /// <summary>
        /// The public key hash.
        /// </summary>
        public string PublicKeyHash { get; }

        private SignerIdentity(string distinguishedName, string hashAlgorithmName, string publicKeyHash)
        {
            Assert.IsNotNullOrEmpty(distinguishedName, nameof(distinguishedName));
            Assert.IsNotNullOrEmpty(hashAlgorithmName, nameof(hashAlgorithmName));
            Assert.IsNotNullOrEmpty(publicKeyHash, nameof(publicKeyHash));

            DistingishedName = distinguishedName;
            HashAlgorithmName = hashAlgorithmName;
            PublicKeyHash = publicKeyHash;

            _hashCode = ToString().GetHashCode();
        }

        /// <summary>
        /// Creates a new instance from the provided X.509 certificate.
        /// </summary>
        /// <param name="certificate">An X.509 certificate.</param>
        /// <returns>A <see cref="SignerIdentity" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="certificate" /> is <c>null</c>.</exception>
        public static SignerIdentity Create(X509Certificate2 certificate)
        {
            Assert.IsNotNull(certificate, nameof(certificate));

            var hashAlgorithmName = Constants.DefaultHashAlgorithmName.ToLowerInvariant();
            var publicKey = certificate.GetPublicKey();
            var publicKeyHash = Convert.ToBase64String(ComputeHash(publicKey));

            return new SignerIdentity(certificate.SubjectName.Name, hashAlgorithmName, publicKeyHash);
        }

        /// <summary>
        /// Parses a signer identity string.
        /// </summary>
        /// <param name="signerIdentityString"></param>
        /// <returns>A <see cref="SignerIdentity" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="signerIdentityString" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="signerIdentityString" /> is an invalid signer identity string.</exception>
        public static SignerIdentity Parse(string signerIdentityString)
        {
            Assert.IsNotNullOrEmpty(signerIdentityString, nameof(signerIdentityString));

            var index = signerIdentityString.IndexOf($";{_publicKeyHashFieldName}=");

            if (index == -1)
            {
                throw new ArgumentException(Strings.InvalidSignerIdentityString, nameof(signerIdentityString));
            }

            var distinguishedNameProperty = signerIdentityString.Substring(0, index);
            var pattern = new Regex($"^{_distinguishedNamePropertyName}=(?<{_distinguishedNamePropertyName}>[^,]+)$");
            var match = pattern.Match(distinguishedNameProperty);

            if (!match.Success)
            {
                throw new ArgumentException(Strings.InvalidSignerIdentityString, nameof(signerIdentityString));
            }

            string distinguishedName;

            try
            {
                distinguishedName = new X500DistinguishedName(match.Groups[_distinguishedNamePropertyName].Value).Name;
            }
            catch (CryptographicException)
            {
                throw new ArgumentException(Strings.InvalidSignerIdentityString, nameof(signerIdentityString));
            }

            var publicKeyHashProperty = signerIdentityString.Substring(index + 1); // +1 for leading semicolon delimiter
            pattern = new Regex($"^{_publicKeyHashFieldName}=(?<{_hashAlgorithmNameFieldName}>[^:]+):(?<{_publicKeyHashFieldName}>.+)$");
            match = pattern.Match(publicKeyHashProperty);

            if (!match.Success)
            {
                throw new ArgumentException(Strings.InvalidSignerIdentityString, nameof(signerIdentityString));
            }

            var hashAlgorithmName = match.Groups[_hashAlgorithmNameFieldName].Value;
            var publicKeyHash = match.Groups[_publicKeyHashFieldName].Value;

            if (!string.Equals(hashAlgorithmName, Constants.DefaultHashAlgorithmName.ToLowerInvariant()))
            {
                throw new ArgumentException(Strings.InvalidHashAlgorithmName);
            }

            if (!Base64Utilities.IsBase64Text(publicKeyHash))
            {
                throw new ArgumentException(Strings.InvalidPublicKeyHash, nameof(signerIdentityString));
            }

            return new SignerIdentity(distinguishedName, hashAlgorithmName, publicKeyHash);
        }

        /// <summary>
        /// Gets the string representation of a signer identity.
        /// </summary>
        /// <returns>A string representation of a signer identity.</returns>
        public override string ToString()
        {
            return $"distinguishedName={DistingishedName};publicKeyHash={HashAlgorithmName}:{PublicKeyHash}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(SignerIdentity other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return DistingishedName == other.DistingishedName
                && HashAlgorithmName == other.HashAlgorithmName
                && PublicKeyHash == other.PublicKeyHash;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SignerIdentity);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Determines whether the specified object instances are considered equal.
        /// </summary>
        /// <param name="a">The first object to compare.</param>
        /// <param name="b">The second object to compare.</param>
        /// <returns><c>true</c> if the objects are considered equal; otherwise, <c>false</c>.
        /// If both objects are <c>null</c>, the method returns <c>true</c>.</returns>
        public static bool operator ==(SignerIdentity a, SignerIdentity b)
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
        public static bool operator !=(SignerIdentity a, SignerIdentity b)
        {
            return !a.Equals(b);
        }

        private static byte[] ComputeHash(byte[] data)
        {
            using (var hashFunc = new Sha512HashFunction())
            {
                hashFunc.Update(data, offset: 0, count: data.Length);

                return hashFunc.GetHash();
            }
        }
    }
}