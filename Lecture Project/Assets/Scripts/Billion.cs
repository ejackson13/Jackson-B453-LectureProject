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

    // Start is called before the first frame update
    void Start()
    {
        closestFlag = null;
        reachedStart = false;
        rb = GetComponent<Rigidbody2D>();
        rigTransform = rb.transform;
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
                }
            }
            // no flags on the field, make closestFlag null again (should already be null, but this is safe)
            else
            {
                closestFlag = null;
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
        float xDiff = moveTo.x - rigTransform.position.x;
        float yDiff = rigTransform.position.y - moveTo.y;

        float xChange = xDiff < .1f ? xDiff : .1f;
        float yChange = yDiff < .1f ? yDiff : .1f;

        rigTransform.position = new Vector3(rigTransform.position.x + xChange, rigTransform.position.y - yChange, rigTransform.position.z);

        reachedStart = rigTransform.position.x == moveTo.x && rigTransform.position.y == moveTo.y;
    }
}
