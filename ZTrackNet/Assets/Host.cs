using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;


// Connects to the glasses project. Finds the locations of the other cars and send them in regular updates
public class Host : MonoBehaviour
{

    // Networking variables
    int connectionId;
    int myReliableChannelId;
    int channelUnreliable;
    int chanSeq;
    int maxConnections = 10;
    int socketId;
    int socketPort = 8888;

    int hId;
    int connId;
    int chanId;

    int connected = 0;

    bool send = true;

    // Start is called before the first frame update
    void Start()
    {

        // Open a channel for the glasses client to connect to
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        myReliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
        HostTopology topology = new HostTopology(config, maxConnections);
        socketId = NetworkTransport.AddHost(topology, socketPort);
        Debug.Log("Socket Open. SocketId is: " + socketId);

    }

    // Update is called once per frame  
    void Update()
    {

        // Check for connections and disconnections
        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recBuffer = new byte[1350];
        int bufferSize = 1350;
        int dataSize;
        byte error;
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);
        
        connectionId = recConnectionId;

        // Updates whether there are any clients connected
        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("incoming connection event received");
                connected++;
                myReliableChannelId = recChannelId;
                hId = recHostId;
                connId = recConnectionId;
                chanId = recChannelId;
                break;
            case NetworkEventType.DataEvent:
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;
                Debug.Log("incoming message event received: " + message);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("remote client event disconnected");
                connected--;
                break;
        }

        // If there is a client then send an update
        if (connected > 0)
        {
            if (send)
            {
                SendSocketMessage();
            }
            // Sends updates every other tick to reduce strain on computer
            send = !send;
        }

    }

    // Sends an update containing the cars information to connected client
    public void SendSocketMessage()
    {
        // Create and fill the info struct with the update information
        Info inf = GetBoxes();

        // Serialise the Info struct into a string for sending
        string jsonString = JsonUtility.ToJson(inf);

        // Send the update
        byte error;
        byte[] buffer = new byte[1350];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, jsonString);

        int bufferSize = 1350;

        NetworkTransport.Send(hId, connId, chanId, buffer, bufferSize, out error);

        if (error != null)
        {
            Debug.Log("Network Error : " + (NetworkError)error);
        }
    }

    // Finds the detected cars and puts the position and size information from them in an Info struct
    public Info GetBoxes()
    {

        // Get the components with the boxScript attached to it
        var comps = GameObject.FindObjectsOfType<boxScript>();

        // Create and initialise the Info struct
        Info toReturn = new Info();
        toReturn.Xs = new float[comps.Length];
        toReturn.Ys = new float[comps.Length];
        toReturn.Zs = new float[comps.Length];
        toReturn.widths = new float[comps.Length];
        toReturn.heights = new float[comps.Length];
        toReturn.depths = new float[comps.Length];
        toReturn.IDs = new int[comps.Length];

        int count = 0;

        // Pattern for matching the IDs
        string pattern = @"([0-9]+)";

        // Go through the boxes which represent detected cars
        foreach (boxScript boxS in comps)
        {
            GameObject box = boxS.gameObject;

            // Use regex to find the ID of the car
            foreach (Match m in Regex.Matches(box.name, pattern))
            {
                if (Int32.TryParse(m.Value, out int j))
                {
                    // Add the cars information to the Info struct
                    toReturn.Xs[count] = (float)box.transform.position.x;
                    toReturn.Ys[count] = (float)box.transform.position.y;
                    toReturn.Zs[count] = (float)box.transform.position.z;

                    toReturn.widths[count] = (float)box.GetComponent<Transform>().localScale.x;
                    toReturn.heights[count] = (float)box.GetComponent<Transform>().localScale.y;
                    toReturn.depths[count] = (float)box.GetComponent<Transform>().localScale.z;
                    toReturn.IDs[count] = j;
                }
                break;
            }
            
            count++;
        }

        return toReturn;

    }

    // Struct for storing the update information. Stores positions, IDs and sizes
    [Serializable]
    public struct Info
    {
        public float[] Xs;
        public float[] Ys;
        public float[] Zs;
        public int[] IDs;
        public float[] widths;
        public float[] heights;
        public float[] depths;
    }

}
