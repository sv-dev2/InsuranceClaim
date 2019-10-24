using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace SV.Domain.Code
{
    
    // writes log events to a local file.
    // ** Design Pattern: Observer

    public class ObserverLogToFile : ILog
    {
        private string fileName;

        public ObserverLogToFile(string fileName)
        {
            this.fileName = fileName;
        }

        // write a log request to a file

        public void Log(object sender, LogEventArgs e)
        {
            string message = "[" + e.Date.ToString() + "] " +
                e.SeverityString + ": " + e.Message;
            
            FileStream fileStream;

            // create directory, if necessary
			int tries = 3;
			fileStream = null;
			while(tries > 0)
			{
				try
				{
					fileStream = new FileStream(fileName, FileMode.Append);
					tries = 0;
				}
				catch(DirectoryNotFoundException)
				{
					Directory.CreateDirectory((new FileInfo(fileName)).DirectoryName);
				}
				catch(IOException ex)
				{
					throw ex;
				}
				catch(Exception ex)
				{
					Thread.Sleep(1);
					tries--;

					if(tries == 0)
						throw ex;
				}
			}

			// NOTE: be sure you have write privileges to folder
			if(fileStream != null)
			{
				var writer = new StreamWriter(fileStream);
				try
				{
					writer.WriteLine(message);
				}
				catch { /* do nothing */}
				finally
				{
					try
					{
						writer.Close();
					}
					catch { /* do nothing */}
				}
			}
        }
    }
}
