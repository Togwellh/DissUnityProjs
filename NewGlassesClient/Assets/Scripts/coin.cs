using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coin : MonoBehaviour
{
    public float value = 10f;
    //private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0f, 1f, 0f);
        transform.position -= Vector3.forward/15;
        //rb.MovePosition(transform.position - Vector3.forward * Time.fixedDeltaTime);

        if (transform.position.z < 0)
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "BoxCol")
        {
            staticStuff.netMan.GetComponent<Client>().score += 10;// other.gameObject.GetComponent<coin>().value;
            Destroy(gameObject);
        }
        
    }


}
