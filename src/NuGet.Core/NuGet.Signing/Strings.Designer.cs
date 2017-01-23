﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NuGet.Signing {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///    A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal Strings() {
        }
        
        /// <summary>
        ///    Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NuGet.Signing.Strings", typeof(Strings).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///    Overrides the current thread's CurrentUICulture property for all
        ///    resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The argument must not be empty..
        /// </summary>
        public static string ArgumentMustNotBeEmpty {
            get {
                return ResourceManager.GetString("ArgumentMustNotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The certificate store &quot;{0}&quot; was not found..
        /// </summary>
        public static string CertificateStoreNotFound {
            get {
                return ResourceManager.GetString("CertificateStoreNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The argument is not valid base64-encoded text..
        /// </summary>
        public static string InvalidBase64Text {
            get {
                return ResourceManager.GetString("InvalidBase64Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The certificate password is incorrect..
        /// </summary>
        public static string InvalidCertificatePassword {
            get {
                return ResourceManager.GetString("InvalidCertificatePassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The ContentDigest data is invalid..
        /// </summary>
        public static string InvalidContentDigest {
            get {
                return ResourceManager.GetString("InvalidContentDigest", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The ContentDigest&apos;s digest algorithm is invalid..
        /// </summary>
        public static string InvalidContentDigestAlgorithm {
            get {
                return ResourceManager.GetString("InvalidContentDigestAlgorithm", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The file identifier in the detached signature file name must be a period followed by one or more word characters with a total length not exceeding 32 characters..
        /// </summary>
        public static string InvalidDetachedSignatureFileIdentifier {
            get {
                return ResourceManager.GetString("InvalidDetachedSignatureFileIdentifier", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The hash algorithm name is invalid..
        /// </summary>
        public static string InvalidHashAlgorithmName {
            get {
                return ResourceManager.GetString("InvalidHashAlgorithmName", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The label is invalid..
        /// </summary>
        public static string InvalidLabel {
            get {
                return ResourceManager.GetString("InvalidLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The package identity is invalid..
        /// </summary>
        public static string InvalidPackageIdentity {
            get {
                return ResourceManager.GetString("InvalidPackageIdentity", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to PEM decoding failed.  The base64 data is invalid..
        /// </summary>
        public static string InvalidPemEncodedTextInvalidBase64Data {
            get {
                return ResourceManager.GetString("InvalidPemEncodedTextInvalidBase64Data", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to PEM decoding failed.  A pre-encapsulation boundary is invalid..
        /// </summary>
        public static string InvalidPemEncodedTextInvalidPreEncapsulationBoundary {
            get {
                return ResourceManager.GetString("InvalidPemEncodedTextInvalidPreEncapsulationBoundary", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to PEM decoding failed.  Matching pre- and post-encapsulation boundaries have mismatched labels..
        /// </summary>
        public static string InvalidPemEncodedTextMismatchedEncapsulationBoundaryLabel {
            get {
                return ResourceManager.GetString("InvalidPemEncodedTextMismatchedEncapsulationBoundaryLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to PEM decoding failed.  A post-encapsulation boundary was not found..
        /// </summary>
        public static string InvalidPemEncodedTextPostEncapsulationBoundaryNotFound {
            get {
                return ResourceManager.GetString("InvalidPemEncodedTextPostEncapsulationBoundaryNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The public key hash is invalid..
        /// </summary>
        public static string InvalidPublicKeyHash {
            get {
                return ResourceManager.GetString("InvalidPublicKeyHash", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The signature&apos;s content type was unexpected..
        /// </summary>
        public static string InvalidSignatureContentType {
            get {
                return ResourceManager.GetString("InvalidSignatureContentType", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Detached signature files must have the file extension &quot;.sig&quot;..
        /// </summary>
        public static string InvalidSignatureFileExtension {
            get {
                return ResourceManager.GetString("InvalidSignatureFileExtension", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The SignatureTarget data is invalid..
        /// </summary>
        public static string InvalidSignatureTarget {
            get {
                return ResourceManager.GetString("InvalidSignatureTarget", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The SignatureTargets data is invalid..
        /// </summary>
        public static string InvalidSignatureTargets {
            get {
                return ResourceManager.GetString("InvalidSignatureTargets", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The SignatureTargets&apos; version is invalid..
        /// </summary>
        public static string InvalidSignatureTargetsVersion {
            get {
                return ResourceManager.GetString("InvalidSignatureTargetsVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The SignatureTarget&apos;s version is invalid..
        /// </summary>
        public static string InvalidSignatureTargetVersion {
            get {
                return ResourceManager.GetString("InvalidSignatureTargetVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Invalid SignedData CMS message.  Exactly one signer is required..
        /// </summary>
        public static string InvalidSignedDataMessage {
            get {
                return ResourceManager.GetString("InvalidSignedDataMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The argument is not a valid signer identity string..
        /// </summary>
        public static string InvalidSignerIdentityString {
            get {
                return ResourceManager.GetString("InvalidSignerIdentityString", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The certificate is invalid for package signing..
        /// </summary>
        public static string InvalidSigningCertificate {
            get {
                return ResourceManager.GetString("InvalidSigningCertificate", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The detached signature file name must begin with the file name of the associated package..
        /// </summary>
        public static string SignatureFileNameMustStartWithPackageFileName {
            get {
                return ResourceManager.GetString("SignatureFileNameMustStartWithPackageFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Unable to build the timestamp certificate chain..
        /// </summary>
        public static string UnableToBuildTimestampCertificateChain {
            get {
                return ResourceManager.GetString("UnableToBuildTimestampCertificateChain", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The URI scheme &quot;{0}&quot; is unsupported..
        /// </summary>
        public static string UnsupportedUriScheme {
            get {
                return ResourceManager.GetString("UnsupportedUriScheme", resourceCulture);
            }
        }
    }
}