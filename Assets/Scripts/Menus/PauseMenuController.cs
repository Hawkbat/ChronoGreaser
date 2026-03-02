using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI escapeText;
    [SerializeField] float timeToEscape = 3f;

    string escapeTextBase;
    float escapeTextTimer;

    void Awake()
    {
        escapeTextBase = escapeText.text;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.isPressed)
        {
            var dots = (int)(Time.time * 2) % 4;
            escapeText.text = escapeTextBase + new string('.', dots);
            escapeText.enabled = true;
            escapeTextTimer += Time.deltaTime;

            if (escapeTextTimer >= timeToEscape)
            {
                SceneManager.LoadScene("Startup");
            }
        }
        else
        {
            escapeText.enabled = false;
            escapeTextTimer = 0f;
        }
    }
}
