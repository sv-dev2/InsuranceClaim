
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SV.Domain.Code
{
    
    // writes log events to the diagnostic trace
    // GoF Design Pattern: Observer

    public class ObserverLogToStdOut : ILog
    {
        public void Log(object sender, LogEventArgs e)
        {
            // example code of entering a log event to output console

            string message = "[" + e.Date.ToString() + "] " +
                e.SeverityString + ": " + e.Message;

            // writes message to debug output window

            Console.WriteLine(message); 
        }
    }
}
