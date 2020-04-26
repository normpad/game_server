using System;
using System.Net;
using System.Net.Sockets;

namespace GameClient
{
    class Program
    {
        private static Socket ConnectSocket(string server, int port)
        {
            Socket s = null;
            IPHostEntry hostEntry = null;
            IPAddress hostAddress = IPAddress.Parse(server);

            // Get host related information.
            hostEntry = Dns.GetHostEntry(server);

            IPEndPoint ipe = new IPEndPoint(hostAddress, port);
            Socket tempSocket =
                new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tempSocket.Connect(ipe);

            s = tempSocket;

            return s;
        }

        // This method requests the home page content for the specified server.
        private static string SocketSendReceive(string server, int port)
        {
            float x = 123.45f;
            float y = 456.78f;
            float z = 910.11f;
            float[] data = { x, y, z };
            Byte[] bytesSent = new byte[256];
            Buffer.BlockCopy(data, 0, bytesSent, 0, 12);

            Console.WriteLine("Attempting to connect to: " + server + ":" + port);

            // Create a socket connection with the specified server and port.
            using (Socket s = ConnectSocket(server, port))
            {
                if (s == null)
                    return ("Connection failed");

                // Send request to the server.
                s.Send(bytesSent, bytesSent.Length, 0);
            }

            return "Done.";
        }

        public static void Main(string[] args)
        {
            string host;
            int port = 80;

            if (args.Length < 2)
            {
                // If no server name is passed as argument to this program, 
                // use the current host name as the default.
                host = "127.0.0.1";
                port = 12345;
            }
            else
            {
                host = args[0];
                port = int.Parse(args[1]);
            }
            

            string result = SocketSendReceive(host, port);
            Console.WriteLine(result);
        }
    }
}