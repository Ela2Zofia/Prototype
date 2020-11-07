using UnityEngine;

public class CollectOnCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Destroy(gameObject);
            if (gameObject.name == "PilotKnife")
            {
                Collection.pilotKnifeCollected = true;
            }else if (gameObject.name == "JumpKit")
            {
                Collection.jumpKitCollected = true;
            }else if (gameObject.name == "Ronin")
            {
                Collection.roninCollected = true;
            }
        }
    }
}
