using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Billion : MonoBehaviour, IDamagable
{
    public string billionColor;
    public Vector3 moveTo;
    private bool reachedStart;
    private Transform rigTransform;
    private Rigidbody2D rb;

    private Transform closestFlag;
    private Vector3 flagLastPos; // the last position of the closestFlag - used so that we can detect when a flag moves and we don't have to check for a new closest flag every frame\
    private Vector3 startPosition; // the position the billion started at when the flag was moved
    [SerializeField] private float maxSpeed; // the max speed the billions can reach

    [SerializeField] private float minCircleSize = .3f; // the minimum size of the inner circle (as a proportion, so between 0 and 1)
    [SerializeField] private float maxHealth; // the amount of starting health the billion gets
    [SerializeField] private float currentHealth; // the amount of health the billion currently has
    [SerializeField] private float xpValue = 10; // the amount of xp the killing base gets when this billion is killed
    
    public int level; // the level of the billion
    [SerializeField] private float healthMultiplier = 25; // the amount the level is multiplied by to get the amount of health each billion has
    [SerializeField] private float damageMultiplier = 10; // the amount the level is multiplied by to get the amount of damage the billion does

    [SerializeField] private GameObject bulletPrefab; // the prefab of the bullets that we shoot
    public Sprite bulletSprite; // the sprite of the bullets we shoot
    [SerializeField] private float shootDistance = 5f; // the max distance to an enemy billion where a billion will fire
    [SerializeField] private float shootInterval = 1.5f; // the interval on which billions will shoot 
    [SerializeField] private float nextFire = 0; // the time in seconds from game start at which the billion can fire its next shot
    [SerializeField] private float bulletDamage; // the damage the bullet does

    // Start is called before the first frame update
    void Start()
    {
        // set variables
        closestFlag = null;
        reachedStart = false;
        rb = GetComponent<Rigidbody2D>();
        rigTransform = rb.transform;

        // set max health and damage based on level
        maxHealth = 75 + (level * healthMultiplier);
        currentHealth = maxHealth;
        bulletDamage = 15 + (level * damageMultiplier);

        // set level indicator
        transform.GetChild(3).GetChild(level-1).gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        // move to the start position if we haven't reached it and if there are no flags
        if (!reachedStart)
        {
            MoveToStart();
        }
        else
        {
            MoveToFlag();
        }

        // always point to nearest enemy
        PointToNearestEnemy();

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
                }
            }

            // check that the flag moved before moving toward it
            if (closestFlag == null || closestFlagFound.position != flagLastPos)
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
        // get the difference to the goal position
        float xDiff = moveTo.x - rigTransform.position.x;
        float yDiff = moveTo.y - rigTransform.position.y;

        float xVel;
        float yVel;
        // set x velocity
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

        // set y velocity
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

        // set the velocity
        rb.velocity = new Vector2(xVel, yVel);
        List<GameObject> allFlags = BillionFlag.GetAllFlagsByColor(billionColor); // check if there are any flags out and don't bother moving to starting point if so
        reachedStart = (yVel == 0 && xVel == 0) || (allFlags.Count > 0); // stop moving to start if the start point has been reached or if there is a flag on the field
    }



    // handles whenever the billion is damaged by enemy bullets
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



    // destroys the game object
    public void Die()
    {
        Destroy(gameObject);
    }



    // angles the billion at the nearest enemy billion or base and fires if they are close enough
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
            bullet.GetComponent<BillionBullet>().bulletColor = billionColor;
            bullet.GetComponent<SpriteRenderer>().sprite = bulletSprite;
            bullet.GetComponent<BillionBullet>().bulletDamage = bulletDamage;
            bullet.GetComponent<BillionBullet>().startPosition = startPos;
            bullet.GetComponent<BillionBullet>().direction = diff.normalized;

            // set time for next fire
            nextFire = Time.time + shootInterval;
        }
        
    }


    // returns the nearest enemy billion or base to the current billion
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
