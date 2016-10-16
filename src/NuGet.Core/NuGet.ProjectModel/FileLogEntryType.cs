using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.ProjectModel
{
    public enum FileLogEntryType : ushort
    {
        None = 0,
        Error = 1,
        Warning = 2
    }
}
