using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxCheckScript : MonoBehaviour
{
    public enum Hitmarker {Body, Head};
    public Hitmarker hitmarkerType;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Health: " + this.transform.root.GetComponent<EnemyScript>().health);
    }

    private void OnTriggerEnter(Collider other) {
        if (hitmarkerType == Hitmarker.Body && other.gameObject.tag == "bullet")
        {
            // Decrease health of enemy (set reference) *CAN ACCESS BY DOING (e.g. this.transform.root.GetComponent<'scriptname'>().health)
        }
        else if (hitmarkerType == Hitmarker.Head && other.gameObject.tag == "bullet")
        {
            // Decrease health of enemy
        }

    }
}
