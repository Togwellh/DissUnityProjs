using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coinScript : MonoBehaviour
{

    public float zScale = 0;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(Random.Range(-1.5f, 1.5f), -0.5f, 3);
        zScale = transform.localScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.TransformPoint(Vector3.zero);
        Vector3 tmpScale = transform.localScale;
        tmpScale *= 1.01f;
        tmpScale.z = zScale;
        transform.position -= new Vector3(0, 0.0005f, 0);
        transform.localScale = tmpScale;
        
        
        if (tmpScale.x > 0.55f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit");

        controller.score += 10;

        Destroy(gameObject);
    }
}
