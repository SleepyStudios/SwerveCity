using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Siren : MonoBehaviour
{
    void Start()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.volume += Random.Range(-0.2f, 0.2f);
        source.pitch += Random.Range(-0.2f, 0.2f);
        source.Play();
    }
}
