using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Net.Sockets;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;

public class Host : MonoBehaviour
{
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

    StreamReader reader;
    string line;

    private System.Net.NetworkInformation.Ping p;

    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();

        myReliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
        //channelUnreliable = config.AddChannel(QosType.UnreliableSequenced);
        //chanSeq = config.AddChannel(QosType.ReliableSequenced);

        HostTopology topology = new HostTopology(config, maxConnections);
        reader = new StreamReader("dataStore.txt");

        socketId = NetworkTransport.AddHost(topology, socketPort);
        Debug.Log("Socket Open. SocketId is: " + socketId);

    }

    private String getOtherIP()
    {
        String hostName = "";
        try
        {
            // Get the local computer host name.
            hostName = Dns.GetHostName();
            Debug.Log("Computer name :" + hostName);
        }
        catch (Exception e)
        {
            Debug.Log("Exception caught!!!");
            Debug.Log("Source : " + e.Source);
            Debug.Log("Message : " + e.Message);
        }

        IPAddress[] addresses = Dns.GetHostAddresses(hostName);
        Debug.Log($"GetHostAddresses({hostName}) returns:");
        String myAddr = "";
        foreach (IPAddress address in addresses)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork && address.ToString().Substring(0, 3) == "192")
            {
                myAddr = address + "";// My IP
                break;
            }
            Debug.Log($"    {address}");
        }
        Debug.Log("MYADD : " + myAddr);

        p = new System.Net.NetworkInformation.Ping();
        String outIp = pingIps("192.168.43", myAddr);

        if (outIp != "")
        {
            Debug.Log("Success! : " + outIp);
        }
        else
        {
            Debug.Log("Trying pt 2");

            for (int i = 1; i <= 255; i++)
            {
                outIp = pingIps("192.168." + i + ".", myAddr);
                if (outIp != "")
                {
                    break;
                }
            }

            if (outIp != "")
            {
                Debug.Log("\nSuccess! : " + outIp);
            }
            else
            {
                Debug.Log("\nFailure");
            }

        }

        return outIp;
    }

    private String pingIps(String firstBit, String myAddr)
    {
        String curIp;
        for (int i = 1; i <= 255; i++)
        {
            Debug.Log("Go " + DateTime.Now.ToString("HH:mm:ss tt"));
            curIp = firstBit + "." + i;
            System.Net.NetworkInformation.PingReply rep = p.Send(curIp, 100);
            if (rep.Status == System.Net.NetworkInformation.IPStatus.Success && curIp != myAddr)
            {
                return curIp;
                //host is active
            }
        }
        return "";
    }

    // Update is called once per frame  
    void Update()
    {

        //GetBoxes();

        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recBuffer = new byte[1350];
        int bufferSize = 1350;
        int dataSize;
        byte error;
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);

        connectionId = recConnectionId;
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

        if (connected > 0)
        {
            //Debug.Log("sending");
            if (send)
            {
                line = reader.ReadLine();
                if (line == null)
                {
                    Debug.Log("Out of lines, resetting");
                    reader = new StreamReader("dataStore.txt");
                }
                else
                {
                    SendSocketMessage();
                }
            }
            send = !send;
        }

    }

    public void SendSocketMessage()
    {

        
        


        //Info inf = GetBoxes();//new Info();
        //inf.Xs = new float[] { 1, 7 };
        //inf.Ys = new float[] { 2, 6 };
        //inf.Zs = new float[] { 3, 5 };


        //string jsonString = JsonUtility.ToJson(inf);

        byte error;
        byte[] buffer = new byte[1350];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, line);

        int bufferSize = 1350;

        NetworkTransport.Send(hId, connId, chanId, buffer, bufferSize, out error);
        //NetworkTransport.Send(socketId, connectionId, myReliableChannelId, buffer, bufferSize, out error);

        Debug.Log((NetworkError)error + " : error");
    }

    public Info GetBoxes()
    {
        int[] comps = { };
        int count = 0;
        //GameObject[] boxes = GameObject.FindGameObjectsWithTag("box");
        Info toReturn = new Info();
        toReturn.Xs = new float[comps.Length];
        toReturn.Ys = new float[comps.Length];
        toReturn.Zs = new float[comps.Length];
        toReturn.widths = new float[comps.Length];
        toReturn.heights = new float[comps.Length];
        toReturn.depths = new float[comps.Length];
        toReturn.IDs = new int[comps.Length];

        GameObject box = null;

        toReturn.Xs[count] = (float)box.transform.position.x;
        toReturn.Ys[count] = (float)box.transform.position.y;
        toReturn.widths[count] = (float)box.GetComponent<RectTransform>().sizeDelta.x;
        toReturn.heights[count] = (float)box.GetComponent<RectTransform>().sizeDelta.y;
        toReturn.IDs[count] = 1;


        //Debug.Log("Count : " + toReturn.Xs.Length);

        return toReturn;

    }

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
