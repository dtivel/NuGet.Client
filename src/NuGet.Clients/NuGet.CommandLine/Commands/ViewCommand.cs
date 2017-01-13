// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.CommandLine.Commands
{
    [Command(typeof(NuGetCommand), "view", "ViewCommandDescription", MinArgs = 1, MaxArgs = 1, UsageSummaryResourceName = "ViewCommandUsageSummary",
            UsageDescriptionResourceName = "ViewCommandUsageDescription", UsageExampleResourceName = "ViewCommandUsageExamples")]
    public class ViewCommand : Command
    {
        private readonly List<string> _signatures = new List<string>();

        [Option(typeof(NuGetCommand), "ViewCommandSignatureDescription")]
        public ICollection<string> Signature
        {
            get { return _signatures; }
        }

        public override async Task ExecuteCommandAsync()
        {
            if (Arguments.Count != 1)
            {
                HelpCommand.ViewHelpForCommand(CommandAttribute.CommandName);

                return;
            }

            var packagePath = Arguments[0];

            await ViewAsync(packagePath, CancellationToken.None);
        }

        private async Task ViewAsync(string packagePath, CancellationToken cancellationToken)
        {
            if (Signature.Count == 0)
            {
                return;
            }

            await Task.FromResult(true);
        }
    }
}