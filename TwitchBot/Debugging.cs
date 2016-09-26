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
        // File Writer
        System.IO.StreamWriter sw_Debugging;

        // Constructor
        public Debugging(string LogFile)
        {
            sw_Debugging = new System.IO.StreamWriter(new FileStream(LogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite), Encoding.UTF8);  // Also use UTF8
        }

        // Destructor
        ~Debugging()
        {
            
        }

        // Write Debug Logging to Console and The Specified LogFile
        public void WriteDebug(string DebugMessage)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] " + DebugMessage);
            sw_Debugging.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] " + DebugMessage);
            sw_Debugging.Flush();
        }
    }
}
