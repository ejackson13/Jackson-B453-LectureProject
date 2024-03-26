using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(float damageDealt, string attackerColor);

    public void Die();


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
