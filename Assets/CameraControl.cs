using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject player;
    
    private float camVerticalAngle = 0f;
    private float senstivity;
    private bool invertY;

    // Start is called before the first frame update
    void Awake()
    {
        senstivity = player.GetComponent<PlayerControl>().senstivity;
        invertY = player.GetComponent<PlayerControl>().invertY;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 1)
        {
            VerticalMouseLook();
        }
        
    }

    void VerticalMouseLook()
    {
        float vertical = -senstivity * Input.GetAxis("Mouse Y");

        if (invertY)
        {
            vertical = -vertical;
        }

        camVerticalAngle += vertical;

        // Keep the camera from going upside dow
        camVerticalAngle = Mathf.Clamp(camVerticalAngle, -90f, 90f);


        transform.localEulerAngles = new Vector3(camVerticalAngle, 0f, 0f);
    }
}
