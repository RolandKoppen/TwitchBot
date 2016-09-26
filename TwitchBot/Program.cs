using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class Program
    {
        static void Main(string[] args)
        {

            // Make sure we output to UTF8
            Console.OutputEncoding = Encoding.UTF8;

            // Initialize Debug Logging
            Debugging Debug = new Debugging("Logfile.txt");

            // Initialize Twitch Networking
            Networking Twitch = new Networking("irc.chat.twitch.tv", 6667, 30000, "testbot", "oath:293487298437912837", "#testbot", "mymaster");
            Twitch.Connect();
            Twitch.Disconnect();

            // Stop console from closing (Enter key)
            Console.ReadLine();
        }
    }
}
