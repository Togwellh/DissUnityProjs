using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

using TMPro;

//http://www.robotmonkeybrain.com/good-enough-guide-to-unitys-unet-transport-layer-llapi/

public class Client : MonoBehaviour
{
    int channelUnreliable;
    int connectionId;
    int socketId;
    int socketPort = 8888;
    int maxConnections = 10;
    int myReliableChannelId;
    int chanSeq;
    string dataStore = "";
    //public TextMeshProUGUI txt;

    public void Connect()
    {

        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();


        myReliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
        //channelUnreliable = config.AddChannel(QosType.UnreliableSequenced);
        //chanSeq = config.AddChannel(QosType.ReliableSequenced);
        HostTopology topology = new HostTopology(config, maxConnections);
        socketId = NetworkTransport.AddHost(topology, 0);
        byte error;
        connectionId = NetworkTransport.Connect(socketId, "127.0.0.1", socketPort, 0, out error);
        Debug.Log((NetworkError)error + " : e");

        //txt.text = error + "";

        Debug.Log("Connected to server. ConnectionId: " + connectionId);
    }

    // Start is called before the first frame update
    void Start()
    {
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recBuffer = new byte[1350];
        int bufferSize = 1350;
        int dataSize;
        byte error;
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);

        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("incoming connection event received");
                break;
            case NetworkEventType.DataEvent:
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;
                Info inf = JsonUtility.FromJson<Info>(message);
                Debug.Log("incoming message event received : " + message);
                dataStore += message + "\n";
                //infoToString(inf);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("remote client event disconnected");
                StreamWriter writer = new StreamWriter("dataStore.txt", true);
                writer.Write(dataStore);
                break;
        }

    }/*
    private void infoToString(Info inf)
    {

        List<int> toRemove = new List<int>();

        foreach (KeyValuePair<int, GameObject> entry in cubes)
        {
            if (Array.IndexOf(inf.IDs, entry.Key) == -1)
            {
                toRemove.Add(entry.Key);
            }
        }

        foreach (int toRem in toRemove)
        {
            Destroy(cubes[toRem]);
            cubes.Remove(toRem);
        }

        for (int i = 0; i < inf.Xs.Length; i++)
        {
            if (!cubes.ContainsKey(inf.IDs[i]))
            {
                GameObject newCube = Instantiate(cube, new Vector3(inf.Xs[i] * 5, inf.Ys[i] * 5, 0), transform.rotation);
                newCube.transform.localScale = new Vector3(inf.widths[i] / 100, inf.heights[i] / 100, 0.1f);
                cubes.Add(inf.IDs[i], newCube);
            }
            else
            {
                cubes[inf.IDs[i]].transform.position = new Vector3(inf.Xs[i], inf.Ys[i], 0);
                cubes[inf.IDs[i]].transform.localScale = new Vector3(inf.widths[i] / 100, inf.heights[i] / 100, 0.1f);
            }
            Debug.Log(i + " ID " + inf.IDs[i] + " : (" + inf.Xs[i] + ", " + inf.Ys[i] + ")");
            //txt.text = i + " ID " + inf.IDs[i] + " : (" + inf.Xs[i] + ", " + inf.Ys[i] + ")";
        }
    }*/

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
