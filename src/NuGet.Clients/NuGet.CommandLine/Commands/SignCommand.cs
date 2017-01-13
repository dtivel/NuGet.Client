// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Signing;

namespace NuGet.CommandLine.Commands
{
    [Command(typeof(NuGetCommand), "sign", "SignCommandDescription", MinArgs = 1, MaxArgs = 2, UsageSummaryResourceName = "SignCommandUsageSummary",
            UsageDescriptionResourceName = "SignCommandUsageDescription", UsageExampleResourceName = "SignCommandUsageExamples")]
    public class SignCommand : Command
    {
        [Option(typeof(NuGetCommand), "SignCommandFileIdentifierDescription")]
        public string FileIdentifier { get; set; }

        [Option(typeof(NuGetCommand), "SignCommandRequestPathDescription")]
        public string RequestPath { get; set; }

        [Option(typeof(NuGetCommand), "SignCommandSecretPathDescription")]
        public string SecretPath { get; set; }

        [Option(typeof(NuGetCommand), "SignCommandSecretPassphraseDescription")]
        public string SecretPassphrase { get; set; }

        [Option(typeof(NuGetCommand), "SignCommandStoreNameDescription")]
        public string StoreName { get; set; }

        [Option(typeof(NuGetCommand), "SignCommandSubjectNameDescription")]
        public string SubjectName { get; set; }

        [Option(typeof(NuGetCommand), "SignCommandFingerprintDescription")]
        public string Fingerprint { get; set; }

        [Option(typeof(NuGetCommand), "SignCommandIsLocalMachineStoreDescription")]
        public bool IsLocalMachineStore { get; set; }

        public override async Task ExecuteCommandAsync()
        {
            if (Arguments.Count < 1 || Arguments.Count > 2)
            {
                HelpCommand.ViewHelpForCommand(CommandAttribute.CommandName);

                return;
            }

            // Do not prompt for a passphrase if no other information about a certificate was provided.
            if (string.IsNullOrEmpty(SecretPath) &&
                string.IsNullOrEmpty(StoreName) &&
                string.IsNullOrEmpty(SubjectName) &&
                string.IsNullOrEmpty(Fingerprint))
            {
                HelpCommand.ViewHelpForCommand(CommandAttribute.CommandName);

                return;
            }

            var fileIdentifier = string.IsNullOrEmpty(FileIdentifier) ? null : $".{FileIdentifier}";

            if (!DetachedSignatureFileName.IsValidFileIdentifier(fileIdentifier))
            {
                HelpCommand.ViewHelpForCommand(CommandAttribute.CommandName);

                return;
            }

            X509Certificate2 certificate;

            using (var options = new CertificateFindOptions()
            {
                SecretPath = SecretPath,
                SecretPassphrase = GetSecretPassphrase(),
                StoreName = StoreName,
                IsLocalMachineStore = IsLocalMachineStore,
                SubjectName = SubjectName,
                Fingerprint = Fingerprint
            })
            {
                if (!TryFindCertificate(options, out certificate))
                {
                    return;
                }
            }

            var packagePath = Arguments[0];
            var timestampAuthorityUri = (Arguments.Count != 2 || string.IsNullOrEmpty(Arguments[1])) ? null : new Uri(Arguments[1]);

            await SignAndTimestampAsync(packagePath, fileIdentifier, certificate, timestampAuthorityUri, CancellationToken.None);
        }

        private async Task SignAndTimestampAsync(string packagePath, string fileIdentifier, X509Certificate2 certificate, Uri timestampAuthorityUri, CancellationToken cancellationToken)
        {
            var signer = new Signer(certificate);
            var signature = await signer.SignAsync(packagePath, cancellationToken);
            var timestamper = new Timestamper(timestampAuthorityUri);
            var timestampedSignature = await timestamper.TimestampAsync(signature, cancellationToken);
            var signatureFile = GetDetachedSignatureFile(packagePath, fileIdentifier);

            using (var writer = new StreamWriter(signatureFile.FullName))
            {
                await PemEncoder.EncodeAsync(timestampedSignature, writer, cancellationToken);
            }
        }

        private bool TryFindCertificate(CertificateFindOptions options, out X509Certificate2 certificate)
        {
            certificate = null;

            var certificates = CertificateFinder.FindCertificates(options);

            if (certificates.Count == 0)
            {
                Console.WriteError(LocalizedResourceManager.GetString("SignCommandNoCertificateFound"));

                return false;
            }

            if (certificates.Count > 1)
            {
                Console.WriteError(LocalizedResourceManager.GetString("SignCommandMultipleCertificatesFound"));

                foreach (var cert in certificates)
                {
                    Console.WriteLine("    Issued to:   " + cert.SubjectName.Name);
                    Console.WriteLine("    Issued by:   " + cert.IssuerName.Name);
                    Console.WriteLine("    Expires:     " + cert.NotAfter.ToString("ddd MMM dd HH:mm:ss yyyy"));
                    Console.WriteLine("    Fingerprint: " + cert.Thumbprint);
                    Console.WriteLine();
                }

                return false;
            }

            certificate = certificates[0];

            return true;
        }

        private SecureString GetSecretPassphrase()
        {
            var secretPassphrase = new SecureString();

            if (!string.IsNullOrEmpty(SecretPassphrase))
            {
                foreach (var @char in SecretPassphrase)
                {
                    secretPassphrase.AppendChar(@char);
                }

                SecretPassphrase = null;

                return secretPassphrase;
            }

            if (!Console.IsNonInteractive)
            {
                Console.Write(LocalizedResourceManager.GetString("Credentials_Password"));

                Console.ReadSecureString(secretPassphrase);

                return secretPassphrase;
            }

            return null;
        }

        private static FileInfo GetDetachedSignatureFile(string packagePath, string fileIdentifier)
        {
            var packageFileName = Path.GetFileName(packagePath);
            var directoryName = Path.GetDirectoryName(packagePath);

            return new FileInfo(Path.Combine(directoryName, $"{packageFileName}{fileIdentifier ?? ""}.sig"));
        }
    }
}