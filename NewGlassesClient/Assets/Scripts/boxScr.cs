using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxScr : MonoBehaviour
{

    private int dispCount = 100;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dispCount > 0)
        {
            dispCount--;
        }

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
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "BoxCol")
        {
            staticStuff.netMan.GetComponent<Client>().score = 0;// other.gameObject.GetComponent<coin>().value;
        }

    }

}
