using System.Configuration;
using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    public float senstivity = Setting.sensitivity;
    public bool invertX = Setting.invertX;
    public bool invertY = Setting.invertY;
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

        cam.localEulerAngles = new Vector3(camVerticalAngle, 0f, cam.localEulerAngles.z);

        rb.rotation *= Quaternion.Euler(0f, horizontal, 0f);

    }

    public void SetSens(float Sens)
    {
        senstivity = Sens;
    }
}
