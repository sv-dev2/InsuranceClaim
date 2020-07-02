using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SV.Domain.Code
{
    
    // enumeration of log severity levels

    public enum LogSeverity
    {
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5
    }
}
