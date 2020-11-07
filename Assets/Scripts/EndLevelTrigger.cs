using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelTrigger : MonoBehaviour
{

    public SceneManagement manage;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 8)
        {
            manage.NextScene();
        }
        
    }
}
