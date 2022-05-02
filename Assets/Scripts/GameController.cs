using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using PathCreation;
using TaloGameServices;
using System.Linq;
using System.Threading.Tasks;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    Player player;

    public TextMeshProUGUI timer, restartText, leaderboardText;
    public Image overlay;

    bool lost;
    public event Action OnPlayerCaught;

    public float score; bool canUpdateScore;

    public float airstrikeScoreRequirement = 30f, reinforcementsScoreRequirement = 60f;

    float tmrReinforcements;
    PathCreator playerLane;

    int registeredHazards;

    string leaderboardName = "score";

    public bool DidPlayerLose {
        get => lost;
        set {
            lost = value;
            if (lost)
            {
                OnPlayerLost();
            }
        }
    }

    private void Awake()
    {
        instance = this;
        player = FindObjectOfType<Player>();

        player.OnLaneChanged += (_, newLaneName) =>
        {
            playerLane = GameObject.Find(newLaneName).GetComponent<PathCreator>();
        };

        string username = PlayerPrefs.GetString("talo-username");

        if (!PlayerPrefs.HasKey("talo-username"))
        {
            var random = new System.Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            username = new string(Enumerable.Range(1, 10).Select(_ => chars[random.Next(chars.Length)]).ToArray());

            PlayerPrefs.SetString("talo-username", username);
            PlayerPrefs.Save();
        }

        Talo.Players.Identify("username", username);

        StartCoroutine("Init");
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(1f);
        canUpdateScore = true;
        yield return null;
    }

    public bool CanSpawnHazard()
    {
        // 0 = 6
        // 30 = 8
        // 60 = 10
        // 90 = 12 etc
        int max = ((int)score / 30) * 2 + 6;
        return registeredHazards < max;
    }

    public void RegisterHazard()
    {
        registeredHazards++;
    }

    public void UnregisterHazard()
    {
        registeredHazards--;
    }

    public bool CanSpawnAirstrike()
    {
        return score >= airstrikeScoreRequirement;
    }

    public bool CanSpawnReinforcements()
    {
        return score >= reinforcementsScoreRequirement;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (lost)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Talo.Events.Track("Restart");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        if (canUpdateScore)
        {
            score += Time.deltaTime;
            timer.text = TimeSpan.FromSeconds(score).ToString("mm\\:ss\\:ff");
        }

        if (CanSpawnReinforcements())
        {
            tmrReinforcements += Time.deltaTime;
            if (tmrReinforcements >= 5f)
            {
                float distanceBehind = 30f;

                if (player.GetDistance() > distanceBehind && player.GetDistance() < playerLane.path.length - distanceBehind)
                {
                    var pos = playerLane.path.GetPointAtDistance(UnityEngine.Random.Range(0f, player.GetDistance() - distanceBehind));

                    Cop cop = Instantiate(Resources.Load("Prefabs/AI Cop") as GameObject, player.transform.position, Quaternion.identity).GetComponent<Cop>();
                    cop.transform.position = pos;
                    cop.startingLane = playerLane;

                    tmrReinforcements = 0f;
                }
            }
        }
    }

    async void OnPlayerLost()
    {
        OnPlayerCaught?.Invoke();

        timer.GetComponent<Animator>().SetTrigger("gameOver");
        overlay.GetComponent<Animator>().SetTrigger("gameOver");
        restartText.GetComponent<Animator>().SetTrigger("gameOver");
        leaderboardText.GetComponent<Animator>().SetTrigger("gameOver");

        var res = await Talo.Leaderboards.AddEntry(leaderboardName, score);
        leaderboardText.text = $"You are currently #{res.Item1.position + 1} in the world\n";

        if (!Mathf.Approximately(res.Item1.score, score))
        {
            if (!res.Item2)
            {
                leaderboardText.text = "You didn't beat your highscore\n" + leaderboardText.text;
            }
            else
            {
                leaderboardText.text = "New highscore\n" + leaderboardText.text;
            }
        }

        var entries = await GetSurroundingEntries(res.Item1.position);
        foreach (var entry in entries)
        {
            string formattedScore = TimeSpan.FromSeconds(entry.score).ToString("mm\\:ss\\:ff");
            leaderboardText.text += $"\n{entry.position + 1}. {formattedScore}";
        }
    }

    async Task<LeaderboardEntry[]> GetSurroundingEntries(int position)
    {
        int page = 0;
        var res = await Talo.Leaderboards.GetEntries(leaderboardName, page);
        if (position == 0)
        {
            return res.entries.Take(3).ToArray();
        } else
        {
            bool found = false;
            var entries = new List<LeaderboardEntry>(res.entries);

            do
            {
                found = entries.Where((entry) => entry.position == position).Count() > 0;
                if (!found)
                {
                    page++;
                    res = await Talo.Leaderboards.GetEntries(leaderboardName, page);
                    entries.AddRange(res.entries);
                }
            } while (!found);

            // check for last place
            if (position == res.count - 1)
            {
                var sorted = entries.OrderByDescending((entry) => entry.position).ToList();
                return sorted.Take(3).Reverse().ToArray();
            }

            return entries.Where((entry) => Math.Abs(entry.position - position) <= 1)
                .OrderBy((entry) => entry.position)
                .Take(3)
                .ToArray();
        }
    }
}
