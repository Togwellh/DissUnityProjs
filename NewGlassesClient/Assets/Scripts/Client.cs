using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

using TMPro;
using NRKernal;

//http://www.robotmonkeybrain.com/good-enough-guide-to-unitys-unet-transport-layer-llapi/


// Gets the locations of other cars from the Zed 2 camera and controls the player using head orientation
public class Client : MonoBehaviour
{

    // Networking Variables
    int channelUnreliable;
    int connectionId;
    int socketId;
    int socketPort = 8888;
    int maxConnections = 10;
    int myReliableChannelId;
    int chanSeq;


    // Offsets and modifiers to ensure cars are in the correct location
    public float xMult = 2;
    public float yMult = 2;

    public float widMult = 0.01f;
    public float heiMult = 0.01f;

    public float xOff = 0;
    public float yOff = 0;
    public float z = 0;

    // Car Kart Variables
    private Plane plane;
    private Ray ray;
    public GameObject cam;

    private Vector3 hitPoint;

    public GameObject player;
    private Rigidbody rb;

    public float width = 3;

    public GameObject coin;
    public float coinWait = 100;
    private float coinWaited = 0;

    public TextMeshProUGUI text;
    public float score;

    Dictionary<int, GameObject> cubes = new Dictionary<int, GameObject>();
    public GameObject cube;


    public GameObject bulletSpawn;
    public GameObject bullet;
    private int fireWait = 1000;
    private int fireWaitTime = 50;

    // Creates a connection with the Zed 2 camera project
    public void Connect()
    {

        // Create the connection
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        myReliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
        HostTopology topology = new HostTopology(config, maxConnections);
        socketId = NetworkTransport.AddHost(topology, 0);
        byte error;
        connectionId = NetworkTransport.Connect(socketId, "127.0.0.1", socketPort, 0, out error);

        // Report any errors
        if (error != null)
        {
            Debug.Log("Network Error : " + (NetworkError)error);
        }

        Debug.Log("Connected to server. ConnectionId: " + connectionId);

    }

    // Start is called before the first frame update
    void Start()
    {
        // Connect to the Zed 2
        Connect();

        // Create the plane which the users head orientation ray will intersect with
        plane = new Plane(new Vector3(-30, -30, 5), new Vector3(0, 30, 5), new Vector3(30, -30, 5));
        rb = player.GetComponent<Rigidbody>();

        // Setup the static class to be accessed by spawned objects
        staticStuff.player = GameObject.Find("Player");
        staticStuff.netMan = GameObject.Find("NetworkStuff");
    }

    // Update is called once per frame
    void Update()
    {

        // Update the score
        text.text = "Score : " + score;

        // Check for an update on the car locations
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

                // Parse the updated locations into the Info struct format and process it
                Info inf = JsonUtility.FromJson<Info>(message);
                updateCars(inf);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("remote client event disconnected");
                break;
        }

        // Spawn coins with a wait period in between
        coinWaited++;
        if (coinWaited > coinWait)
        {
            coinWaited = 0;
            GameObject newCoin = Instantiate(coin, new Vector3(UnityEngine.Random.Range(-width, width), 0.2f, 15), transform.rotation);
        }



        if (fireWait > fireWaitTime)
        {
            if (Input.GetKeyDown("space"))
            {
                fireWait = 0;
                Quaternion newRot = bulletSpawn.transform.rotation;
                newRot.y += 90;
                newRot.z += 90;

                GameObject newBullet = Instantiate(bullet, bulletSpawn.transform.position, newRot);
            }
        }
        else {
            fireWait++;
        }

    }

    // Takes in the Info struct and uses it to update existing cars and add any new ones
    private void updateCars(Info inf)
    {
        // Determine if there are currently spawned cars which do not exist in the update
        List<int> toRemove = new List<int>();
        foreach (KeyValuePair<int, GameObject> entry in cubes)
        {
            if (Array.IndexOf(inf.IDs, entry.Key) == -1)
            {
                toRemove.Add(entry.Key);
            }
        }
        
        // Remove any found
        foreach (int toRem in toRemove)
        {
            Destroy(cubes[toRem]);
            cubes.Remove(toRem);
        }

        // Go through the info update
        for (int i = 0; i < inf.Xs.Length; i++)
        {
            // If the car does not exist already then create and register it
            if (!cubes.ContainsKey(inf.IDs[i]))
            {
                GameObject newCube = Instantiate(cube, new Vector3((inf.Xs[i] * xMult) - xOff, (inf.Ys[i] * yMult) - yOff, inf.Zs[i]), transform.rotation);
                newCube.transform.localScale = new Vector3(inf.widths[i] * widMult, inf.heights[i] * heiMult, inf.depths[i]);
                cubes.Add(inf.IDs[i], newCube);
            }
            // If it does then update the position and size
            else {
                cubes[inf.IDs[i]].transform.position = new Vector3((inf.Xs[i]* xMult) - xOff, (inf.Ys[i]* yMult) - yOff, inf.Zs[i]);
                cubes[inf.IDs[i]].transform.localScale = new Vector3(inf.widths[i] * widMult, inf.heights[i] * heiMult, inf.depths[i]);
            }
        }
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


    // Updates the player position based on the user's head movement
    void FixedUpdate()
    {

        Vector3 move = new Vector3(0, 0, 0);

        // Create a ray based on the direction the user is looking
        ray = new Ray(cam.transform.position, cam.transform.forward);

        float enter = 0.0f;

        // If the ray hits the plane then update the player location
        if (plane.Raycast(ray, out enter))
        {
            hitPoint = ray.GetPoint(enter);

            // Move the player
            if (transform.position.x < hitPoint.x - 0.2f)
            {
                move += (transform.right * Time.fixedDeltaTime * 1.5f);
            }
            if (transform.position.x > hitPoint.x + 0.2f)
            {
                move += -(transform.right * Time.fixedDeltaTime * 1.5f);
            }
            rb.MovePosition(transform.position + move);

            // Limit the player movement to the set game boundaries
            Vector3 tmps = transform.position;
            tmps.x = hitPoint.x;
            if (tmps.x < -width)
            {
                tmps.x = -width;
            }
            if (tmps.x > width)
            {
                tmps.x = width;
            }

            transform.position = tmps;
        }
    }

}
