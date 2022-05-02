using UnityEngine;
using System.Collections;
using Cinemachine;
using TaloGameServices;

public class Player : Car
{
    Animator cameraAnimator;
    public float changeLaneCooldown = 0.25f;

    Light[] brakeLights;

    public float speedLost;

    protected override void Start()
    {
        base.Start();
        cameraAnimator = FindObjectOfType<CinemachineStateDrivenCamera>().GetComponent<Animator>();
        OnLaneChanged += (amount, newLaneName) => StartCoroutine("HandleChangeLaneCooldown");
        OnDirectionChanged += (_) => PlaySound("turn" + Random.Range(0, 3));

        brakeLights = GetComponentsInChildren<Light>();
        HandleBrakeLights(false);
    }

    void HandleBrakeLights(bool on)
    {
        foreach (var light in brakeLights)
        {
            light.enabled = on;
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleInput();
    }

    void HandleInput()
    {
        if (isStopping) return;

        if (!isBraking && canChangeLane)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                ChangeLane(1);
                Talo.Events.Track("Turn Right");
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {

                ChangeLane(-1);
                Talo.Events.Track("Turn Left");
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Talo.Events.Track("Brake");

            StartCoroutine("HandleBrake");
            PlaySound("brake");
            HandleBrakeLights(true);
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            isBraking = true;
            cameraAnimator.SetBool("isBraking", true);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
        {
            isBraking = false;
            cameraAnimator.SetBool("isBraking", false);
            HandleBrakeLights(false);
        }
    }

    IEnumerator HandleChangeLaneCooldown()
    {
        PlaySound("turn" + Random.Range(0, 3));

        canChangeLane = false;
        yield return new WaitForSeconds(changeLaneCooldown);
        canChangeLane = true;
        yield return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameController.instance.DidPlayerLose) return;

        if (collision.gameObject.CompareTag("Cop"))
        {
            rb.AddForce(collision.GetContact(0).normal * 16f, ForceMode.Impulse);
            StartCoroutine("HandleCaught", collision.gameObject.tag);

            Talo.Stats.Track("police-deaths");

        }
        else if (collision.gameObject.CompareTag("Blockade"))
        {
            Talo.Stats.Track("blockades-hit");

            StartCoroutine("HandleCaught", collision.gameObject.tag);
        } else if (collision.gameObject.CompareTag("Caltrops"))
        {
            Talo.Stats.Track("caltrops-hit");

            speed -= 1f;
            brakeSpeed -= 0.2f;

            speedLost += 1f;

            StartCoroutine("HandleCaltropsHit");
        }
        else if (collision.gameObject.CompareTag("Missile"))
        {
            Talo.Stats.Track("missile-deaths");

            isStopping = true;
            rb.AddExplosionForce(900f, transform.position + (Vector3.left * Random.Range(-1f, 1f)), 30f, 40f);

            StartCoroutine("HandleCaught", collision.gameObject.tag);
        }
    }

    IEnumerator HandleCaught(string tag)
    {
        HandleBrakeLights(false);

        GameController.instance.DidPlayerLose = true;

        isStopping = true;
        isBraking = false;

        cameraAnimator.SetBool("isBraking", true);
        cameraAnimator.SetTrigger("caught");

        PlaySound("crash");

        if (tag != "Missile") rb.AddExplosionForce(300f, transform.position, 3f, 16f);

        yield return null;
    }

    void PlaySound(string sound)
    {
        AudioSource sfx = GetComponent<AudioSource>();
        var clip = Resources.Load($"Sounds/{sound}") as AudioClip;
        sfx.pitch = Random.Range(0.8f, 1.2f);
        sfx.PlayOneShot(clip);
    }

    IEnumerator HandleCaltropsHit()
    {
        PlaySound("caltrophit");
        yield return new WaitForSeconds(0.2f);
        PlaySound("caltrops" + Random.Range(0, 2));
    }
}
