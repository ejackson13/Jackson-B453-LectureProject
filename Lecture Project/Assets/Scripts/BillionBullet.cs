using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillionBullet : MonoBehaviour
{
    public string bulletColor; // the color of the bullet

    public float bulletSpeed = 5f; // the speed at which a bullet will travel
    public float bulletDistance = 10; // the max distance a bullet will travel
    public float bulletDamage = 25; // the damage the bullet deals

    public Vector3 startPosition; // the starting position of the bullet, set by billion when it is shot
    public Vector2 direction; // the direction the bullet is heading

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // destroy gameobject if the bullet has traveled farther than the max distance
        if (Vector2.Distance(startPosition, transform.position) >= bulletDistance)
        {
            Destroy(gameObject);
        }

        // move bulletSpeed units in direction every second
        Vector3 deltaPos = direction.normalized * (bulletSpeed * Time.deltaTime);
        deltaPos.z = 0;
        transform.position += deltaPos;
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        // check if the object it hits is an enemy billion
        if (other.gameObject.CompareTag("billion") && other.gameObject.GetComponent<Billion>().billionColor != bulletColor)
        {
            // deal damage to billion
            other.gameObject.GetComponent<Billion>().TakeDamage(bulletDamage);

            // destroy this gameObject
            Destroy(gameObject);
        }
        // destroy the bullet if it hits a wall or a billionare base
        else if (!other.gameObject.CompareTag("billion"))
        {
            Destroy(gameObject);
        }
    }
}
