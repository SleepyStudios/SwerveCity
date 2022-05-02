using System.Collections;
using UnityEngine;

public class SpinningLight : MonoBehaviour
{
    Light lightToSpin;

    public float spinSpeed = 100f;
    public float delay;
    bool spin;

    void Start()
    {
        delay = Random.Range(0.1f, 0.4f);
        lightToSpin = GetComponent<Light>();
        StartCoroutine("Init");
    }

    void Update()
    {
        if (spin)
        {
            lightToSpin.transform.RotateAround(lightToSpin.transform.position, Vector3.up, Time.deltaTime * spinSpeed);
        }
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(delay);
        spin = true;
        yield return null;
    }
}
