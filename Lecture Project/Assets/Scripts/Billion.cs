using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Billion : MonoBehaviour, IDamagable
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
    public float maxHealth; // the amount of starting health the billion gets
    public float currentHealth; // the amount of health the billion currently has
    public float clickDamage = 25; // the amount of damage the billion takes when being clicked
    public float xpValue = 10; // the amount of xp the killing base gets when this billion is killed
    
    public int level; // the level of the billion
    public float healthMultiplier = 25; // the amount the level is multiplied by to get the amount of health each billion has
    public float damageMultiplier = 10; // the amount the level is multiplied by to get the amount of damage the billion does

    public GameObject bulletPrefab; // the prefab of the bullets that we shoot
    public float shootDistance = 5f; // the max distance to an enemy billion where a billion will fire
    public float shootInterval = 1.5f; // the interval on which billions will shoot 
    private float nextFire = 0; // the time in seconds from game start at which the billion can fire its next shot
    public float bulletDamage; // the damage the bullet does

    // Start is called before the first frame update
    void Start()
    {
        closestFlag = null;
        reachedStart = false;
        rb = GetComponent<Rigidbody2D>();
        rigTransform = rb.transform;

        maxHealth = 75 + (level * healthMultiplier);
        currentHealth = maxHealth;
        bulletDamage = 15 + (level * damageMultiplier);

        // set level indicator
        transform.GetChild(3).GetChild(level-1).gameObject.SetActive(true);
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

        /*
        // check for middle mouse click
        if (Input.GetMouseButtonDown(2))
        {
            // get mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // check if the click occurred over the current billion
            if (GetComponent<Collider2D>().OverlapPoint(mousePos))
            {
                TakeDamage(clickDamage, "");
            }
        }
        */

    }


    // finds nearest flag and moves billion toward it
    private void MoveToFlag()
    {
        // get all flags of the billion's color
        List<GameObject> allFlags = BillionFlag.GetAllFlagsByColor(billionColor);

        // find nearest flag (only if there are flags on the field)
        if (allFlags.Count > 0)
        {
            float closestDist = Mathf.Infinity;
            Transform closestFlagFound = allFlags[0].transform;

            // do a simple for loop through the flags to find the closest one to the mouse position
            foreach (GameObject f in allFlags)
            {
                float currentDist = Vector3.Distance(transform.position, f.transform.position);
                if (currentDist < closestDist)
                {
                    closestDist = currentDist;
                    closestFlagFound = f.transform;
                    //flagLastPos = new Vector3(f.transform.position.x, f.transform.position.y, f.transform.position.z);
                    //startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                }
            }

            // check that the flag moved before moving toward it
            if (closestFlag == null || closestFlagFound.position != closestFlag.position)
            {
                closestFlag = closestFlagFound;
                flagLastPos = new Vector3(closestFlag.position.x, closestFlag.position.y, closestFlag.position.z);
                startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            }

            // if a flag is set, move toward it
            float distToFlag = Vector2.Distance(transform.position, closestFlag.position);
            Vector2 dir = (closestFlag.position - transform.position).normalized;

            // set velocity based on sinusoidal function with distance as the x axis and velocity on the y axis
            rb.velocity = dir * (maxSpeed * Mathf.Sin(distToFlag * ((2 * Mathf.PI) / (Vector2.Distance(startPosition, flagLastPos) * 2))) + .1f);
        }
        // no flags on the field, make closestFlag null again (should already be null, but this is safe)
        else
        {
            closestFlag = null;
            rb.velocity = Vector3.zero;
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



    public void TakeDamage(float damageDealt, string attackerColor)
    {
        // decrease health by amount of damage taken
        currentHealth -= damageDealt;

        // destroy the billion if its health drops to or below zero
        if (currentHealth <= 0 )
        {
            // get the base whose billion killed this billion
            GameObject[] allBases = GameObject.FindGameObjectsWithTag("base");
            GameObject killingBase = null;
            foreach (GameObject b in allBases)
            {
                if (b.GetComponent<BillionareBase>().baseColor == attackerColor)
                {
                    killingBase = b;
                    break;
                }
            }

            // dole out experience points to base whose billion killed this billion
            if (killingBase != null)
            {
                killingBase.GetComponent<BillionareBase>().GainXP(xpValue);
            }

            Die();
            return; // prevent any other code from executing
        }

        // reduce size of inner circle with lerp
        float scale = Mathf.Lerp(minCircleSize, 1, currentHealth / maxHealth); // use lerp to get scale
        gameObject.transform.GetChild(0).transform.localScale = new Vector3(scale, scale, gameObject.transform.GetChild(0).transform.localScale.z); // update scale of just InnerCircle child object
    }



    public void Die()
    {
        Destroy(gameObject);
    }



    private void PointToNearestEnemy()
    {
        // Get nearest enemy
        GameObject nearestEnemy = GetNearestEnemy();
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
            bullet.GetComponent<BillionBullet>().bulletDamage = bulletDamage;
            bullet.GetComponent<BillionBullet>().startPosition = startPos;
            bullet.GetComponent<BillionBullet>().direction = diff.normalized;

            // set time for next fire
            nextFire = Time.time + shootInterval;
        }
        
    }


    private GameObject GetNearestEnemy()
    {
        // get all enemies on screen
        List<GameObject> allEnemies = IDamagable.GetAllEnemyDamagableObjects(billionColor);

        // make sure we return null if there are no enemies
        if (allEnemies.Count == 0 )
        {
            return null;
        }

        // get closest enemy
        GameObject nearestEnemy = allEnemies[0];
        float nearestDist = Mathf.Infinity;
        foreach (GameObject enemy in allEnemies)
        {
            float currentDist = Vector2.Distance(this.transform.position, enemy.transform.position); // use vector2 distance so that z values aren't considered in calculation

            // check that enemy is closer than the previous closest
            if (currentDist < nearestDist)
            {
                nearestEnemy = enemy;
                nearestDist = currentDist;
            }
        }

        return nearestEnemy;
    }








    // wrapper for GameObject.FindGameObjectsWithTag() that returns all enemy billions (all billions that don't match the given color)
    public static List<GameObject> GetAllEnemyBillions(string testColor)
    {
        GameObject[] allBillions = GameObject.FindGameObjectsWithTag("billion");
        List<GameObject> billionsOfColor = new List<GameObject>();

        foreach (GameObject billion in allBillions)
        {
            if (billion.GetComponent<Billion>().billionColor != testColor)
            {
                billionsOfColor.Add(billion);
            }
        }
        return billionsOfColor;
    }
}
