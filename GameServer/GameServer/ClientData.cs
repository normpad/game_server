using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

public enum PacketType
{ 
    CLIENT_ID_UPDATE = 0,
    POSITION_UPDATE = 1
}

public class Client
{
    public Client()
    {
        clientNumber = 255;
        socket = null;
        position = new Vector3();
    }

    public byte clientNumber { set; get; }
    public TcpClient socket { set; get; }
    public Vector3 position { set; get; }
}

public class ClientData
{
    public static int dataSize = 14;   //Number of bytes of data, float(4B)x3=12

    /*
     *  Initializes the data with default values
     */
    public ClientData()
    {
        raw = new byte[dataSize];
        positionArray = new float[3];
        clientNumber = 255;
    }

    /*
     * Initializes the data with raw bytes from incoming buffer
     */
    public ClientData(byte[] buffer)
    {
        raw = new byte[dataSize];
        positionArray = new float[3];
        clientNumber = 255;
        Buffer.BlockCopy(buffer, 0, raw, 0, dataSize);
        deserialize();
    }

    public byte clientNumber
    {
        get
        {
            return clientNum;
        }
        set
        {
            clientNum = value;
            serialize();
        }
    }

    public PacketType dataType
    {
        get
        {
            return (PacketType)dataT;
        }
        set
        {
            dataT = (byte)value;
            serialize();
        }
    }

    /*
     * Player X position
     */
    public float x
    {
        get 
        {
            return positionArray[0]; 
        }
        set
        { 
            positionArray[0] = value;
            serialize();
        }
    }

    /*
     * Player Y position
     */
    public float y
    {
        get 
        {
            return positionArray[1];  
        }
        set 
        { 
            positionArray[1] = value;
            serialize();
        }
    }

    /*
     * Player Z position
     */
    public float z
    {
        get 
        {
            return positionArray[2];  
        }
        set 
        { 
            positionArray[2] = value;
            serialize();
        }
    }

    private byte clientNum;
    private byte dataT;

    //Float array
    private float[] positionArray;

    //Raw bytes
    public byte[] raw { get; set; }

    public void serialize()
    {
        raw[0] = clientNumber;
        raw[1] = dataT;
        Buffer.BlockCopy(positionArray, 0, raw, 2, dataSize - 2);
    }

    public void deserialize()
    {
        clientNum = raw[0];
        dataT = raw[1];
        Buffer.BlockCopy(raw, 2, positionArray, 0, dataSize - 2);
    }

}