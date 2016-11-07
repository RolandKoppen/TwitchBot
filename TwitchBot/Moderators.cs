using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class Moderators
    {
        List<string> s_Moderators = new List<string>();

        // Enable Debugging
        Debugging Debug = new Debugging("Moderators.txt");

        // Constructor
        public Moderators()
        {           
        }

        // Destructor
        ~Moderators()
        {
        }

        // Add or Remove Username from Moderator list
        public void ModeratorStatus(string TwitchChannel, string Username)
        {
            Debug.WriteDebug("Moderator Status Change for channel: " + TwitchChannel + " Username: " + Username);
            s_Moderators.Add(TwitchChannel + "," + Username);
        }

        // See if Username is a Moderator
        public bool IsModerator(string Username, string TwitchChannel)
        {
            // We need to not rely on +o -o for mod status. We need to call /mods and parse that way for each channel joined. (DONE)
            // Channel joined should be an array we could foreach/for with
            if (s_Moderators.Contains(TwitchChannel + "," + Username))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Get Total Moderators
        public int TotalModerators()
        {
            return s_Moderators.Count;
        }

        public string ReturnModerators()
        {
            string AllModerators = "";

            for (int i = 0;i < s_Moderators.Count; i++)
            {
                if (i < s_Moderators.Count - 1)
                {
                    AllModerators += s_Moderators[i] + ", ";
                }
                else
                {
                    AllModerators += s_Moderators[i];
                }
            }

            return AllModerators;
        }
    }
}
