using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBlockLevel3Script : MonoBehaviour
{
    public GameObject enemyGroup;
    // Start is called before the first frame update
    void Start()
    {
        enemyGroup = GameObject.FindWithTag("EnemyGroup");
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.tag == "DoorBlock" && enemyGroup.transform.childCount == 0) {
            Destroy(gameObject);
        }
    }
}
