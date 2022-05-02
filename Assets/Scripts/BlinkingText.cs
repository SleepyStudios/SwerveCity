using UnityEngine;
using TMPro;

public class BlinkingText : MonoBehaviour
{
    string originalText;
    TextMeshProUGUI tm;
    float tmrBlink;
    public bool canBlink = true;

    void Start()
    {
        tm = GetComponent<TextMeshProUGUI>();
        originalText = tm.text;
    }

    void Update()
    {
        tmrBlink += Time.deltaTime;
        if (tmrBlink >= 0.3f && canBlink)
        {
            tm.text = tm.text == "" ? originalText : "";
            tmrBlink = 0;
        }
    }
}
