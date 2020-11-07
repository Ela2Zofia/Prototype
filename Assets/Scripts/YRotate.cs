using UnityEngine;

public class YRotate : MonoBehaviour
{
    void Update()
    {
        transform.eulerAngles = transform.eulerAngles + (new Vector3(0, 70f, 0)) * Time.deltaTime;
    }
}
