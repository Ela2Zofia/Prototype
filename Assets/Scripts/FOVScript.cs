using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Checks if player is in enemy's field of vision
    public bool inSight(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction = direction + Vector3.up*0.8f;
        
        RaycastHit hit;

        int mask = LayerMask.GetMask("Hitmarker"); // Mask used to ignore raycast collision with body hitmarkers
        mask = ~mask;

        // Debug.DrawRay(transform.position, new Vector3(direction.x, direction.y, direction.z));
        // float angleToPlayer = (Vector3.Angle(direction, transform.forward));
        // if (angleToPlayer >= -90 && angleToPlayer <= 90) { // If target is in the 180 degree FOV
        if (Physics.Raycast(transform.position, new Vector3(direction.x, direction.y, direction.z), out hit, Mathf.Infinity, mask)) {
            if (hit.collider.tag == "Player") {
                return true;
            }
        }
        // }
        return false;
    }
}
