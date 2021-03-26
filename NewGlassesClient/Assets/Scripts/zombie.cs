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

    public GameObject bloodSplatter;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (staticStuff.gameOver) {
            return;
        }

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
                staticStuff.carHealth -= 0.1f;
            }
            Debug.Log("HIT CAR");
        }
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.name == "BulletHolder(Clone)")
        {
            staticStuff.score += 1;
            GameObject blood = Instantiate(bloodSplatter, c.gameObject.transform.position, c.gameObject.transform.rotation);
            //blood.transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
            staticStuff.zombieNum--;
            Destroy(gameObject);
            Destroy(c.gameObject);
        }
    }

}
