using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [SerializeField] Image overlay;

    bool isFading;
    bool fadeIn;
    Color fadeColor;
    float fadeStartTime;
    float fadeEndTime;

    static FadeController instance;

    public static FadeController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<FadeController>();
            }
            return instance;
        }
    }

    public void StartFade(Color color, float duration, bool fadeIn)
    {
        this.fadeColor = color;
        this.fadeIn = fadeIn;
        this.fadeStartTime = TimeLoop.CurrentTime;
        this.fadeEndTime = fadeStartTime + duration;
        isFading = true;
        overlay.enabled = true;
    }

    public void StopFade()
    {
        isFading = false;
        overlay.enabled = false;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        overlay.enabled = false;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Update()
    {
        if (isFading && TimeLoop.CurrentTime <= fadeStartTime)
        {
            isFading = false;
            overlay.enabled = false;
        }

        if (isFading)
        {
            float t = Mathf.Clamp01(Mathf.InverseLerp(fadeStartTime, fadeEndTime, TimeLoop.CurrentTime));
            var c = fadeColor;
            c.a = fadeIn ? 1f - t : t;
            overlay.color = c;
        }
    }
}
