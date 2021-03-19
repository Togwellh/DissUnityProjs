using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxScr : MonoBehaviour
{

    private int dispCount = 100;
    public int type; // 0 = score, 1 = ammo, 2 = health

    public GameObject[] types;

    public Material initial;
    public Material cracked;

    private GameObject symbol;

    // Start is called before the first frame update
    void Start()
    {
        type = Random.Range(0, 3);
        symbol = Instantiate(types[type], new Vector3(0, 1, 0), Quaternion.identity);
        symbol.transform.parent = transform;
        symbol.transform.localPosition = new Vector3(0, 1, 0);
        GetComponent<MeshRenderer>().material = initial;
    }

    // Update is called once per frame
    void Update()
    {
        if (dispCount > 0)
        {
            dispCount--;
        }
        /*
        if (transform.position.x < staticStuff.lowXcutoff || transform.position.x > staticStuff.upperXcutoff || dispCount > 0)
        {
            Color c = GetComponent<MeshRenderer>().material.color;
            c.a = 0f;
            GetComponent<MeshRenderer>().material.color = c;
        }
        else {
            Color c = GetComponent<MeshRenderer>().material.color;
            c.a = 1f;
            GetComponent<MeshRenderer>().material.color = c;
        }*/

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "BoxCol")
        {
            staticStuff.netMan.GetComponent<Client>().score = 0;// other.gameObject.GetComponent<coin>().value;
        }

        if (other.name == "BulletHolder(Clone)") {
            GetComponent<MeshRenderer>().material = cracked;
            if (type == 0) {
                staticStuff.score++;
            }
            if (type == 1)
            {
                staticStuff.ammo++;
            }
            if (type == 2) {
                staticStuff.carHealth = staticStuff.maxCarHealth;
            }
            symbol.SetActive(false);
        }

    }

}
