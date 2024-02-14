using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BillionFlag : MonoBehaviour
{
    public string flagColor;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }




    // wrapper for GameObject.FindGameObjectsWithTag() that returns all flags of a given color
    public static List<GameObject> GetAllFlagsByColor(string testColor)
    {
        GameObject[] allFlags = GameObject.FindGameObjectsWithTag("flag");
        List<GameObject> flagsOfColor = new List<GameObject>();
        
        foreach (GameObject flag in allFlags)
        {
            if (flag.GetComponent<BillionFlag>().flagColor == testColor)
            {
                flagsOfColor.Add(flag);
            }
        }

        return flagsOfColor;
    }
}
