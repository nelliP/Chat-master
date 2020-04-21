using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat
{
    public class Server
    {
        private Socket listenerSocket;
        private byte[] buffer;

        /// <summary>
        /// Checks if the server is already running
        /// </summary>
        /// <returns></returns>
        public bool ServerIsRunning()
        {
            using (var tcpClient = new TcpClient())
            {
                try
                {
                    var ipAddress = IPAddress.Parse("127.0.0.1");
                    tcpClient.Connect(ipAddress, 666);
                }
                catch (SocketException)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Setup a server socket which the clients can connect to
        /// </summary>
        public void SetupServer()
        {
            try
            {
                var port = 666;
                listenerSocket = new Socket(Program.ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenerSocket.Bind(new IPEndPoint(Program.ipAddress, port));
                listenerSocket.Listen(100);
                Console.WriteLine("Sever listening ...");

                listenerSocket.BeginAccept(new AsyncCallback(OnClientConnected), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// When a client connects to the server, the socket used by the client is connected on the server side
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnClientConnected(IAsyncResult asyncResult)
        {
            Socket clientSocket = listenerSocket.EndAccept(asyncResult);
            Program.clientSockets.Add(clientSocket);
            Console.WriteLine("Client connected ...");
            ReceiveMessage(clientSocket);

            listenerSocket.BeginAccept(new AsyncCallback(OnClientConnected), null);
        }

        /// <summary>
        /// Common method for how to start receiving messages
        /// </summary>
        /// <param name="socket"></param>
        private void ReceiveMessage(Socket clientSocket)
        {
            buffer = new byte[clientSocket.ReceiveBufferSize]; //new byte[2048];
            clientSocket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(OnMessageReceived), clientSocket);
        }

        /// <summary>
        /// When a new message is received, this method is triggered to extract the message
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnMessageReceived(IAsyncResult asyncResult)
        {
            var clientSocket = (Socket)asyncResult.AsyncState;
            int messageLength = clientSocket.EndReceive(asyncResult);
            var buff = new byte[messageLength]; // new byte[2048];
            Array.Copy(buffer, buff, messageLength);
            var message = Encoding.UTF8.GetString(buff, 0, messageLength);

            if (!String.IsNullOrEmpty(message))
            {
                Console.WriteLine("Message received: " + message);
                var data = Encoding.UTF8.GetBytes("Message sent back: " + message);
                int count = 0;
                foreach (var socket in Program.clientSockets)
                {
                    //ToDo fix the underlaying problem of too many clients added to the clientSockets-list: OnClientConnected is triggered before Client is setup propperly
                    if (count % 2 != 0)
                    {
                        socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSendMessage), socket);
                    }
                    count++;
                }
            }
            clientSocket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(OnMessageReceived), clientSocket);
        }

        /// <summary>
        /// When a new message is sent, this method is triggered to send the message
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnSendMessage(IAsyncResult asyncResult)
        {
            var clientSocket = (Socket)asyncResult.AsyncState;
            clientSocket.EndSend(asyncResult);
        }
    }
}
