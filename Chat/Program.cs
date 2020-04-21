using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Chat
{
    public class Program
    {
        public static List<Socket> clientSockets = new List<Socket>();
        public static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

        /// <summary>
        /// Start method of the CLI-application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Server server = new Server();
            if (!server.ServerIsRunning())
            {
                server.SetupServer();
                Console.ReadLine();
            }
            else
            {
                Client client = new Client();
                client.StartClient();               
            }            
        }
    }
}
