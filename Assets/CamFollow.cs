using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public Transform playerCamera;
    public Transform player;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = playerCamera.position;
        transform.eulerAngles = new Vector3(playerCamera.eulerAngles.x, player.eulerAngles.y, player.rotation.z);
    }
}
