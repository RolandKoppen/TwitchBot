using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // Required For Input/Output to Files (StreamWriter)

namespace TwitchBot
{
    class Debugging
    {
        string s_DebugFile;

        // Constructor
        public Debugging(string LogFile)
        {
            s_DebugFile = LogFile;
        }

        // Destructor
        ~Debugging()
        {          
        }

        // Write Debug Logging to Console and The Specified LogFile
        public void WriteDebug(string DebugMessage)
        {
            using (StreamWriter outputFile = new StreamWriter(s_DebugFile, true, Encoding.UTF8))
            {
                Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] " + DebugMessage);
                outputFile.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] " + DebugMessage);
            }
        }
    }
}
