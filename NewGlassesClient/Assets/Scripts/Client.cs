using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

using TMPro;
using NRKernal;

//http://www.robotmonkeybrain.com/good-enough-guide-to-unitys-unet-transport-layer-llapi/
/// <summary>
///  https://stackoverflow.com/questions/57070802/get-ip-address-of-all-devices-connected-on-the-same-network-in-unity
/// </summary>

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

    public float zOff = 0;
    public float zMult = 1;

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
    public int bullets = 20;

    Dictionary<int, GameObject> cubes = new Dictionary<int, GameObject>();
    public GameObject cube;


    public GameObject bulletSpawn;
    public GameObject bullet;
    private int fireWait = 1000;
    private int fireWaitTime = 50;

    public GameObject center;

    public float spawnRadius = 15;
    public int spawnWait = 50;
    public int spawnTimer = 0;

    public GameObject zombie;
    public GameObject car;
    public GameObject windowCracks;

    public TextMeshProUGUI output;
    private System.Net.NetworkInformation.Ping p;

    private int clip = 10;
    private int reloading = 0;

    public int bulletTrickleTime = 100;
    private int buTiWaited = 0;

    public float allowedAngle = 15f;

    public int maxZombies = 10;

    public Material cracks;

    public TextMeshProUGUI endText;
    private int countdown = 300;

    public bool controllerMode = false;
    public GameObject gun;

    public Boolean localConnect = false;

    public Boolean spawnZs = true;

    // Creates a connection with the Zed 2 camera project
    public void Connect(String otherIP)
    {
        output.text = output.text + "starting";
        // Create the connection
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        myReliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
        HostTopology topology = new HostTopology(config, maxConnections);
        socketId = NetworkTransport.AddHost(topology, 0);
        byte error;

        if (!localConnect)
        {
            connectionId = NetworkTransport.Connect(socketId, "192.168.43.42", socketPort, 0, out error);
        }
        else
        {
            connectionId = NetworkTransport.Connect(socketId, "127.0.0.1", socketPort, 0, out error);
        }

        // Report any errors
        if (error != null)
        {
            Debug.Log("Network Error : " + (NetworkError)error);
            output.text = output.text + "Network Error : " + (NetworkError)error;
        }
        output.text = output.text + " Connected to server. ConnectionId: " + connectionId;
        Debug.Log("Connected to server. ConnectionId: " + connectionId);

    }

    // Start is called before the first frame update
    void Start()
    {

        //String ip = getOtherIP();
        String ip = "whatever";
        output.text = output.text + "starting connection";
        Connect(ip);

        // Create the plane which the users head orientation ray will intersect with
        plane = new Plane(new Vector3(-30, -30, 5), new Vector3(0, 30, 5), new Vector3(30, -30, 5));
        //rb = player.GetComponent<Rigidbody>();

        // Setup the static class to be accessed by spawned objects
        //staticStuff.player = GameObject.Find("Player");
        staticStuff.netMan = GameObject.Find("NetworkStuff");

        cracks.SetTextureScale("_MainTex", new Vector2(0,0));

    }

    private String getOtherIP() {
        String hostName = "";
        String outtxt = "";
        try
        {
            // Get the local computer host name.
            hostName = Dns.GetHostName();
            Debug.Log("Computer name :" + hostName);
            outtxt += "Computer name :" + hostName + "\n";
        }
        catch (Exception e)
        {
            Debug.Log("Exception caught!!!");
            Debug.Log("Source : " + e.Source);
            Debug.Log("Message : " + e.Message);
        }

        IPAddress[] addresses = Dns.GetHostAddresses(hostName);
        outtxt += "GetHostAddresses(" + hostName + ") returns:\n";
        Debug.Log($"GetHostAddresses({hostName}) returns:");
        String myAddr = "";
        foreach (IPAddress address in addresses)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork && address.ToString().Substring(0, 3) == "192")
            {
                myAddr = address + "";// My IP
                break;
            }
            outtxt += address + "\n";
            Debug.Log($"    {address}");
        }
        output.text = outtxt;
        output.text = "Myaddr : " + myAddr;
        Debug.Log("MYADD : " + myAddr);

        p = new System.Net.NetworkInformation.Ping();
        String outIp = pingIps("192.168.43", myAddr);

        output.text += "\nHereA";

        if (outIp != "")
        {
            output.text = "Success! : " + outIp;
        }
        else
        {
            output.text = "Trying pt 2";

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
                output.text += "\nSuccess! : " + outIp;
            }
            else
            {
                output.text += "\nFailure";
            }

        }

        return outIp;
    }

    private String pingIps(String firstBit, String myAddr) {
        String curIp;
        for (int i = 1; i <= 255; i++)
        {
            output.text = i + "";
            Debug.Log("Go " + DateTime.Now.ToString("HH:mm:ss tt"));
            curIp = firstBit + "." + i;
            try
            {
                System.Net.NetworkInformation.PingReply rep = p.Send(curIp, 100);
                output.text = "pinged " + i;
                if (rep.Status == System.Net.NetworkInformation.IPStatus.Success && curIp != myAddr)
                {
                    return curIp;
                    //host is active
                }
            }
            catch(Exception e)
            {
                output.text = e + "";
            }
            
        }
        return "";
    }

    // Update is called once per frame
    void Update()
    {

        if (staticStuff.score > 0) {
            score += staticStuff.score * 10;
            staticStuff.score = 0;
        }

        if (staticStuff.ammo > 0)
        {
            bullets += staticStuff.ammo * 10;
            staticStuff.ammo = 0;
        }
        if (staticStuff.carHealth < 0) {
            staticStuff.carHealth = 0;
        }
        // Update the score
        if (reloading > 0) {
            text.text = "Score : " + score + "\nHealth : " + Math.Round(staticStuff.carHealth, 1) + "\nAmmo : Reloading/" + bullets;
        }
        else
        {
            text.text = "Score : " + score + "\nHealth : " + Math.Round(staticStuff.carHealth, 1) + "\nAmmo : " + clip + "/" + bullets;
        }

        if (staticStuff.gameOver)
        {
            if (countdown > 0)
            {
                countdown--;
            }
            text.text = "";
            endText.text = "Game Over!\nScore:" + score + "\nRestarting in " + Math.Ceiling((double)(countdown / 100)+1) + "...";
            if (countdown <= 0) {
                staticStuff.carHealth = staticStuff.maxCarHealth;
                staticStuff.score = 0;
                staticStuff.ammo = 0;
                staticStuff.zombieNum = 0;
                staticStuff.gameOver = false;
                countdown = 300;
                clip = 10;
                bullets = 20;
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); This works on laptop but crashed on nreal
            }
        }
        else {
            endText.text = "";
        }

        if (staticStuff.carHealth <= 0) {
            staticStuff.gameOver = true;
        }

        // Check for an update on the car locations
        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recBuffer = new byte[1350];
        int bufferSize = 1350;
        int dataSize;
        byte error;
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);
        //output.text = output.text + "" + recNetworkEvent;
        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("incoming connection event received");
                //output.text = output.text + "connectEventReceived";
                break;
            case NetworkEventType.DataEvent:
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;

                // Parse the updated locations into the Info struct format and process it
                Info inf = JsonUtility.FromJson<Info>(message);
                updateCars(inf);
                //output.text = output.text + "car update received";
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
            //GameObject newCoin = Instantiate(coin, new Vector3(UnityEngine.Random.Range(-width, width), 0.2f, 15), transform.rotation);
        }
        /*
        if (staticStuff.carHealth < 50 && windowCracks.transform.position.y > 50)
        {
            windowCracks.transform.position -= new Vector3(0, windowCracks.transform.position.y - 0.75f, 0);
        }

        if (staticStuff.carHealth > 50 && windowCracks.transform.position.y < 50)
        {
            windowCracks.transform.position += new Vector3(0, 100, 0);
        }
        */
        float newC = (float)(1 - Math.Round(staticStuff.carHealth / 100, 1));
        cracks.SetTextureScale("_MainTex", new Vector2(newC, newC));
        

        spawnTimer++;
        if (spawnTimer > spawnWait && staticStuff.zombieNum < maxZombies && !staticStuff.gameOver && spawnZs) {
            spawnTimer = 0;
            GameObject newZombie = Instantiate(zombie, new Vector3(0,0.1f,20), Quaternion.identity);
            newZombie.transform.RotateAround(center.transform.position, Vector3.up, UnityEngine.Random.Range(0, 360));
            newZombie.GetComponent<zombie>().car = car;
            newZombie.GetComponent<zombie>().target = center;
            Vector3 targetDir = newZombie.transform.position - car.transform.position;
            float angle = Vector3.Angle(targetDir, car.transform.forward);
            Debug.Log("Created with angle : " + angle);
            if (angle > allowedAngle)
            {
                Destroy(newZombie);
                spawnTimer = spawnWait + 1;
            }
            else {
                staticStuff.zombieNum++;
            }
        }

        if (controllerMode) {
            //gun.transform.position = NRInput.GetPosition() + new Vector3(0,1,0);
            gun.transform.rotation = NRInput.GetRotation();
        }

        if ((Input.GetKeyDown(KeyCode.E) || NRInput.GetButtonDown(ControllerButton.TRIGGER)) && clip > 0 && !staticStuff.gameOver)
        {
            Vector3 newRot = cam.transform.eulerAngles;

            //newRot.x += 90;
            //Debug.Log(newRot);
            //newRot.y += 90;
            //newRot.z += 90;
            Quaternion newRotQ = new Quaternion(newRot.x, newRot.y, newRot.z, cam.transform.localRotation.w);//.normalized;
            GameObject newBullet = Instantiate(bullet, bulletSpawn.transform.position, newRotQ);
            //newBullet.transform.parent = bulletSpawn.transform;
            //newBullet.transform.rotation = newRotQ;
            //newBullet.transform.GetChild(0).GetComponent<bullet>().rot = cam.transform.rotation;
            if (controllerMode) {
                newBullet.GetComponent<bullet>().rot = gun.transform.rotation;
            }
            else
            {
                newBullet.GetComponent<bullet>().rot = cam.transform.rotation;
            }
            clip--;
        }

        buTiWaited++;
        if (buTiWaited > bulletTrickleTime) {
            buTiWaited = 0;
            bullets++;
        }

        if (clip <= 0 && reloading <= 0) {
            reloading = 100;
        }

        if (reloading > 0 && bullets > 0) {
            reloading--;
        }

        if (reloading <= 0 && clip == 0) {
            if (bullets < 10)
            {
                clip = bullets;
                bullets -= clip;
            }
            if (bullets >= 10) {
                clip = 10;
                bullets -= 10;
            }
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
                GameObject newCube = Instantiate(cube, new Vector3((inf.Xs[i] * xMult) - xOff, (inf.Ys[i] * yMult) - yOff, (inf.Zs[i] * zMult) - zOff), transform.rotation);
                newCube.transform.localScale = new Vector3(inf.widths[i] * widMult, inf.heights[i] * heiMult, inf.depths[i]);
                cubes.Add(inf.IDs[i], newCube);
            }
            // If it does then update the position and size
            else {
                cubes[inf.IDs[i]].transform.position = new Vector3((inf.Xs[i]* xMult) - xOff, (inf.Ys[i]* yMult) - yOff, (inf.Zs[i] * zMult) - zOff);
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
        /*
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
        */

    }

}
