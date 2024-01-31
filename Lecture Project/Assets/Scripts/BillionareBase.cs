using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillionareBase : MonoBehaviour
{
    public string color;
    public GameObject billionPrefab;
    public float spawnInterval;
    private IEnumerator billionSpawner;
    private CircleCollider2D collider;
    private Vector3 spawnPosition;
    private Vector3 moveTo;

    // Start is called before the first frame update
    void Start()
    {
        billionSpawner = Spawn_Billions();
        StartCoroutine(billionSpawner);
        collider = GetComponent<CircleCollider2D>();

        // set spawn position of billions to be in bottom right corner of bases
        //spawnPosition = new Vector3(transform.position.x + (collider.radius * transform.localScale.x + .1f), transform.position.y - (collider.radius * transform.localScale.x + .1f), transform.position.z);
        spawnPosition = transform.position;
        moveTo = new Vector3(transform.position.x + (collider.radius * transform.localScale.x + .1f), transform.position.y - (collider.radius * transform.localScale.x + .1f), transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
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
        billion.GetComponent<Billion>().color = color;
        billion.GetComponent<Billion>().moveTo = moveTo;
    }
}
