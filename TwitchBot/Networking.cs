using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets; // Used for Networking
using System.Threading; // Used for Reconnect Timer
using System.IO; // Used for StreamReader

namespace TwitchBot
{
    class Networking
    {
        // Twitch Networking Information
        string s_IRCServerAddress, s_TwitchUsername, s_TwitchToken, s_TwitchChannel, s_TwitchMaster;
        int i_IRCServerPort, i_IRCServerReconnectTime;
        bool b_IsConnected;

        // Netowkring Information
        TcpClient t_Client;
        NetworkStream ns_Client;

        // Enable Debugging
        Debugging Debug = new Debugging("Networking.txt");

        // Enable Probability
        Probability p_Probability = new Probability();

        // Enable ChannelFilter
        ChannelFilter cf_ChannelFilter = new ChannelFilter();

        // Constructor
        public Networking(string IRCServerAddress, int IRCServerPort, int IRCServerReconnectTime, string TwitchUsername, string TwitchToken, string TwitchChannel, string TwitchMaster)
        {
            s_IRCServerAddress = IRCServerAddress;
            Debug.WriteDebug("Networking > Setting IRC Server: " + s_IRCServerAddress);
            i_IRCServerPort = IRCServerPort;
            Debug.WriteDebug("Networking > Setting IRC Server Port: " + i_IRCServerPort);
            i_IRCServerReconnectTime = IRCServerReconnectTime;
            Debug.WriteDebug("Networking > Setting IRC Server Reconnect Time: " + TimeSpan.FromMilliseconds(i_IRCServerReconnectTime).TotalSeconds + " seconds");
            s_TwitchUsername = TwitchUsername;
            Debug.WriteDebug("Networking > Setting Twitch Username: " + s_TwitchUsername);
            s_TwitchToken = TwitchToken;
            Debug.WriteDebug("Networking > Setting Twitch Token: " + s_TwitchToken);
            s_TwitchChannel = TwitchChannel.ToLower();
            Debug.WriteDebug("Networking > Setting Twitch Channel (auto lowercase): " + s_TwitchChannel);
            s_TwitchMaster = TwitchMaster;
            Debug.WriteDebug("Networking > Setting Twitch Master: " + s_TwitchMaster);
        }

        // Destructor
        ~Networking()
        {

        }

        // Connect to the IRC/Twitch Server & Loop (Blocking Wise)
        public void Connect()
        {
            if (b_IsConnected == true)
            {
                Debug.WriteDebug("Networking > Connect > You are already connected");
            }
            else
            {
                try
                {
                    Debug.WriteDebug("Networking > Connect > Trying to connect to " + s_IRCServerAddress + " on port " + i_IRCServerPort);
                    t_Client = new TcpClient(s_IRCServerAddress, i_IRCServerPort);
                    ns_Client = t_Client.GetStream();
                    b_IsConnected = true; // Try/Catch

                    // Send First Messages to Authenticate (Basicly IRC RFC)
                    Debug.WriteDebug("Networking > Connect > Handshaking with Protocol");
                    SendMessage(t_Client, ns_Client, "PASS " + s_TwitchToken);
                    SendMessage(t_Client, ns_Client, "NICK " + s_TwitchUsername);
                    SendMessage(t_Client, ns_Client, "CAP REQ :twitch.tv/membership"); // Shows JOIN/PART/UMODES
                    SendMessage(t_Client, ns_Client, "CAP REQ :twitch.tv/commands"); // Activate Commands such as WHISPER/HOST/BAN/PERMABAN/DONATION/MODS
                    SendMessage(t_Client, ns_Client, "JOIN " + s_TwitchChannel);
                    // Let channel know that its connected
                    //SendMessage(t_Client, ns_Client, ":" + s_TwitchUsername + "!" + s_TwitchUsername + "@" + s_TwitchUsername + ".tmi.twitch.tv PRIVMSG " + s_TwitchChannel + " :Connected ..."); // We reconnect too often at the moment

                    while (b_IsConnected == true)
                    {
                        // We need to try reading Per Line and not in a stream of 1024 data. We need to analyze each line speratly. Thus we need to stop using \r\n and or splitting methods.
                        // Try new approach

                        using (StreamReader Reader = new StreamReader(t_Client.GetStream(), Encoding.UTF8))
                        {
                            string Message;
                            while ((Message = Reader.ReadLine()) != null) // Read untill the socket dies?
                            {
                                Debug.WriteDebug(Message);
                                Parse_Message(Message);
                            }
                        }
                    }
                }
                catch (SocketException se)
                {
                    Debug.WriteDebug("Networking > Connect > " + se.Message.ToString());
                    Debug.WriteDebug("Networking > Connect > Waiting " + TimeSpan.FromMilliseconds(i_IRCServerReconnectTime).TotalSeconds + " seconds");
                    Thread.Sleep(i_IRCServerReconnectTime);
                    Connect();
                }
            }
        }

        public void Parse_Message(string Message)
        {
            string[] SplitMessage = Message.Split(' ');

            // Twitch Ping Pong (KEEP-ALIVE)
            if (SplitMessage[0] == "PING")
            {
                SendMessage(t_Client, ns_Client, "PONG " + SplitMessage[1]);
            }
            // Channel Message
            else if (SplitMessage[1] == "PRIVMSG")
            {
                string[] Username = SplitMessage[0].Split('!');
                Username[0] = Username[0].Remove(0, 1); // Remove first : character from Username
                string PrivateMessage = null;
                for (int i = 3; i < SplitMessage.Length; i++) // Build whole sentance from PRIVMSG
                {
                    if (i == 3)
                    {
                        PrivateMessage += SplitMessage[i];
                        PrivateMessage.Remove(0, 1); // Remove first : character (":wimpflix98!wimpflix98@wimpflix98.tmi.twitch.tv PRIVMSG #summit1g :SourPls ANELE")
                    }
                    else
                    {
                        PrivateMessage += " " + SplitMessage[i];
                    }
                }

                cf_ChannelFilter.ContainsCaps(Username[0], PrivateMessage);
                cf_ChannelFilter.ContainsURL(Username[0], PrivateMessage);
                cf_ChannelFilter.ContainsTwitchUsername(Username[0], s_TwitchUsername, PrivateMessage);
            }
        }

        // Disconnect from IRC/Twitch Server
        public void Disconnect()
        {
            if (b_IsConnected == false)
            {
                Debug.WriteDebug("Networking > Connect > You are not connected");
            }
            else if (b_IsConnected == true)
            {
                Debug.WriteDebug("Networking > Connect > Disconnecting");
                ns_Client.Close();
                t_Client.Close();
                b_IsConnected = false;
            }
        }

        // Send a message through the socket to IRC/Twitch
        public void SendMessage(TcpClient Client, NetworkStream Client_Stream, string Message)
        {
            if (b_IsConnected == false)
            {
                Debug.WriteDebug("Networking > Connect > You are not connected");
            }
            else if (b_IsConnected == true)
            {
                Debug.WriteDebug("Networking > Connect > Sent: " + Message);
                byte[] b_Message = System.Text.Encoding.UTF8.GetBytes(Message + "\r\n"); // Encode UTF8
                Client_Stream.Write(b_Message, 0, b_Message.Length);
            }
        }
    }
}
