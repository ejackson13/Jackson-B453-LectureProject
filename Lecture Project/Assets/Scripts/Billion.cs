using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billion : MonoBehaviour
{

    public string color;
    public Vector3 moveTo;
    public bool reachedStart;
    private Transform rigTransform;

    // Start is called before the first frame update
    void Start()
    {
        reachedStart = false;
        rigTransform = GetComponent<Rigidbody2D>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!reachedStart)
        {
            float xDiff = moveTo.x - rigTransform.position.x;
            float yDiff = rigTransform.position.y - moveTo.y;

            float xChange = xDiff < .1f ? xDiff : .1f;
            float yChange = yDiff < .1f ? yDiff : .1f;
            
            rigTransform.position = new Vector3(rigTransform.position.x + xChange, rigTransform.position.y - yChange, rigTransform.position.z);

            reachedStart = rigTransform.position.x == moveTo.x && rigTransform.position.y == moveTo.y;
        }
    }
}
