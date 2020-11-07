using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthBar;

    public Transform spawn;

    public float health = 100f;

    // Update is called once per frame
    void Update()
    {
        healthBar.GetComponent<HealthBar>().SetHealth(health);

        if (health <= 0 || gameObject.transform.position.y < -30)
        {
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
            GameObject.FindObjectOfType<CustomImageEffect>().material.SetFloat("_Amount", 0);
            health = 100;
        }
    }
}
