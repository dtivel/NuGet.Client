// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Signing;

namespace NuGet.CommandLine.Commands
{
    [Command(typeof(NuGetCommand), "request", "SignRequestCommandDescription", MinArgs = 1, MaxArgs = 1, UsageSummaryResourceName = "SignRequestCommandUsageSummary",
            UsageDescriptionResourceName = "SignRequestCommandUsageDescription", UsageExampleResourceName = "SignRequestCommandUsageExamples")]
    public class SignRequestCommand : Command
    {
        [Option(typeof(NuGetCommand), "SignRequestCommandFileIdentifierDescription")]
        public string FileIdentifier { get; set; }

        public override async Task ExecuteCommandAsync()
        {
            if (Arguments.Count != 1)
            {
                HelpCommand.ViewHelpForCommand(CommandAttribute.CommandName);

                return;
            }

            string fileIdentifier = null;

            if (FileIdentifier != null)
            {
                fileIdentifier = $".{FileIdentifier}";

                if (!DetachedSignatureFileName.IsValidFileIdentifier(fileIdentifier))
                {
                    HelpCommand.ViewHelpForCommand(CommandAttribute.CommandName);

                    return;
                }
            }

            var packagePath = Arguments[0];

            await RequestAsync(packagePath, fileIdentifier, CancellationToken.None);
        }

        private async Task RequestAsync(string packagePath, string fileIdentifier, CancellationToken cancellationToken)
        {
            var digest = await SigningUtilities.ComputeDigestAsync(packagePath, cancellationToken);
            var request = FileSigningRequest.Create(digest);
            var requestFile = GetSignatureRequestFile(packagePath, fileIdentifier);

            using (var stream = new StreamWriter(requestFile.FullName))
            {
                await request.WriteAsync(stream, cancellationToken);
            }
        }

        private static FileInfo GetSignatureRequestFile(string packagePath, string fileIdentifier)
        {
            var packageFileName = Path.GetFileName(packagePath);
            var directoryName = Path.GetDirectoryName(packagePath);

            return new FileInfo(Path.Combine(directoryName, $"{packageFileName}{fileIdentifier ?? ""}.req"));
        }
    }
}