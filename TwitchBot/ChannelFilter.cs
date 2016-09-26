using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class ChannelFilter
    {
        // Constructor
        public ChannelFilter()
        {

        }

        // Destructor
        ~ChannelFilter()
        {

        }

        // If Message is all CAPS
        public bool ContainsCaps (string Message)
        {
            for (int i = 0; i < Message.Length; i++)
            {
                if (!Char.IsUpper(Message[i]))
                    return false;
            }

            return true;
        }

        // Returns true if the Message contains HTTP://
        public bool ContainsURL (string Message)
        {
            if (Message.Contains("http://") == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
