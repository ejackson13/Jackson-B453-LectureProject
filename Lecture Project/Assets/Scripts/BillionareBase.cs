using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class BillionareBase : MonoBehaviour
{
    public string baseColor;
    public GameObject billionPrefab;
    public float spawnInterval;
    public GameObject flagPrefab;

    private IEnumerator billionSpawner;
    private CircleCollider2D circleCollider;
    private LineRenderer lineRenderer;

    private Vector3 spawnPosition;
    private Vector3 moveTo;
    private int numFlags; // the number of flags of the current color currently on screen
    private GameObject flagClicked; // the initial position the flag that is being clicked and dragged
    private Boolean wasFlagClicked = false; // used to track whether or not a flag was initially clicked (to determine if a line needs to be drawn when clicking and dragging)

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
        //spawnPosition = new Vector3(transform.position.x + (collider.radius * transform.localScale.x + .1f), transform.position.y - (collider.radius * transform.localScale.x + .1f), transform.position.z);
        spawnPosition = transform.position;
        moveTo = new Vector3(transform.position.x + (circleCollider.radius * transform.localScale.x + .1f), transform.position.y - (circleCollider.radius * transform.localScale.x + .1f), transform.position.z);
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
        Debug.Log(baseColor + " place");
        Debug.Log(numFlags);
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


    IEnumerator Spawn_Billions()
    {
        yield return new WaitForSeconds(spawnInterval);
        while (true)
        {
            Instantiate_Billion();

            yield return new WaitForSeconds(spawnInterval);
        }
    }


    void Instantiate_Billion()
    {
        
        // create billion with given spawn location (I don't set the new billion as a child because of issues with scaling, and I'll just wait till it becomes a problem to change it)
        GameObject billion = Instantiate(billionPrefab, spawnPosition, Quaternion.identity);

        // set color of billion so it knows what to attack (might not be necessary depending on how I structure the rest of the code)
        billion.GetComponent<Billion>().billionColor = baseColor;
        billion.GetComponent<Billion>().moveTo = moveTo;
    }
}
