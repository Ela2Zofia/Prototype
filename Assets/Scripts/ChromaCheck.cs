using UnityEngine;

public class ChromaCheck : MonoBehaviour
{
    Material mat;
    void OnTriggerEnter(Collider collider)
    {
        mat = GameObject.FindObjectOfType<CustomImageEffect>().material;
        if (collider.gameObject.layer == 8)
        {
            Debug.Log("impact");
            if (mat.GetFloat("_Amount") != 0) 
            {
                mat.SetFloat("_Amount", 0);
            }
            else
            {
                mat.SetFloat("_Amount", 0.004f);
            }
        }

    }
}
