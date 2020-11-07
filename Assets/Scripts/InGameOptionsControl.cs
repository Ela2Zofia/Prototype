using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
public class InGameOptionsControl : MonoBehaviour
{
    public AudioMixer audioMixer;
    public SceneManagement scene;
    public Slider sens;
    public Slider fov;
    public Slider volume;
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
        FindObjectOfType<PlayerRotate>().SetSens(sens);
    }

    public void SetFov(float fov)
    {
        Setting.fov = fov;
        FindObjectOfType<PlayerControl>().SetFov(fov);
    }

    public void SetVolume(float volume)
    {
        Setting.volume = volume;
        audioMixer.SetFloat("Volume", volume);
    }


    public void ReturnToMain()
    {
        scene.LoadMainMenu();
    }

}
