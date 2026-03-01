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

    bool isInteracting;
    PressData currentPress;
    Stack<PressData> pressHistory = new();

    public bool Pressed => pressed && !TimeLoop.IsRewinding;

    public bool Locked
    {
        get => locked;
        set => locked = value;
    }

    void Awake()
    {
        pressed = false;
        isInteracting = false;
        currentPress = new PressData
        {
            pressTime = 0f,
            releaseTime = 0f
        };
        pressHistory = new();
        InteractableProxy.Make(button.gameObject, this);
    }

    void Update()
    {
        while (pressHistory.Count > 0 && pressHistory.Peek().pressTime > TimeLoop.CurrentTime)
        {
            pressHistory.Pop();
        }
        if (isInteracting)
        {
            pressed = !locked && currentPress.IsPressed;
        }
        else if (pressHistory.Count > 0)
        {
            pressed = pressHistory.Peek().IsPressed;
        }
        else
        {
            pressed = false;
        }
        button.position = Vector3.Lerp(releasedTransform.position, pressedTransform.position, pressed ? 1f : 0f);
    }

    public bool AllowInteraction() => TimeLoop.IsPlaying && !locked;
    public bool InteractionLocksCamera() => false;
    public float GetInteractionCameraSpeedMultiplier() => 0.5f;

    public void StartInteraction(Vector3 worldPos)
    {
        isInteracting = true;
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
        isInteracting = false;
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
