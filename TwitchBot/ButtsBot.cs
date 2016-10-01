using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class ButtsBot
    {

        // Enable Debugging
        Debugging Debug = new Debugging("ButtsBot.txt");

        public string ButtsBotName = "ButtsBot";
        string[] ButtsBotReplies = {
            "Kappa",
            "NotLikeThis",
            "FeelsAmazingMan",
            "FeelsGoodMan",
            "FeelsBadMan"
        };

        // Constructor
        public ButtsBot()
        {

        }

        // Destructor
        ~ButtsBot()
        {

        }

        public string ButtsBotReply()
        {
            // Randomly Pick A Line
            string ButtsBotReply = ButtsBotReplies[new Random().Next(0, ButtsBotReplies.Length)];
            Debug.WriteDebug("ButtsBot > Replying with: " + ButtsBotReply);
            return ButtsBotReply;
        }
    }
}
