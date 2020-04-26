using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

public enum PacketType
{ 
    CLIENT_ID_UPDATE = 0,
    POSITION_UPDATE  = 1,
    ROTATION_UPDATE  = 2,
    ANIMATION_UPDATE = 3
}

public class Client
{
    public Client()
    {
        clientNumber = 255;
        socket = null;
        position = new Vector3();
        threadStarted = false;
    }

    public byte clientNumber { set; get; }
    public TcpClient socket { set; get; }
    public Vector3 position { set; get; }
    public Vector3 rotation { set; get; }
    public bool threadStarted { set; get; }
}

public class ClientData
{
    public static int dataSize = 32;   //32 bytes
    public static int dataStartOffset = 2;  // Offset where data starts, index 0,1 are meta

    /*
     *  Initializes the data with default values
     */
    public ClientData()
    {
        raw = new byte[dataSize];
        positionArray = new float[3];
        rotationArray = new float[3];
        clientNumber = 255;
    }

    public ClientData(PacketType pType)
    {
        raw = new byte[dataSize];
        positionArray = new float[3];
        rotationArray = new float[3];
        clientNumber = 255;

        dataType = pType;
    }

    /*
     * Initializes the data with raw bytes from incoming buffer
     */
    public ClientData(byte[] buffer)
    {
        raw = new byte[dataSize];
        positionArray = new float[3];
        rotationArray = new float[3];
        clientNumber = 255;

        //Copy the buffer into the raw data
        Buffer.BlockCopy(buffer, 0, raw, 0, dataSize);

        //Deserialize the data
        deserialize();
    }

    public byte clientNumber
    {
        get
        {
            return raw[0];
        }
        set
        {
            raw[0] = value;
        }
    }

    public PacketType dataType
    {
        get
        {
            return (PacketType)raw[1];
        }
        set
        {
            raw[1] = (byte)value;
        }
    }

    /*
     * Player X position
     */
    public float xPos
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
    public float yPos
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
    public float zPos
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

    public float xRot
    {
        get
        {
            return rotationArray[0];
        }
        set
        {
            rotationArray[0] = value;
            serialize();
        }
    }

    public float yRot
    {
        get
        {
            return rotationArray[1];
        }
        set
        {
            rotationArray[1] = value;
            serialize();
        }
    }

    public float zRot
    {
        get
        {
            return rotationArray[2];
        }
        set
        {
            rotationArray[2] = value;
            serialize();
        }
    }

    //Float array
    private float[] positionArray;
    private float[] rotationArray;

    //Raw bytes
    public byte[] raw { get; set; }

    public void serialize()
    {
        if (dataType == PacketType.POSITION_UPDATE)
        {
            Buffer.BlockCopy(positionArray, 0, raw, dataStartOffset, 12);
        }
        else if (dataType == PacketType.ROTATION_UPDATE)
        {
            Buffer.BlockCopy(rotationArray, 0, raw, dataStartOffset, 12);
        }
    }

    public void deserialize()
    {
        if (dataType == PacketType.POSITION_UPDATE)
        {
            Buffer.BlockCopy(raw, dataStartOffset, positionArray, 0, 12);
        }
        else if (dataType == PacketType.ROTATION_UPDATE)
        {
            Buffer.BlockCopy(raw, dataStartOffset, rotationArray, 0, 12);
        }
    }
}