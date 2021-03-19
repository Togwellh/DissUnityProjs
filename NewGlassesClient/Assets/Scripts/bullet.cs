using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{

    public float speed = 5f;

    private Rigidbody rb;

    public Quaternion rot;

    public float damage = 5;

    public float lifetime = 250;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //transform.rotation
        Debug.Log("Rot : " + rot);
        transform.localRotation = rot;
        //transform.rotation = rot;
        //rb.rotation = rot.normalized;
        //Debug.Log("Actual : " + rb.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tempVect = new Vector3(0, 0, 1);
        tempVect = transform.forward;
        tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.MovePosition(transform.position + tempVect);
        //rb.rotation = rot.normalized;

        lifetime--;
        if (lifetime < 0) {
            Destroy(gameObject);
        }
        
    }

}
