using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Billion : MonoBehaviour
{
    public string billionColor;
    public Vector3 moveTo;
    public bool reachedStart;
    private Transform rigTransform;
    private Rigidbody2D rb;

    private Transform closestFlag;
    private Vector3 flagLastPos; // the last position of the closestFlag - used so that we can detect when a flag moves and we don't have to check for a new closest flag every frame\
    private Vector3 startPosition; // the position the billion started at when the flag was moved
    //private float slope; // the slope of the velocity graph
    public float maxSpeed; // the max speed the billions can reach
    private int numFlags;

    public float minCircleSize = .3f; // the minimum size of the inner circle (as a proportion, so between 0 and 1)
    public float maxHealth = 100; // the amount of starting health the billion gets
    public float currentHealth; // the amount of health the billion currently has
    public float clickDamage = 25; // the amount of damage the billion takes when being clicked

    public GameObject bulletPrefab; // the prefab of the bullets that we shoot
    public float shootDistance = 5f; // the max distance to an enemy billion where a billion will fire
    public float shootInterval = 1.5f; // the interval on which billions will shoot 
    private float nextFire = 0; // the time in seconds from game start at which the billion can fire its next shot

    // Start is called before the first frame update
    void Start()
    {
        closestFlag = null;
        reachedStart = false;
        rb = GetComponent<Rigidbody2D>();
        rigTransform = rb.transform;
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!reachedStart)
        {
            MoveToStart();
        }
        else
        {
            MoveToFlag();
        }

        PointToNearestEnemy();

        // check for middle mouse click
        if (Input.GetMouseButtonDown(2))
        {
            // get mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // check if the click occurred over the current billion
            if (GetComponent<Collider2D>().OverlapPoint(mousePos))
            {
                TakeDamage(clickDamage);
            }
        }

    }


    // finds nearest flag and moves billion toward it
    private void MoveToFlag()
    {
        // check for new flags (if new ones can be created
        bool newFlags = false;
        if (numFlags < 2)
        {
            int actualNumFlags = transform.parent.gameObject.GetComponent<BillionareBase>().numFlags;
            if (actualNumFlags != numFlags)
            {
                numFlags = actualNumFlags;
                newFlags = true;
            }

        }

        // if a flag is not set, or the flag's position changes, find (new) closest flag
        if (closestFlag == null || closestFlag.position != flagLastPos || newFlags)
        {
            // reuse code from flag placement to find nearest flag

            // get all items tagged withflag
            //GameObject[] allFlags = GameObject.FindGameObjectsWithTag("flag");
            List<GameObject> allFlags = BillionFlag.GetAllFlagsByColor(billionColor);

            // find nearest flag (only if there are flags on the field)
            if (allFlags.Count > 0)
            {
                float closestDist = Mathf.Infinity;
                closestFlag = allFlags[0].transform;

                // do a simple for loop through the flags to find the closest one to the mouse position
                foreach (GameObject f in allFlags)
                {
                    float currentDist = Vector3.Distance(transform.position, f.transform.position);
                    if (currentDist < closestDist)
                    {
                        closestDist = currentDist;
                        closestFlag = f.transform;
                        flagLastPos = new Vector3(f.transform.position.x, f.transform.position.y, f.transform.position.z);
                        startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                        //slope = maxSpeed / (Vector2.Distance(startPosition, flagLastPos) / 2);
                        //Debug.Log("Slope: " + slope);
                    }
                }

                // protect against case where there are only flags of different colors
                if (closestFlag.gameObject.GetComponent<BillionFlag>().flagColor != billionColor)
                {
                    closestFlag = null;
                    rb.velocity = Vector3.zero;
                }
            }
            // no flags on the field, make closestFlag null again (should already be null, but this is safe)
            else
            {
                closestFlag = null;
                rb.velocity = Vector3.zero;
            }
        }
        // if a flag is set, move toward it
        else
        {
            float distToFlag = Vector2.Distance(transform.position, closestFlag.position);
            Vector2 dir = (closestFlag.position - transform.position).normalized;


            //Debug.Log("Distance: " + distToFlag);
            //Debug.Log("Direction: " + dir);
            //Debug.Log("Slope: " + slope);

            // alternate linear function
            /*
            Debug.Log("Formula: " + dir * ((distToFlag * slope * -1) + (maxSpeed * 2 + .1f)));
            if (distToFlag < (Vector2.Distance(startPosition, flagLastPos) / 2))
            {
                rb.velocity = dir * (distToFlag * slope);
            }
            else
            {
                //Vector2 v = dir * ((distToFlag * slope * -1) + (maxSpeed * 2));
                rb.velocity = dir * ((distToFlag * slope * -1) + (maxSpeed * 2 + .1f));
            }
            */

            // set velocity based on sinusoidal function with distance as the x axis and velocity on the y axis
            rb.velocity = dir * (maxSpeed * Mathf.Sin(distToFlag * ((2 * Mathf.PI) / (Vector2.Distance(startPosition, flagLastPos) * 2))) + .1f);
            //Debug.Log(rb.velocity);

        }
    }


    // moves billion to start position, pushing other billions out of the way
    private void MoveToStart()
    {
        /*
        float xDiff = moveTo.x - rigTransform.position.x;
        float yDiff = rigTransform.position.y - moveTo.y;

        float xChange = xDiff < .1f ? xDiff : .1f;
        float yChange = yDiff < .1f ? yDiff : .1f;

        rigTransform.position = new Vector3(rigTransform.position.x + xChange, rigTransform.position.y - yChange, rigTransform.position.z);

        reachedStart = rigTransform.position.x == moveTo.x && rigTransform.position.y == moveTo.y;
        */

        float xDiff = moveTo.x - rigTransform.position.x;
        float yDiff = moveTo.y - rigTransform.position.y;

        float xVel;
        float yVel;
        if (Mathf.Abs(xDiff) < .25f)
        {
            xVel = 0;
        } else if (xDiff < 0)
        {
            xVel = -.1f;
        } else
        {
            xVel = .1f;
        }

        if (Mathf.Abs(yDiff) < .25f)
        {
            yVel = 0;
        } else if (yDiff < 0)
        {
            yVel = -.1f;
        } else
        {
            yVel = .1f;
        }

        rb.velocity = new Vector2(xVel, yVel);
        List<GameObject> allFlags = BillionFlag.GetAllFlagsByColor(billionColor); // check if there are any flags out and don't bother moving to starting point if so
        reachedStart = (yVel == 0 && xVel == 0) || (allFlags.Count > 0);
    }



    public void TakeDamage(float damageDealt)
    {
        // decrease health by amount of damage taken
        currentHealth -= damageDealt;

        // destroy the billion if its health drops to or below zero
        if (currentHealth <= 0 )
        {
            Die();
            return; // prevent any other code from executing
        }

        // reduce size of inner circle with lerp
        float scale = Mathf.Lerp(minCircleSize, 1, currentHealth / maxHealth); // use lerp to get scale
        Debug.Log("Health %: " + currentHealth / maxHealth);
        Debug.Log("Scale: " + scale);
        gameObject.transform.GetChild(0).transform.localScale = new Vector3(scale, scale, gameObject.transform.GetChild(0).transform.localScale.z); // update scale of just InnerCircle child object
    }



    private void Die()
    {
        Destroy(gameObject);
    }



    private void PointToNearestEnemy()
    {
        // Get nearest enemy
        GameObject nearestEnemy = GetNearestEnemyBillion();
        if (nearestEnemy == null) // return if there are no enemy billions
        {
            return;
        }
        Vector2 nearestEnemyPos = nearestEnemy.transform.position;

        Vector2 thisPos = transform.position; // convert the position of this billion to a vector 2

        // get rotation angle
        Vector2 diff = nearestEnemyPos - thisPos; // get difference between the two vectors
        float angleRad = Mathf.Atan2(diff.y, diff.x);
        float angleDeg = angleRad * Mathf.Rad2Deg;

        // update rotation angle
        transform.eulerAngles = new Vector3(0, 0, angleDeg);

        // shoot if enough time has passed and there is an enemy within range
        if (Vector2.Distance(thisPos, nearestEnemyPos) <= shootDistance && Time.time >= nextFire)
        {
            // spawn at end of turret
            float distFromCenter = GetComponent<CircleCollider2D>().radius; // get distance from center that bullet should spawn at
            Vector3 startPos = (Vector2)transform.position + (diff.normalized * distFromCenter); // set start position to be distFromCenter away from the center of the billion
            startPos.z = transform.position.z;

            // instantiate bullet
            GameObject bullet = Instantiate(bulletPrefab, startPos, Quaternion.identity);

            // rotate bullet according to direction billion is facing
            bullet.transform.eulerAngles = new Vector3(0, 0, angleDeg-90);
            //Debug.Log(billionColor + " spawned at: " + startPos + " | billion at: " + transform.position + " | actually at: " + bullet.transform.position);

            // give bullet necessary information
            //bullet.GetComponent<BillionBullet>().bulletColor = billionColor;
            bullet.GetComponent<BillionBullet>().startPosition = startPos;
            bullet.GetComponent<BillionBullet>().direction = diff.normalized;

            // set time for next fire
            nextFire = Time.time + shootInterval;
        }
        
    }


    private GameObject GetNearestEnemyBillion()
    {
        // get all billions on screen
        GameObject[] allBillions = GameObject.FindGameObjectsWithTag("billion");

        // make sure we return null if there are no enemy billions
        if (allBillions.Length == 0 )
        {
            return null;
        }

        // get closest billion
        GameObject nearestBillion = allBillions[0];
        float nearestDist = Mathf.Infinity;
        foreach (GameObject billion in allBillions)
        {
            float currentDist = Vector2.Distance(this.transform.position, billion.transform.position); // use vector2 distance so that z values aren't considered in calculation

            // check that billion is enemy and that it is closer than the previous closest
            if (billion.GetComponent<Billion>().billionColor != billionColor && currentDist < nearestDist)
            {
                nearestBillion = billion;
                nearestDist = currentDist;
            }
        }

        return nearestBillion;
    }
}
