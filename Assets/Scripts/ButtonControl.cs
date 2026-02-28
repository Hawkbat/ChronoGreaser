using System.Collections.Generic;
using UnityEngine;

public class ButtonControl : MonoBehaviour, IClickable
{
    [SerializeField] bool pressed;
    [SerializeField] Transform button;
    [SerializeField] Transform pressedTransform;
    [SerializeField] Transform releasedTransform;

    PressData currentPress;
    Stack<PressData> pressHistory = new();

    void Awake()
    {
        pressed = false;
        currentPress = new PressData
        {
            pressTime = 0f,
            releaseTime = 0f
        };
        pressHistory = new();
        pressHistory.Push(currentPress);
    }

    void Update()
    {
        if (TimeLoop.IsRewinding)
        {
            if (pressHistory.Count > 0)
            {
                var prev = pressHistory.Peek();
                while (prev.pressTime > currentPress.pressTime)
                {
                    currentPress = prev;
                    pressHistory.Pop();
                }
            }
        }
        else
        {
            while (pressHistory.Count > 0 && pressHistory.Peek().pressTime > TimeLoop.CurrentTime)
            {
                pressHistory.Pop();
            }
        }
        pressed = currentPress.IsPressed;
        button.position = Vector3.Lerp(releasedTransform.position, pressedTransform.position, pressed ? 1f : 0f);
    }

    public void OnClick(Vector3 worldTarget)
    {

    }

    public void StartClicking()
    {
        currentPress = new PressData
        {
            pressTime = TimeLoop.CurrentTime,
            releaseTime = float.MaxValue
        };
    }

    public void EndClicking()
    {
        currentPress.releaseTime = TimeLoop.CurrentTime;
        pressHistory.Push(currentPress);
    }

    struct PressData
    {
        public float pressTime;
        public float releaseTime;

        public readonly bool IsPressed => TimeLoop.CurrentTime >= pressTime && TimeLoop.CurrentTime < releaseTime;
        public readonly bool IsReleased => TimeLoop.CurrentTime >= releaseTime;
    }
}
