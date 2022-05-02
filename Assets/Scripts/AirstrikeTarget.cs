using UnityEngine;
using PathCreation;

public class AirstrikeTarget : MonoBehaviour
{
    Player player;
    bool isVisible;
    MeshRenderer mr;
    float tmrBlink, tmrShoot;
    PathCreator lane;

    public GameObject missilePrefab;
    GameObject missile;
    bool fired;
    float distance;

    public float missileForce = 100f, minTimeBetweenStrikes = 10f, maxTimeBetweenStrikes = 20f;
    float nextSpawnTime, tmrSpawn;

    AudioSource sfx;


    private void Start()
    {
        mr = GetComponent<MeshRenderer>();
        mr.enabled = false;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        sfx = GetComponent<AudioSource>();

        SetNextSpawnTime();
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes);
        tmrSpawn = 0;
    }

    void GoInvisible()
    {
        isVisible = false;
        mr.enabled = false;
        fired = false;
        lane = null;
    }

    private void Update()
    {
        if (GameController.instance.DidPlayerLose)
        {
            if (isVisible) GoInvisible();
            return;
        }

        if (isVisible)
        {
            tmrBlink += Time.deltaTime;
            if (tmrBlink >= 0.2f)
            {
                mr.enabled = !mr.enabled;
                if (mr.enabled) sfx.Play();
                tmrBlink = 0;
            }

            tmrShoot += Time.deltaTime;
            if (tmrShoot >= distance * 0.0765f)
            {
                DropMissile();
                tmrShoot = 0;
            }
        } else
        {
            if (GameController.instance.CanSpawnAirstrike())
            {
                tmrSpawn += Time.deltaTime;
                if (tmrSpawn >= nextSpawnTime)
                {
                    lane = GameObject.Find(player.GetCurrentLaneName()).GetComponent<PathCreator>();
                    distance = 30f + (player.speedLost * 0.75f);
                    var newPos = lane.path.GetPointAtDistance(player.GetDistance() + distance) + new Vector3(0, 0.015f, 0);

                    transform.position = newPos;

                    isVisible = true;
                    mr.enabled = false;

                    tmrSpawn = 0;

                    SetNextSpawnTime();
                }
            }
        }
    }

    void DropMissile()
    {
        if (fired) return;

        missile = Instantiate(missilePrefab, transform.position + new Vector3(0, 20f, 0), Quaternion.identity);
        missile.GetComponent<Rigidbody>().AddForce(Vector3.down * missileForce, ForceMode.Impulse);
        fired = true;

        GoInvisible();
    }
}
