using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat
{
    public class Client
    {
        private Socket clientSocket;
        private byte[] buffer = new byte[2048];

        /// <summary>
        /// Starts a client which connects to the server
        /// </summary>
        public void StartClient()
        {
            clientSocket = new Socket(Program.ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            while (!clientSocket.Connected)
            {
                try
                {
                    clientSocket.Connect(IPAddress.Loopback, 666);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine("Client connected");

            MessageLoop();
        }

        /// <summary>
        /// Starts a loop to send and receive messages
        /// </summary>
        private void MessageLoop()
        {
            while (true)
            {               
                ReceiveResponse();
                SendRequest();
            }
        }

        /// <summary>
        /// Receive messages from the server
        /// </summary>
        private void ReceiveResponse()
        {
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnMessageReceived), clientSocket);
        }

        private void OnMessageReceived(IAsyncResult ar)
        {
            clientSocket = (Socket)ar.AsyncState;
            int received = clientSocket.EndReceive(ar);
            if (received == 0) return;
            var databuffer = new byte[received];
            Array.Copy(buffer, databuffer, received);
            string text = Encoding.UTF8.GetString(databuffer);
            Console.WriteLine(text);
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnMessageReceived), clientSocket);
        }

        /// <summary>
        /// Sends messages to the server
        /// </summary>
        private void SendRequest()
        {
            Console.WriteLine("Enter a message: ");
            string message = Console.ReadLine();
            if (message == "e")
            {
                clientSocket.Close();
                Environment.Exit(0);
            }
            SendMessage(clientSocket, message);
        }

        /// <summary>
        /// Send message via a socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        private void SendMessage(Socket socket, string message)
        {
            var byteData = Encoding.UTF8.GetBytes(message);
            socket.Send(byteData);
        }
    }
}
