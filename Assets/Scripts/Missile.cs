using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine("HandleDestroy");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Airstrike Target"))
        {
            StartCoroutine("HandleDestroy");
        }
    }

    IEnumerator HandleDestroy()
    {
        var explosion = Instantiate(Resources.Load("Prefabs/Explosion"), transform.position, Quaternion.identity);

        var sfx = GetComponent<AudioSource>();
        sfx.pitch = Random.Range(0.8f, 1.2f);
        sfx.Play();

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().isTrigger = true;

        yield return new WaitForSeconds(1f);
        Destroy(explosion);

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);

        yield return null;
    }
}
