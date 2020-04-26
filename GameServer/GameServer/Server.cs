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
    static Mutex clientsMutex = new Mutex();

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
        catch
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
            clientsMutex.WaitOne();
            for (byte cid = 0; cid < maxClients; cid++)
            {
                if(!clients.ContainsKey(cid))
                {
                    id = cid;
                    break;
                }
            }
            clientsMutex.ReleaseMutex();

            if (id != 255)
            {
                Client client = new Client();
                client.clientNumber = id;
                client.socket = incomingConnection;
                client.position = new Vector3(0, 0, 0);

                clientsMutex.WaitOne();
                clients[id] = client;
                clientsMutex.ReleaseMutex();

                ClientData idData = new ClientData(PacketType.CLIENT_ID_UPDATE);
                idData.clientNumber = id;

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
        while (true)
        {
            //First read the incoming stuff
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                //Console.WriteLine(e.ToString());
                Console.WriteLine("Lost connection to client.");

                clientsMutex.WaitOne();
                if (clients.ContainsKey(client.clientNumber))
                {
                    clients.Remove(client.clientNumber);
                }
                else
                {
                    Console.WriteLine("error");
                }
                clientsMutex.ReleaseMutex();

                // Shutdown and end connection
                client.socket.Close();
                return;
            }

            ClientData data = new ClientData(buffer);

            if (data.dataType == PacketType.POSITION_UPDATE)
            {
                byte clientNum = data.clientNumber;
                Vector3 pos = new Vector3(data.xPos, data.yPos, data.zPos);

                clientsMutex.WaitOne();
                clients[clientNum].position = pos;
                clientsMutex.ReleaseMutex();
            }
            else if(data.dataType == PacketType.ROTATION_UPDATE)
            {
                byte clientNum = data.clientNumber;
                Vector3 rot = new Vector3(data.xRot, data.yRot, data.zRot);

                clientsMutex.WaitOne();
                clients[clientNum].rotation = rot;
                clientsMutex.ReleaseMutex();
            }

            //Then send data
            clientsMutex.WaitOne();
            foreach (var c in clients)
            {
                //Only send data for clients that aren't this client
                if (c.Key != client.clientNumber)
                {
                    Client remoteClient = c.Value;

                    //Set up the position data packets
                    ClientData posData = new ClientData(PacketType.POSITION_UPDATE);
                    posData.clientNumber = remoteClient.clientNumber;
                    posData.xPos = remoteClient.position.X;
                    posData.yPos = remoteClient.position.Y;
                    posData.zPos = remoteClient.position.Z;

                    //Set up the rotation data packets
                    ClientData rotData = new ClientData(PacketType.ROTATION_UPDATE);
                    rotData.clientNumber = remoteClient.clientNumber;
                    rotData.xRot = remoteClient.rotation.X;
                    rotData.yRot = remoteClient.rotation.Y;
                    rotData.zRot = remoteClient.rotation.Z;

                    try
                    {
                        //set buffer to postion data
                        buffer = posData.raw;

                        //Send position
                        stream.Write(buffer);

                        //set buffer to rotation data
                        buffer = rotData.raw;

                        //Send rotation
                        stream.Write(buffer);
                    }
                    catch
                    {
                        //Console.WriteLine(e.ToString());
                        Console.WriteLine("Lost connection to client.");

                        if (clients.ContainsKey(client.clientNumber))
                        {
                            clients.Remove(client.clientNumber);
                        }
                        else
                        {
                            Console.WriteLine("error");
                        }


                        // Shutdown and end connection
                        client.socket.Close();
                        clientsMutex.ReleaseMutex();
                        return;
                    }
                }
            }
            clientsMutex.ReleaseMutex();
        }
    }

    public static void Main(string[] args)
    {
        string host;
        int port = 80;

        if (args.Length < 2)
        {
            // If no server name is passed as argument to this program, 
            // use the current host name as the default.
            host = "192.168.1.175";
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
            clientsMutex.WaitOne();
            foreach ( var client in clients)
            {
                if(!client.Value.threadStarted)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(HandleConnection));
                    t.Start(client.Value);
                    client.Value.threadStarted = true;
                }
            }
            clientsMutex.ReleaseMutex();
        }
    }
}