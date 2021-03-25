using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodScript : MonoBehaviour
{

    private float speed = 3f;
    private Rigidbody rb;
    private float scale;

    // Start is called before the first frame update
    void Start()
    {
        scale = Random.Range(0.03f, 0.15f);
        transform.localScale = new Vector3(scale, scale, scale);
        rb = GetComponent<Rigidbody>();
        Quaternion q = transform.rotation;// = transform.rotation * new Quaternion(Random.Range(0, 0.0001f), Random.Range(0, 0.0001f), Random.Range(0, 0.0001f), Random.Range(0, 0.0001f));
        q.x += Random.Range(0, 0.1f);
        q.y += Random.Range(0, 0.1f);
        q.z += Random.Range(0, 0.1f);
        q.w += Random.Range(0, 0.1f);
        transform.rotation = q;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tempVect = new Vector3(0, 0, 1);
        tempVect = transform.forward;
        tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.MovePosition(transform.position + tempVect);

        scale -= 0.003f;
        if (scale < 0.001) {
            Destroy(gameObject);
        }
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
