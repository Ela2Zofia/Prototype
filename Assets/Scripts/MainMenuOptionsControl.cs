using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuOptionsControl : MonoBehaviour
{
    public Slider sens;
    public Slider fov;
    public Slider volume;
    public AudioMixer audioMixer;

    public TextMeshProUGUI sensNum;
    public TextMeshProUGUI fovNum;
    public TextMeshProUGUI volumeNum;

    void Start()
    {
        sens.value = Setting.sensitivity;
        fov.value = Setting.fov;
        volume.value = Setting.volume;
    }

    void Update()
    {
        sensNum.text = sens.value.ToString();
        fovNum.text = fov.value.ToString();
        volumeNum.text = (80+volume.value).ToString();
    }

    public void SetSensitivity(float sens)
    {
        Setting.sensitivity = sens;
    }

    public void SetFov(float fov)
    {
        Setting.fov = fov;
    }

    public void SetVolume(float audio)
    {
        Setting.volume = audio;
        audioMixer.SetFloat("Volume", audio);
    }
}
