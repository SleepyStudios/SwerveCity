using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HazardSpawner : MonoBehaviour
{
    public GameObject[] prefabs;
    float tmrShouldDestroy, tmrShouldSpawn;
    bool shouldDestroy, shouldSpawn;

    void Start()
    {
        GetComponent<MeshFilter>().mesh = null;
    }

    private void OnBecameVisible()
    {
        if (shouldDestroy)
        {
            transform.GetChild(0).GetComponentsInChildren<Collider>().Select((collider) =>
            {
                collider.isTrigger = true;
                return collider;
            });
        }

        shouldDestroy = false;

        shouldSpawn = false;
    }

    private void OnBecameInvisible()
    {
        if (transform.childCount == 0)
        {
            if (GameController.instance.CanSpawnHazard()) shouldSpawn = true;
        } else
        {
            shouldDestroy = true;

            transform.GetChild(0).GetComponentsInChildren<Collider>().Select((collider) =>
            {
                collider.isTrigger = true;
                return collider;
            });
        }
    }

    private void Update()
    {
        if (shouldDestroy)
        {
            tmrShouldDestroy += Time.deltaTime;
            if (tmrShouldDestroy >= 4f)
            {
                Destroy(transform.GetChild(0).gameObject);
                shouldDestroy = false;
                tmrShouldDestroy = 0;

                GameController.instance.UnregisterHazard();
            }
        } else
        {
            tmrShouldDestroy = 0;
        }

        if (shouldSpawn)
        {
            tmrShouldSpawn += Time.deltaTime;
            if (tmrShouldSpawn >= 1)
            {
                Instantiate(prefabs[Random.Range(0, prefabs.Length - 1)], transform.position, Quaternion.identity, transform);
                shouldSpawn = false;
                tmrShouldSpawn = 0;

                GameController.instance.RegisterHazard();
            }
        } else
        {
            tmrShouldSpawn = 0;
        }
    }
}
