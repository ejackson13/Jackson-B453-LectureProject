using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class BillionareBase : MonoBehaviour, IDamagable
{
    [Header("General Info")]
    public string baseColor;
    [SerializeField] private float spawnInterval;

    [Header("Prefabs and sprites")]
    [SerializeField] private GameObject billionPrefab;
    [SerializeField] private GameObject flagPrefab;

    private IEnumerator billionSpawner;
    private CircleCollider2D circleCollider;
    private LineRenderer lineRenderer;

    private Vector3 spawnPosition;
    private Vector3 moveTo;
    private int numFlags; // the number of flags of the current color currently on screen
    private GameObject flagClicked; // the initial position the flag that is being clicked and dragged
    private bool wasFlagClicked = false; // used to track whether or not a flag was initially clicked (to determine if a line needs to be drawn when clicking and dragging)


    [Header("Shooting variables")]
    [SerializeField] private float turnSpeed = 2f; // the max speed at which the base will rotate in a given frame (in degrees)
    [SerializeField] private GameObject bulletPrefab; // the prefab of the bullets that we shoot
    public float shootDistance = 5f; // the max distance to an enemy billion where a billion will fire
    [SerializeField] private float shootInterval = 1.5f; // the interval on which billions will shoot 
    private float nextFire = 0; // the time in seconds from game start at which the billion can fire its next shot
    [SerializeField] private float bulletSpeed = 3f; // the speed at which a bullet will travel
    [SerializeField] private float bulletDistance = 3; // the max distance a bullet will travel
    [SerializeField] private float bulletDamage = 40; // the damage the bullet deals

    [Header("Health and XP")]
    [SerializeField] private float maxHealth = 200; // the amount of starting health the base gets
    private float currentHealth; // the amount of health the base currently has
    [SerializeField] private float xpValue = 20; // the amount of xp the killing base gets when this base is killed
    private float currentXp = 0; // the current amount of xp the base has
    [SerializeField] private float nextLevel = 100; // the amount of xp the base needs to get to the next level
    private int level = 1; // the level of the base

    private Image healthBar; // the image containing the radial health bar UI element
    private Image xpBar; // the image containing the radial xp bar UI element
    private TextMeshProUGUI levelText; // the text box containing the current level

    // Start is called before the first frame update
    void Start()
    {
        billionSpawner = Spawn_Billions();
        StartCoroutine(billionSpawner);
        circleCollider = GetComponent<CircleCollider2D>();
        numFlags = 0;

        // set up line renderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] {Vector3.zero, Vector3.zero});

        // set spawn position of billions to be in bottom right corner of bases
        spawnPosition = transform.position;
        moveTo = new Vector3(transform.position.x + (circleCollider.radius * transform.localScale.x + .1f), transform.position.y - (circleCollider.radius * transform.localScale.x + .1f), transform.position.z);

        // get the UI elements
        healthBar = transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>();
        xpBar = transform.GetChild(1).GetChild(1).gameObject.GetComponent<Image>();
        levelText = transform.GetChild(1).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();

        // initialize UI values
        healthBar.fillAmount = 1;
        xpBar.fillAmount = 0;

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // use if statements so left clicking places blue flags and right clicking places yellow flags
        if (baseColor == "blue")
        {
            // call Place_Flag with the mouse input for placing flags as the left mouse button
            Put_Flag(0);
        }
        else if (baseColor == "yellow")
        {
            // call Place_Flag with the mouse input for placing flags as the right mouse button
            Put_Flag(1);
        }


        PointToNearestEnemy();
    }


    // method that handles all mouse input for placing a flag
    private void Put_Flag(int mouseButton)
    {
        // get mouse position as world point
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;

        // if the mouse button is clicked and the player is not on a flag, find the nearest flag and move it to that location
        // it they do click on a flag, assume they are going to click and drag, so set initialMousePosition and flagClicked
        if (Input.GetMouseButtonDown(mouseButton))
        {
            Debug.Log(baseColor + " click");
            // check if the mouse is over a flag collider

            // get all flags (all game objects that are tagged as flags)
            GameObject intersectingFlag = null; // the first flag that intersects with the current mouse position, set to null initially
            GameObject[] allFlags = GameObject.FindGameObjectsWithTag("flag");

            // do a simple for loop through the flags to check if any of their colliders overlap with the mouse position
            foreach (GameObject f in allFlags)
            {
                if (f.GetComponent<Collider2D>().OverlapPoint(mousePos) && f.GetComponent<BillionFlag>().flagColor == baseColor)
                {
                    // set intersecting flag to be the first flag that intersects with the mousePos
                    // if multiple flags intersect the mouse position, one will be chosen arbitrarily
                    intersectingFlag = f;
                    break;
                }
            }

            
            // check if the mouse is on a flag (also avoid drawing line if there are less than 2 flags)
            if (intersectingFlag != null && numFlags == 2)
            {
                Debug.Log(baseColor + " flag clicked");
                // store the flag we clicked and set up the line renderer
                flagClicked = intersectingFlag;
                wasFlagClicked = true;
                lineRenderer.enabled = true;
                lineRenderer.SetPositions(new Vector3[] {flagClicked.transform.position, mousePos });
            }
            else
            {
                Place_or_Move_Flag(mousePos);
            }
        }
        // if the player is holding down the mouse button and their original click was on a flag, draw a 
        else if (Input.GetMouseButton(mouseButton) && wasFlagClicked)
        {
            // draw line from initialFlagPosition to mousePos - only need to update position of 
            lineRenderer.SetPosition(1, mousePos);
        }
        // if the player is clicking and dragging, as soon as the mouse button is released the flag should be either placed or moved
        else if (Input.GetMouseButtonUp(mouseButton) && wasFlagClicked)
        {
            Place_or_Move_Flag(mousePos);
        }
    }

    
    // method that either moves or instantiates a flag at the given location
    private void Place_or_Move_Flag(Vector3 pos)
    {
        // if there are less than 2 flags, instantiate new flag
        if (numFlags < 2)
        {
            Debug.Log("Create new " + baseColor);
            GameObject newFlag = Instantiate(flagPrefab, pos, Quaternion.identity);
            newFlag.transform.SetParent(transform, true); // make the flag a child of the current base (keeping the world position of the flag as set in the previous line)
            newFlag.GetComponent<BillionFlag>().flagColor = baseColor; // set the color of the flag in its script
            numFlags++;
        }
        // make sure that if a flag is being clicked and dragged, that flag is moved
        else if (wasFlagClicked)
        {
            flagClicked.transform.position = pos;
        }
        // find closest flag and set its position to be the mouse position
        else
        {
            // get all flags (all game objects that are tagged as flags)
            GameObject[] allFlags = GameObject.FindGameObjectsWithTag("flag");
            float closestDist = Mathf.Infinity;
            GameObject closestFlag = allFlags[0]; // should be safe because there must be at least two flags in order for this part of the if/else to be reached

            // do a simple for loop through the flags to find the closest one to the mouse position
            foreach (GameObject f in allFlags) 
            { 
                if (f.GetComponent<BillionFlag>().flagColor == baseColor)
                {
                    float currentDist = Vector3.Distance(pos, f.transform.position);
                    if (currentDist < closestDist)
                    {
                        closestDist = currentDist;
                        closestFlag = f;
                    }
                }
            }

            // update the position of the closest flag
            closestFlag.transform.position = pos;
        }
        
        
        // excesive but safe to reset these after every flag is placed
        wasFlagClicked = false;
        lineRenderer.enabled = false;
    }


    // Coroutine that continually spawns billions at a specified interval
    IEnumerator Spawn_Billions()
    {
        yield return new WaitForSeconds(spawnInterval);
        while (true)
        {
            Instantiate_Billion();

            yield return new WaitForSeconds(spawnInterval);
        }
    }


    // spawns billions and gives them the necessary information
    void Instantiate_Billion()
    {
        
        // create billion with given spawn location (I don't set the new billion as a child because of issues with scaling, and I'll just wait till it becomes a problem to change it)
        GameObject billion = Instantiate(billionPrefab, spawnPosition, Quaternion.identity);

        // set color of billion so it knows what to attack (might not be necessary depending on how I structure the rest of the code)
        billion.GetComponent<Billion>().billionColor = baseColor;
        billion.GetComponent<Billion>().moveTo = moveTo;
        billion.GetComponent<Billion>().level = level;
        billion.transform.SetParent(transform, true); // make billions children of the base that spawns them
    }


    // rotates base to aim at nearest enemy and shoot at it if it is close enough
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

        // get rotation angle to billion
        Vector2 diff = nearestEnemyPos - thisPos; // get difference between the two vectors
        float angleRad = Mathf.Atan2(diff.y, diff.x);
        float angleDeg = angleRad * Mathf.Rad2Deg;
        if (angleDeg < 0)
        {
            angleDeg += 360;
        }
        else if (angleDeg >= 360)
        {
            angleDeg -= 360;
        }

        // get current rotation angle
        float currentRot = transform.GetChild(0).transform.eulerAngles.z;
        if (currentRot < 0)
        {
            currentRot += 360;
        } else if (currentRot >= 360){
            currentRot -= 360;
        }

        // update rotation angle
        float newRot = 0;
        float deltaRot = angleDeg - currentRot;
        if (Mathf.Abs(deltaRot) <= turnSpeed)
        {
            newRot = angleDeg;
        }
        else
        {
            float turnAmt = turnSpeed;

            // rotate cw or ccw depending on shortest distance to face the billion
            if (deltaRot > 180)
            {
                turnAmt *= -1;
            }
            else if (deltaRot < 0 && deltaRot > -180)
            {
                turnAmt *= -1;
            }
            newRot = currentRot + turnAmt;
        }

        transform.GetChild(0).transform.eulerAngles = new Vector3(0, 0, newRot);

        // shoot if enough time has passed and there is an enemy within range
        if (Vector2.Distance(thisPos, nearestEnemyPos) <= shootDistance && Time.time >= nextFire)
        {
            // spawn at end of turret
            float distFromCenter = GetComponent<CircleCollider2D>().radius * transform.localScale.x + (bulletPrefab.GetComponent<BoxCollider2D>().size.y/2); // get distance from center that bullet should spawn at
            Vector2 direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * newRot), Mathf.Sin(Mathf.Deg2Rad * newRot));
            Vector3 startPos = (Vector2)transform.position + (direction.normalized * distFromCenter); // set start position to be distFromCenter away from the center of the billion
            startPos.z = transform.position.z;

            // instantiate bullet
            GameObject bullet = Instantiate(bulletPrefab, startPos, Quaternion.identity);

            // rotate bullet according to direction billion is facing
            bullet.transform.eulerAngles = new Vector3(0, 0, newRot - 90);

            // give bullet necessary information
            //bullet.GetComponent<BillionBullet>().bulletColor = billionColor;
            bullet.GetComponent<BillionBullet>().startPosition = startPos;
            bullet.GetComponent<BillionBullet>().direction = direction.normalized;
            bullet.GetComponent <BillionBullet>().bulletSpeed = bulletSpeed;
            bullet.GetComponent<BillionBullet>().bulletDistance = bulletDistance;
            bullet.GetComponent<BillionBullet>().bulletDamage = bulletDamage;

            // set time for next fire
            nextFire = Time.time + shootInterval;
        }

    }


    // returns the enemy billion or base nearest to this base
    private GameObject GetNearestEnemy()
    {
        // get all enemies on screen
        List<GameObject> allEnemies = IDamagable.GetAllEnemyDamagableObjects(baseColor);

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



    // handles the base taking damage and dying if the damage is lethal
    public void TakeDamage(float damageDealt, string attackerColor)
    {
        // decrease health by amount of damage taken
        currentHealth -= damageDealt;

        // destroy the base if its health drops to or below zero
        if (currentHealth <= 0)
        {
            // get the base whose billion killed this base
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

            // dole out experience points to base whose billion killed this base
            if (killingBase != null)
            {
                killingBase.GetComponent<BillionareBase>().GainXP(xpValue);
            }

            Die();
            return; // prevent any other code from executing
        }

        // decrease angle of radial health bar
        healthBar.fillAmount = currentHealth / maxHealth;
        
    }


    // destroys the base
    public void Die()
    {
        Destroy(gameObject);
    }



    // handles gaining xp when an enemy billion or base is killed
    public void GainXP(float xpAmount)
    {
        currentXp += xpAmount;

        if (currentXp >= nextLevel)
        {
            LevelUp();
        }


        // update radial xp bar
        xpBar.fillAmount = currentXp / nextLevel;
    }


    // handles leveling up when the xp threshold is reached
    public void LevelUp()
    {
        if (level == 9)
        {
            return;
        }

        // carry over xp to next level
        currentXp = currentXp - nextLevel;

        // increase threshold
        nextLevel = (int)(nextLevel * 1.75f);

        // increase level
        level++;


        // update UI
        levelText.text = $"{level}";
    }
}
