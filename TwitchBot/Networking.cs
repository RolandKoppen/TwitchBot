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

        // Enable Moderators
        Moderators m_Moderators = new Moderators();

        // Enable ButtsBot
        ButtsBot b_ButtsBot = new ButtsBot();

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
                    SendMessage("PASS " + s_TwitchToken);
                    SendMessage("NICK " + s_TwitchUsername);
                    SendMessage("CAP REQ :twitch.tv/membership"); // Shows JOIN/PART/UMODES
                    SendMessage("CAP REQ :twitch.tv/commands"); // Activate Commands such as WHISPER/HOST/BAN/PERMABAN/DONATION/MODS
                    SendMessage("JOIN " + s_TwitchChannel);
                    string MyUsername = ":" + s_TwitchUsername + "!" + s_TwitchUsername + "@" + s_TwitchUsername + ".tmi.twitch.tv"; // Build MyUsername for easy access
                    RefreshModsList(MyUsername);
                    // Let channel know that its connected
                    //SendMessage(ns_Client, ":" + s_TwitchUsername + "!" + s_TwitchUsername + "@" + s_TwitchUsername + ".tmi.twitch.tv PRIVMSG " + s_TwitchChannel + " :Connected ...");

                    while (b_IsConnected == true)
                    {
                        // We need to try reading Per Line and not in a stream of 1024 data. We need to analyze each line speratly. Thus we need to stop using \r\n and or splitting methods.
                        // Try new approach

                        using (StreamReader Reader = new StreamReader(t_Client.GetStream(), Encoding.UTF8))
                        {
                            string Message;
                            // Try/Catch, Exception when !bye is used
                            while ((Message = Reader.ReadLine()) != null) // Read untill the socket dies?
                            {
                                Debug.WriteDebug("Networking > Connect > Recieved: " + Message);
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
                SendMessage("PONG " + SplitMessage[1]);
            }
            // Twitch Notice
            else if (SplitMessage[0] == ":tmi.twitch.tv" && SplitMessage[1] == "NOTICE")
            {
                string PrivateMessage = BuildPrivateMessage(SplitMessage);

                if (PrivateMessage.Contains("The moderators of this room are:"))
                {
                    PrivateMessage = PrivateMessage.Remove(0, 33);
                    string[] SplitModerators = PrivateMessage.Split(' ');
                    for (int i = 0; i < SplitModerators.Length; i++)
                    {
                        if (SplitModerators[i][SplitModerators[i].Length - 1] == ',')
                        {
                            SplitModerators[i] = SplitModerators[i].Remove(SplitModerators[i].Length - 1, 1);
                        }
                        m_Moderators.ModeratorStatus(SplitModerators[i]);
                    }
                }
            }
            // Twitch HostTarget
            else if (SplitMessage[0] == ":tmi.twitch.tv" && SplitMessage[1] == "HOSTTARGET")
            {
                string MyUsername = ":" + s_TwitchUsername + "!" + s_TwitchUsername + "@" + s_TwitchUsername + ".tmi.twitch.tv"; // Build MyUsername for easy access

                SplitMessage[2] = SplitMessage[2].Remove(0, 1);
                SplitMessage[3] = SplitMessage[3].Remove(0, 1);

                if (SplitMessage[3].Contains("-"))
                {
                    SendChannelMessage(MyUsername, SplitMessage[2] + " has stopped the host. Welcome back everybody!");
                }
                else
                {
                    SendChannelMessage(MyUsername, SplitMessage[2] + " is now hosting " + SplitMessage[3] + " for a total of " + SplitMessage[4] + " viewers. You can also go directly to this channel and join the chat by clicking this: https://www.twitch.tv/" + SplitMessage[3]);
                }
            }
            // Username Whisper
            else if (SplitMessage[1] == "WHISPER")
            {
                string[] Username = SplitMessage[0].Split('!');
                Username[0] = Username[0].Remove(0, 1); // Remove first : character from Username
                string MyUsername = ":" + s_TwitchUsername + "!" + s_TwitchUsername + "@" + s_TwitchUsername + ".tmi.twitch.tv"; // Build MyUsername for easy access
                string PrivateMessage = BuildPrivateMessage(SplitMessage);
                SendUsernameWhisper(MyUsername, Username[0], "You send me: " + PrivateMessage);

                string[] MySplitPrivateMessage = PrivateMessage.Split(' ');
                if (MySplitPrivateMessage[0] == "!mods" && m_Moderators.IsModerator(Username[0]))
                {
                    SendUsernameWhisper(MyUsername, Username[0], "The output is: " + m_Moderators.TotalModerators() + " Moderators: " + m_Moderators.ReturnModerators());
                }
                // Depricated for now, focus on single channel first
                //else if (MySplitPrivateMessage[0] == "!join" && m_Moderators.IsModerator(Username[0]))
                //{
                //    SendUsernameWhisper(MyUsername, Username[0], "Trying to join " + MySplitPrivateMessage[1]);
                //    JoinChannel(MyUsername, MySplitPrivateMessage[1]);
                //}
                //else if (MySplitPrivateMessage[0] == "!part" && m_Moderators.IsModerator(Username[0]))
                //{
                //    SendUsernameWhisper(MyUsername, Username[0], "Trying to part " + MySplitPrivateMessage[1]);
                //    PartChannel(MyUsername, MySplitPrivateMessage[1]);
                //}
                else if (MySplitPrivateMessage[0] == "!amiamod")
                {
                    SendUsernameWhisper(MyUsername, Username[0], "The output is: " + m_Moderators.IsModerator(Username[0]));
                }
            }
            // Channel Message
            else if (SplitMessage[1] == "PRIVMSG")
            {
                string[] Username = SplitMessage[0].Split('!');
                Username[0] = Username[0].Remove(0, 1); // Remove first : character from Username
                string MyUsername = ":" + s_TwitchUsername + "!" + s_TwitchUsername + "@" + s_TwitchUsername + ".tmi.twitch.tv"; // Build MyUsername for easy access
                string PrivateMessage = BuildPrivateMessage(SplitMessage);
                string[] MySplitPrivateMessage = PrivateMessage.Split(' ');

                if (MySplitPrivateMessage[0] == "!flip" && m_Moderators.IsModerator(Username[0]))
                {
                    SendChannelMessage(MyUsername, "┌∩┐( ಠ益ಠ )┌∩┐");
                }

                cf_ChannelFilter.ContainsCaps(Username[0], PrivateMessage);
                cf_ChannelFilter.ContainsURL(Username[0], PrivateMessage);
                cf_ChannelFilter.ContainsTwitchUsername(Username[0], s_TwitchUsername, PrivateMessage);

                // ButtsBot Part
                if (Username[0] == b_ButtsBot.ButtsBotName.ToLower()) // Always cast ToLower?
                {
                    Debug.WriteDebug("Networking > ButtsBot > ButtsBot said: " + PrivateMessage);
                    if (p_Probability.ProbabilityPercentage(25) == true)
                    {
                        SendChannelMessage(MyUsername, b_ButtsBot.ButtsBotReply());
                    }
                }
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
        public void SendMessage(string Message)
        {
            if (b_IsConnected == false)
            {
                Debug.WriteDebug("Networking > Connect > You are not connected");
            }
            else if (b_IsConnected == true)
            {
                Debug.WriteDebug("Networking > Connect > Sent: " + Message);
                byte[] b_Message = System.Text.Encoding.UTF8.GetBytes(Message + "\r\n"); // Encode UTF8
                ns_Client.Write(b_Message, 0, b_Message.Length);
            }
        }

        // Return the Full Private Message String
        public string BuildPrivateMessage(string[] SplitMessage)
        {
            string PrivateMessage = null;
            for (int i = 3; i < SplitMessage.Length; i++) // Build whole sentance from PRIVMSG
            {
                if (i == 3)
                {
                    PrivateMessage += SplitMessage[i];
                    PrivateMessage = PrivateMessage.Remove(0, 1); // Remove first : character (":username!username@username.tmi.twitch.tv PRIVMSG #somechannel :some message Kappa")
                }
                else
                {
                    PrivateMessage += " " + SplitMessage[i];
                }
            }
            return PrivateMessage;
        }

        // Build Mod List Without Relying On +o -o
        public void RefreshModsList(string MyUsername)
        {
            SendMessage(MyUsername + " PRIVMSG " + s_TwitchChannel + " :/MODS");
        }

        // Send a Message to a Channel
        public void SendChannelMessage(string MyUsername, string Message)
        {
            SendMessage(MyUsername + " PRIVMSG " + s_TwitchChannel + " :" + Message);
        }

        // Send a Whisper to a Username
        public void SendUsernameWhisper(string MyUsername, string Username, string Message)
        {
            SendMessage(MyUsername + " PRIVMSG #jtv :/w " + Username + " " + Message);
        }

        // Join Channel
        public void JoinChannel(string MyUsername, string Channel)
        {
            SendMessage(MyUsername + " JOIN " + Channel);
        }

        // Part Channel
        public void PartChannel(string MyUsername, string Channel)
        {
            SendMessage(MyUsername + " PART " + Channel);
        }
    }
}
