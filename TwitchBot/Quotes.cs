using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // Required For Input/Output to Files (StreamWriter)

namespace TwitchBot
{
    class Quotes
    {
        // Constructor
        public Quotes()
        {
        }

        // Destructor
        ~Quotes()
        {
        }
        // random quote return
        public string RandomQuote(string TwitchChannel)
        {
            try
            {
                string[] Quotes = System.IO.File.ReadAllLines("Quotes_" + TwitchChannel + ".txt");
                string Quote = Quotes[new Random().Next(0, Quotes.Length)];
                return Quote;
            }
            catch (System.IO.FileNotFoundException)
            {
                return "Error: Quotes_" + TwitchChannel + ".txt was not found!";
            }
        }

        public int CountQuotes(string TwitchChannel)
        {
            try
            {
                string[] Quotes = System.IO.File.ReadAllLines("Quotes_" + TwitchChannel + ".txt");
                return Quotes.Length;
            }
                catch (System.IO.FileNotFoundException)
            {
                return -1;
            }        
        }

        public void WriteQuote(string Quote, string Username, string TwitchChannel)
        {
            using (StreamWriter outputFile = new StreamWriter("Quotes_" + TwitchChannel + ".txt", true, Encoding.UTF8))
            {
                outputFile.WriteLine("'" + Quote + "', added by " + Username + " on " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            }
        }
    }
}
