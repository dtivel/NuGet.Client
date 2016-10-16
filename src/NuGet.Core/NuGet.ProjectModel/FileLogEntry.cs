using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.ProjectModel
{
    public class FileLogEntry
    {
        public FileLogEntryType Type { get; }

        public string Message { get; }

        public FileLogEntry(FileLogEntryType type, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Type = type;
            Message = message;
        }
    }
}
