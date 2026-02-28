using System.Collections.Generic;
using UnityEngine;

public class ButtonControl : MonoBehaviour, IInteractable
{
    [SerializeField] bool pressed;
    [SerializeField] Transform button;
    [SerializeField] Transform pressedTransform;
    [SerializeField] Transform releasedTransform;
    [SerializeField] bool locked;

    PressData currentPress;
    Stack<PressData> pressHistory = new();

    public bool Locked
    {
        get => locked;
        set => locked = value;
    }

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
        InteractableProxy.Make(button.gameObject, this);
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

    public bool AllowInteraction() => TimeLoop.IsPlaying && !locked;
    public bool InteractionLocksCamera() => false;

    public void StartInteraction(Vector3 worldPos)
    {
        currentPress = new PressData
        {
            pressTime = TimeLoop.CurrentTime,
            releaseTime = float.MaxValue
        };
    }

    public void UpdateInteraction(Vector3 worldPos, Vector2 moveDelta)
    {

    }

    public void EndInteraction(Vector3 worldPos)
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
