using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIMLbot; // Used for the AIMLBot

namespace TwitchBot
{
    class ChannelFilter
    {
        // Enable Debugging
        Debugging Debug = new Debugging("ChannelFilter.txt");

        // Enable AIMLBot
        Bot AIMLBot = new Bot();

        // Constructor
        public ChannelFilter()
        {
            // New AIML Part // myUser??
            AIMLBot.loadSettings();
            AIMLBot.loadAIMLFromFiles();
            AIMLBot.isAcceptingUserInput = true;
        }

        // Destructor
        ~ChannelFilter()
        {
        }

        // If Message is all CAPS
        public bool ContainsCaps(string Username, string Message)
        {
            for (int i = 0; i < Message.Length; i++)
            {
                if (!Char.IsUpper(Message[i]))
                    return false;
            }
            Debug.WriteDebug("ChannelFilter: ContainsCaps > Username: " + Username + " Message: " + Message);
            return true;
        }

        // Returns true if the Message contains HTTP://
        public bool ContainsURL(string Username, string Message)
        {
            if (Message.Contains("http://") == true)
            {
                Debug.WriteDebug("ChannelFilter: ContainsURL > Username: " + Username + " Message: " + Message);
                return true;
            }
            else
            {
                return false;
            }
        }

        // Returns true if the Message contains the Bot Username
        public string ContainsTwitchUsername(string Username, string TwitchUsername, string Message)
        {
            if (Message.Contains(TwitchUsername) == true)
            {
                Debug.WriteDebug("ChannelFilter: ContainsTwitchUsername > Username: " + Username + " Message: " + Message);

                string StrippedMessageFromTwitchUsername = Message.Replace(TwitchUsername, "");

                //if (StrippedMessageFromTwitchUsername.Contains(", "))
                //{
                //    StrippedMessageFromTwitchUsername = StrippedMessageFromTwitchUsername.Replace(", ", "");
                //}
                //StrippedMessageFromTwitchUsername = TwitchUsername + ", " + StrippedMessageFromTwitchUsername;

                User AIMLUsername = new User(Username, AIMLBot);
                Request r = new Request(StrippedMessageFromTwitchUsername, AIMLUsername, AIMLBot);
                Result res = AIMLBot.Chat(r);
                return res.Output;
            }
            else
            {
                return "";
            }
        }
    }
}
