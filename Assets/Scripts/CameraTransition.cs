
using UnityEngine;

public class CameraTransition : MonoBehaviour
{
    public Animator animator;
    public void MainMenuOn()
    {
        animator.SetBool("MainMenuOn", true);
    }
    public void MainMenuOff()
    {
        animator.SetBool("MainMenuOn", false);
    }

    public void AllowCamTrans()
    {
        animator.SetBool("CamTrans", true);
    }
}
