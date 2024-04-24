using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(float damageDealt, string attackerColor);

    public void Die();


    // static helper method to get all enemy billions and bases based on a given base/billion color
    public static List<GameObject> GetAllEnemyDamagableObjects(string testColor)
    {
        // get all enemy billions
        GameObject[] allBillions = GameObject.FindGameObjectsWithTag("billion");
        List<GameObject> allDamagable = new List<GameObject>();
        foreach (GameObject billion in allBillions)
        {
            if (billion.GetComponent<Billion>().billionColor != testColor)
            {
                allDamagable.Add(billion);
            }
        }

        // get all enemy bases
        GameObject[] allBases = GameObject.FindGameObjectsWithTag("base");
        foreach (GameObject cbase in allBases)
        {
            if (cbase.GetComponent<BillionareBase>().baseColor != testColor)
            {
                allDamagable.Add(cbase);
            }
        }

        return allDamagable;
    }
}
