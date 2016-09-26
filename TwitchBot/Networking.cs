using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets; // Used for Networking
using System.Threading; // Used for Reconnect Timer
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
                    SendMessage(t_Client, ns_Client, ":" + s_TwitchUsername + "!" + s_TwitchUsername + "@" + s_TwitchUsername + ".tmi.twitch.tv PRIVMSG " + s_TwitchChannel + " :Connected ...");

                    while (b_IsConnected == true)
                    {
                        if (ns_Client.CanRead)
                        {
                            byte[] myReadBuffer = new byte[1024];
                            StringBuilder myCompleteMessage = new StringBuilder();
                            int numberOfBytesRead = 0;
                            // Incoming message may be larger than the buffer size.
                            do
                            {
                                numberOfBytesRead = ns_Client.Read(myReadBuffer, 0, myReadBuffer.Length);
                                myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead)); // Decode UTF8
                                myCompleteMessage.ToString().TrimEnd(new char[] { '\r', '\n' });
                            }
                            while (ns_Client.DataAvailable);

                            // Log the recieved message
                            myCompleteMessage.Replace("\r\n", "");

                            if (myCompleteMessage.Length == 0)
                            {
                                Debug.WriteDebug("Networking > Connect > Recieved Empty Message");
                                return; // Exit looping/non existant socket, or some bug (yet)
                            }

                            // Split and Parse message
                            string[] myCompleteSplitMessage = myCompleteMessage.ToString().Split(' ');

                            Debug.WriteDebug("Networking > Connect > Recieved: " + myCompleteMessage);

                            // Twitch Ping Pong (KEEP-ALIVE)
                            if (myCompleteSplitMessage[0] == "PING" && myCompleteSplitMessage[1].Length > 0)
                            {
                                SendMessage(t_Client, ns_Client, "PONG " + myCompleteSplitMessage[1]);
                            }
                        }
                        else
                        {
                            Debug.WriteDebug("Networking > Connect > You cannot read from this NetworkStream");
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
