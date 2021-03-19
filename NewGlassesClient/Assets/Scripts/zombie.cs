using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombie : MonoBehaviour
{

    public GameObject target;
    public float speed;
    public float damage;

    public float maxHealth;
    private float health;

    private Rigidbody rb;

    public GameObject bullet;
    public GameObject car;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target.transform);
        Vector3 tempVect = new Vector3(0, 0, 1);
        tempVect = transform.forward;
        tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.MovePosition(transform.position + tempVect);
    }

    void OnCollisionStay(Collision collisionInfo) {
        Debug.Log(collisionInfo.gameObject.name);
        if (collisionInfo.gameObject.name == "BulletHolder(Clone)") {
            Destroy(gameObject);
            Destroy(collisionInfo.gameObject);
        }
        if (collisionInfo.gameObject == car) {
            if (staticStuff.carHealth > 0)
            {
                staticStuff.carHealth -= 0.5f;
            }
            Debug.Log("HIT CAR");
        }
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.name == "BulletHolder(Clone)")
        {
            Destroy(gameObject);
            Destroy(c.gameObject);
        }
    }

}
