using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class Probability
    {
        // Identify Random Number Generator
        Random Rand;

        // Constructor
        public Probability()
        {
            // Initialize Random Number Generator
            Rand = new Random();
        }

        // Destructor
        ~Probability()
        {

        }

        // Returns true wether you get the drop rate/outcome. With Percent 25 it is 1/4 to become true
        public bool ProbabilityPercentage(int Percent)
        {
            if (Rand.Next(1,101) <= Percent)
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
