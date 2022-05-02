using UnityEngine;
using System.Collections;

public class BlockadeCar : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Cop"))
        {
            if (!GameController.instance.DidPlayerLose)
            {
                //StartCoroutine("HandleCopCollision");
            }
        }
    }

    IEnumerator HandleCopCollision()
    {
        yield return new WaitForSeconds(0.5f);
        GetComponent<Collider>().isTrigger = true;
        yield return null;
    }
}
