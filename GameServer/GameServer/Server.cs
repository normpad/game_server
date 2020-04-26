using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

public class Server
{
    static TcpListener server;
    static Dictionary<int, Client> clients;

    static int maxClients = 100;

    // This method requests the home page content for the specified server.
    private static string InitializeSocket(string host, int port)
    {

        IPAddress bindAddress = IPAddress.Parse(host);
        server = new TcpListener(bindAddress, port);

        try
        {
            server = new TcpListener(bindAddress, port);
            server.Start();
        }
        catch (SocketException err)
        {
            Console.WriteLine("fuck");
            return "fuck";
        }

        return "Socket listening on: " + host + ":" + port;
    }

    public static void ListenForConnections()
    {
        Console.WriteLine("Waiting for a connection...");
        while (true)
        {
            TcpClient incomingConnection = server.AcceptTcpClient();

            Console.WriteLine("Got a connection!");

            //assign client ID
            byte id = 255;
            for (byte cid = 0; cid < maxClients; cid++)
            {
                if(!clients.ContainsKey(cid))
                {
                    id = cid;
                    break;
                }
            }

            if (id != 255)
            {
                Client client = new Client();
                client.clientNumber = id;
                client.socket = incomingConnection;
                client.position = new Vector3(0, 0, 0);

                clients[id] = client;

                ClientData idData = new ClientData();
                idData.clientNumber = id;
                idData.dataType = PacketType.CLIENT_ID_UPDATE;

                //Send the dude his number
                client.socket.GetStream().Write(idData.raw, 0, ClientData.dataSize);
            }
        }
    }
    
    public static void HandleConnection(object arg)
    {
        Client client = (Client)arg;

        Console.WriteLine("Handling connection for client...");

        Byte[] buffer = new Byte[ClientData.dataSize];

        // Get a stream object for reading and writing
        NetworkStream stream = client.socket.GetStream();

        int bytesRead;

        // Loop to receive all the data sent by the client.
        try
        {
            while (true)
            {
                //First read the incoming stuff
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                ClientData data = new ClientData(buffer);

                if (data.dataType == PacketType.POSITION_UPDATE)
                {
                    byte clientNum = data.clientNumber;
                    Vector3 pos = new Vector3(data.x, data.y, data.z);
                    clients[clientNum].position = pos;
                }
                

                //Then send data
                foreach(var c in clients)
                {
                    //Only send data for clients that aren't this client
                    if (c.Key != client.clientNumber)
                    {
                        Client remoteClient = c.Value;
                        ClientData sendData = new ClientData();
                        sendData.clientNumber = remoteClient.clientNumber;
                        sendData.dataType = PacketType.POSITION_UPDATE;
                        sendData.x = remoteClient.position.X;
                        sendData.y = remoteClient.position.Y;
                        sendData.z = remoteClient.position.Z;
                        buffer = sendData.raw;
                        stream.Write(buffer);
                    }
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine("Lost connection to client.");

            // Shutdown and end connection
            client.socket.Close();
        }
    }

    public static void Main(string[] args)
    {
        string host;
        int port = 80;
        int numConnections = 0;

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


        clients = new Dictionary<int,Client>();
        string result = InitializeSocket(host, port);

        Console.WriteLine(result);

        Thread listenThread = new Thread(new ThreadStart(ListenForConnections));
        listenThread.Start();

        while(true)
        {
            if (numConnections < clients.Count)
            {
                Thread t = new Thread(new ParameterizedThreadStart(HandleConnection));
                t.Start(clients[numConnections]);
                numConnections++;
            }
        }
    }
}