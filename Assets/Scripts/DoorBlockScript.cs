using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBlockScript : MonoBehaviour
{
    public GameObject enemyGroup1;
    public GameObject enemyGroup2;
    public GameObject enemyGroup3;
    // Start is called before the first frame update
    void Start()
    {
        enemyGroup1 = GameObject.FindWithTag("EnemyGroup1");
        enemyGroup2 = GameObject.FindWithTag("EnemyGroup2");
        enemyGroup3 = GameObject.FindWithTag("EnemyGroup3");
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.tag == "DoorBlock1" && enemyGroup1.transform.childCount == 0) {
            Destroy(gameObject);
        }
        else if (this.gameObject.tag == "DoorBlock2" && enemyGroup2.transform.childCount == 0) {
            Destroy(gameObject);
        }
        else if (this.gameObject.tag == "DoorBlock3" && enemyGroup3.transform.childCount == 0) {
            Destroy(gameObject);
        }
    }
}
