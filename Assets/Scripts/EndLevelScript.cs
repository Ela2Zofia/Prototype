using UnityEngine;
using TMPro;

public class EndLevelScript : MonoBehaviour
{
    public SceneManagement management;
    public TextMeshProUGUI achievement;
    private string output = System.String.Empty;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        display();
    }

    private void display()
    {
        
        if (Collection.pilotKnifeCollected)
        {
            output += "Pilot Knife\n";
        }
        if (Collection.jumpKitCollected)
        {
            output += "Jump Kit\n";
        }
        if (Collection.roninCollected)
        {
            output += "Ronin\n";
        }
        if ((!Collection.pilotKnifeCollected) && (!Collection.jumpKitCollected) && (!Collection.roninCollected))
        {
            output += "Nothing!\n";
        }
        achievement.text = output;

    }

    public void Back()
    {
        management.LoadMainMenu();
    }



}
