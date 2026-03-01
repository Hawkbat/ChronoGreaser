using TMPro;
using UnityEngine;

public class ProjectorController : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Color normalColor;
    [SerializeField] Color flickerColor;
    [SerializeField] float flickerDuration = 0.1f;
    [SerializeField] float flickerInterval = 1f;
    [SerializeField] float flickerAmount = 0.02f;

    Vector3 originalCanvasPos;

    private void Awake()
    {
        originalCanvasPos = canvas.transform.localPosition;
    }

    void Update()
    {
        var time = TimeLoop.CurrentTime;
        if (time % flickerInterval < flickerDuration)
        {
            var t = (time % flickerInterval) / flickerDuration;
            var color = Color.Lerp(normalColor, flickerColor, Mathf.Sin(t * Mathf.PI * 2) * 0.5f + 0.5f);
            text.color = color;
            canvas.transform.localPosition = originalCanvasPos + (Vector3)Random.insideUnitCircle * flickerAmount;
        }
        else
        {
            text.color = normalColor;
            canvas.transform.localPosition = originalCanvasPos;
        }
    }
}
