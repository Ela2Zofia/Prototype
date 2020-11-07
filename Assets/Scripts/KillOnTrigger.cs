using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 8)
        {
            FindObjectOfType<PlayerHealth>().health = -100;
        }
    }
}
