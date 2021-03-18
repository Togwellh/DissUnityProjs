using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bombScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(Random.Range(-2f, 2f), 5, 3);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.TransformPoint(Vector3.zero);

        transform.position -= new Vector3(0, 0.005f, 0);
        Vector3 tmp = transform.position;
        tmp.z = 3;
        transform.position = tmp;

        if (transform.position.y < -3)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit");

        controller.score = 0;

        Destroy(gameObject);
    }

}
