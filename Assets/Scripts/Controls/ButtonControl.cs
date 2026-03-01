using System.Collections.Generic;
using UnityEngine;

public class ButtonControl : MonoBehaviour, IInteractable
{
    [SerializeField] bool pressed;
    [SerializeField] Transform button;
    [SerializeField] Transform pressedTransform;
    [SerializeField] Transform releasedTransform;
    [SerializeField] bool locked;
    [SerializeField] SoundController pressSound;
    [SerializeField] SoundController releaseSound;

    PressData currentPress;
    Stack<PressData> pressHistory = new();

    public bool Pressed
    {
        get => pressed;
        set => pressed = value;
    }

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
            if (pressHistory.Count > 1)
            {
                var prev = pressHistory.Peek();
                while (pressHistory.Count > 0 && prev.pressTime > TimeLoop.CurrentTime)
                {
                    currentPress = prev;
                    pressHistory.Pop();
                }
            }
        }
        else
        {
            while (pressHistory.Count > 1 && pressHistory.Peek().pressTime > TimeLoop.CurrentTime)
            {
                pressHistory.Pop();
            }
        }

        pressed = !locked && currentPress.IsPressed;
        button.position = Vector3.Lerp(releasedTransform.position, pressedTransform.position, pressed ? 1f : 0f);
    }

    public bool AllowInteraction() => TimeLoop.IsPlaying && !locked;
    public bool InteractionLocksCamera() => false;
    public float GetInteractionCameraSpeedMultiplier() => 0.5f;

    public void StartInteraction(Vector3 worldPos)
    {
        currentPress = new PressData
        {
            pressTime = TimeLoop.CurrentTime,
            releaseTime = float.MaxValue
        };
        if (pressSound != null) pressSound.Play();
    }

    public void UpdateInteraction(Vector3 worldPos, Vector2 moveDelta)
    {

    }

    public void EndInteraction(Vector3 worldPos)
    {
        currentPress.releaseTime = TimeLoop.CurrentTime;
        pressHistory.Push(currentPress);
        if (releaseSound != null) releaseSound.Play();
    }

    struct PressData
    {
        public float pressTime;
        public float releaseTime;

        public readonly bool IsPressed => TimeLoop.CurrentTime >= pressTime && TimeLoop.CurrentTime < releaseTime;
        public readonly bool IsReleased => TimeLoop.CurrentTime >= releaseTime;
    }
}
