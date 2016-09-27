using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class ChannelFilter
    {
        // Enable Debugging
        Debugging Debug = new Debugging("ChannelFilter.txt");
        
        // Constructor
        public ChannelFilter()
        {

        }

        // Destructor
        ~ChannelFilter()
        {

        }

        // If Message is all CAPS
        public bool ContainsCaps (string Username, string Message)
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
        public bool ContainsTwitchUsername (string Username, string TwitchUsername, string Message)
        {
            if (Message.Contains(TwitchUsername) == true)
            {
                Debug.WriteDebug("ChannelFilter: ContainsTwitchUsername > Username: " + Username + " Message: " + Message);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
