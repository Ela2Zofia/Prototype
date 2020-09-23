using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    [Range(0.1f, 100.0f)] public float senstivity = 2.0f;
    public bool invertX = false;
    public bool invertY = false;
    public Transform cam;
    
    private float camVerticalAngle = 0f;

    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Time.timeScale == 1)
        {
            MouseLook();
        }
        
    }


    void MouseLook()
    {
        float horizontal = senstivity * Input.GetAxisRaw("Mouse X");
        float vertical = -senstivity * Input.GetAxisRaw("Mouse Y");
        float yLook = transform.eulerAngles.y;
        if (invertX)
        {
            horizontal = -horizontal;
        }
        if (invertY)
        {
            vertical = -vertical;
        }
        camVerticalAngle += vertical;

        // Keep the camera from going upside dow
        camVerticalAngle = Mathf.Clamp(camVerticalAngle, -90f, 90f);

        cam.localEulerAngles = new Vector3(camVerticalAngle, 0f, 0f);

        rb.rotation *= Quaternion.Euler(0f, horizontal, 0f);

    }
}
