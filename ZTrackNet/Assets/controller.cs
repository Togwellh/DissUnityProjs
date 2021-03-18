using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class controller : MonoBehaviour
{

    public static int score = 0;

    public bool on = false;

    public GameObject car;
    public GameObject coin;
    public GameObject bomb;

    public GameObject camera;

    public int coinWait = 100;
    private int waited = 0;

    public int bombWait = 150;
    private int bombWaited = 0;

    private bool addBomb = true;

    public TextMeshProUGUI txt;

    // Start is called before the first frame update
    void Start()
    {
        car.SetActive(on);
        if (!on)
        {
            txt.text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (on)
        {
            waited++;
            bombWaited++;
            if (waited > coinWait)
            {
                waited = 0;
                Vector3 loc = new Vector3(0, -0.7f, 3);
                Quaternion rot = new Quaternion(0, 0, 0, 0);
                GameObject newCoin = Instantiate(coin, loc, rot);
                newCoin.transform.parent = camera.transform;
                
            }

            if (bombWaited > bombWait) {
                Debug.Log("spaned bomb");
                bombWaited = 0;
                Vector3 locB = new Vector3(0, 5f, 3);
                Quaternion rotB = new Quaternion(0, 0, 0, 0);
                GameObject newBomb = Instantiate(bomb, locB, rotB);
                newBomb.transform.parent = camera.transform;
            }
            txt.text = "Score : " + score;
        }

        

    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.E))
        {
            score = 0;
        }
        if (Input.GetKey(KeyCode.A))
        {
            car.transform.position -= new Vector3(0.03f,0,0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            car.transform.position += new Vector3(0.03f, 0, 0);
        }
    }

}
