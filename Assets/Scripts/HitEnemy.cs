using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEnemy : MonoBehaviour
{
    GameObject enemy;
    EnemyScript enemyScript;
    
    void Start()
    {
        enemy = FindEnemy();
        enemyScript = enemy.GetComponent<EnemyScript>();
    }

    public void Damage(float amount)
    {
        enemy.GetComponent<EnemyScript>().followCd = Time.time + 7.0f;
        enemy.GetComponent<EnemyScript>().health -= amount;
        float health = enemy.GetComponent<EnemyScript>().health;


        if (health <= 0f)
        {
            Die();
        }   
    }
    void Die()
    {
        if (enemy.tag == "BasicEnemy" || enemy.tag == "SoldierEnemy") {
            enemyScript.startDestroyEnemy();
        } else {
            Destroy(enemy);
        }
    }

    private GameObject FindEnemy()
    {
        Transform t = gameObject.transform;
        while (t.parent != null)
        {
        if (t.parent.tag == "BasicEnemy" || t.parent.tag == "SoldierEnemy")
        {
            return t.parent.gameObject;
        }
        t = t.parent.transform;
        }
        return null; // Could not find a parent with given tag.
    }
}
