using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneManagement : MonoBehaviour
{
    private const int SCENE_NUMBER = 5;

    public Canvas pauseMenu;

    void Update()
    {
        if(SceneManager.GetActiveScene().buildIndex != 0)
        {
            pauseGame();
        }
    }

    void pauseGame()
    {
        if (Input.GetKeyDown("escape"))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
                pauseMenu.gameObject.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                pauseMenu.gameObject.SetActive(false);
            }
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1;
    }
    
    public void LoadSceneNum(int num)
    {
        SceneManager.LoadScene(num);
    }
    public void NextScene()
    {
        if (SceneManager.GetActiveScene().buildIndex < SCENE_NUMBER)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            LoadMainMenu();
        }
        GameObject.FindObjectOfType<CustomImageEffect>().material.SetFloat("_Amount", 0);
    }

}
