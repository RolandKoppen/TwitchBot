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
        public void ModeratorStatus(string Username, string ModeratorStatus)
        {
            Debug.WriteDebug("Moderator Status Change: " + ModeratorStatus + " for Username: " + Username);
            if (ModeratorStatus == "+o")
            {
                s_Moderators.Add(Username);
            }
            else if (ModeratorStatus == "-o")
            {
                s_Moderators.Remove(Username);
            }
        }

        // See if Username is a Moderator
        public bool IsModerator(string Username)
        {
            // We need to not rely on +o -o for mod status. We need to call /mods and parse that way for each channel joined.
            // Channel joined should be an array we could foreach/for with
            if (s_Moderators.Contains(Username))
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
    }
}
