using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public Image overlay;

    bool isLoading;
    int dots;
    float tmrDots;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isLoading)
        {
            isLoading = true;
            loadingText.GetComponent<BlinkingText>().canBlink = false;
            loadingText.text = "Loading";

            overlay.GetComponent<Animator>().SetTrigger("quickFadeIn");

            StartCoroutine(LoadGame());
        }

        if (isLoading)
        {
            tmrDots += Time.deltaTime;
            if (tmrDots >= 0.3f)
            {
                dots++;
                if (dots < 4)
                {
                    loadingText.text += ".";
                } else
                {
                    loadingText.text = "Loading";
                    dots = 0;
                }
                
                tmrDots = 0;
            }
        }
    }

    IEnumerator LoadGame()
    {
        yield return new WaitForSeconds(3f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
