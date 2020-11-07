using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    public Gradient gradient;

    public Image img;

    public TextMeshProUGUI num;

    void Update()
    {
        num.text = slider.value.ToString();
    }

    public void SetHealth(float health)
    {
        slider.value = health;
        img.color = gradient.Evaluate(slider.normalizedValue);
    }
}
