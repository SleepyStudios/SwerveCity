using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cop : Car
{
    Car player;
    float tmrMove;

    List<(Vector3, int)> playerMovements = new List<(Vector3, int)>();
    float nextMoveTime = 0.2f;

    Renderer r;
    float tmrOffscreen;

    protected override void Start()
    {
        base.Start();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Car>();
        player.OnLaneChanged += OnPlayerLaneChangedEvent;
        GameController.instance.OnPlayerCaught += OnPlayerCaughtEvent;
        OnLaneChanged += OnLaneChangedEvent;

        StartCoroutine("Init");

        r = GetComponentInChildren<Renderer>();
    }

    private void OnDisable()
    {
        player.OnLaneChanged -= OnPlayerLaneChangedEvent;
        GameController.instance.OnPlayerCaught -= OnPlayerCaughtEvent;
        OnLaneChanged -= OnLaneChangedEvent;
    }

    void OnLaneChangedEvent(int amount, string newLaneName)
    {
        if (newLaneName == player.GetCurrentLaneName() && playerMovements.Count >= 2)
        {
            playerMovements.Clear();
        }
    }

    void OnPlayerCaughtEvent()
    {
        StartCoroutine("HandleGameOver");
    }

    void OnPlayerLaneChangedEvent(int amount, string newLaneName)
    {
        if (amount != 0) playerMovements.Add((player.transform.position, amount));
    }

    IEnumerator Init()
    {
        GetComponent<Collider>().isTrigger = true;

        isBraking = true;
        yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        speed += Random.Range(-0.1f, 0.75f);
        isBraking = false;

        GetComponent<Collider>().isTrigger = false;

        yield return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine("HandlePlayerCaught");
        } else if (collision.gameObject.CompareTag("Caltrops") || collision.gameObject.CompareTag("Missile"))
        {
            StartCoroutine("HandleHazardHit");
        }
    }

    IEnumerator HandlePlayerCaught()
    {
        isStopping = true;
        yield return null;
    }

    IEnumerator HandleGameOver()
    {
        isStopping = true;
        yield return null;
    }

    protected override void Update()
    {
        base.Update();

        tmrMove += Time.deltaTime;
        if (tmrMove >= nextMoveTime && playerMovements.Count > 0)
        {
            float distance = Vector3.Distance(transform.position, playerMovements[0].Item1);
            if (distance <= maxChange)
            {
                ChangeLane(playerMovements[0].Item2);
                if (playerMovements.Count > 0) playerMovements.RemoveAt(0);
            }
            nextMoveTime = (40f - distance) * 0.01f;

            tmrMove = 0;
        }

        if (!r.isVisible && GameController.instance.CanSpawnReinforcements())
        {
            tmrOffscreen += Time.deltaTime;
            if (tmrOffscreen >= 30f)
            {
                Destroy(gameObject);
                tmrOffscreen = 0;
            }
        }
        else
        {
            tmrOffscreen = 0;
        }
    }

    IEnumerator HandleHazardHit()
    {
        isBraking = true;
        StartCoroutine("HandleBrake");
        yield return new WaitForSeconds(0.5f);

        isBraking = false;
        yield return null;
    }
}
