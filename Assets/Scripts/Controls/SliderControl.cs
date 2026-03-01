using System.Collections.Generic;
using UnityEngine;

public class SliderControl : MonoBehaviour, IInteractable
{
    [SerializeField] float value;
    [SerializeField] float minValue = 0f;
    [SerializeField] float maxValue = 1f;
    [SerializeField] Transform handle;
    [SerializeField] Transform minTransform;
    [SerializeField] Transform maxTransform;
    [SerializeField] float snapshotDebounceTime = 0.1f;
    [SerializeField] bool locked;
    [SerializeField] SoundController pressSound;
    [SerializeField] SoundController releaseSound;

    float initialValue;
    bool isInteracting;
    Vector3 interactOffset;
    Snapshot rewindSnapshot;
    Stack<Snapshot> snapshots = new();

    public float Value
    {
        get => value;
        set
        {
            this.value = Mathf.Clamp(value, minValue, maxValue);
            UpsertSnapshot();
        }
    }

    public bool Locked
    {
        get => locked;
        set => locked = value;
    }

    public bool IsInteracting => isInteracting;

    void Awake()
    {
        initialValue = value;
        snapshots = new();
        InteractableProxy.Make(handle.gameObject, this);
    }

    void Update()
    {
        if (!TimeLoop.IsRewinding)
        {
            rewindSnapshot = new Snapshot
            {
                time = 0,
                value = initialValue,
            };
        }

        while (snapshots.Count > 0 && snapshots.Peek().time > TimeLoop.CurrentTime)
        {
            rewindSnapshot = snapshots.Pop();
        }
        if (snapshots.Count > 0)
        {
            var prev = snapshots.Peek();
            if (TimeLoop.IsRewinding && prev.time < rewindSnapshot.time)
            {
                BlendSnapshots(prev, rewindSnapshot, TimeLoop.CurrentTime);
            }
            else
            {
                ApplySnapshot(prev);
            }
        }
        else
        {
            value = initialValue;
        }

        var t = Mathf.InverseLerp(minValue, maxValue, value);
        handle.position = Vector3.Lerp(minTransform.position, maxTransform.position, t);
    }

    public bool AllowInteraction() => TimeLoop.IsPlaying && !locked;
    public bool InteractionLocksCamera() => false;
    public float GetInteractionCameraSpeedMultiplier() => 0.1f;

    public void StartInteraction(Vector3 worldPos)
    {
        if (pressSound != null) pressSound.Play();
        isInteracting = true;
        interactOffset = handle.position - worldPos;
    }

    public void UpdateInteraction(Vector3 worldPos, Vector2 moveDelta)
    {
        var handlePos = worldPos + interactOffset;
        var minToMax = maxTransform.position - minTransform.position;
        var minToHandle = handlePos - minTransform.position;
        var projectedLength = Vector3.Dot(minToHandle, minToMax.normalized);
        var t = Mathf.Clamp01(projectedLength / minToMax.magnitude);
        value = Mathf.Lerp(minValue, maxValue, t);
        UpsertSnapshot();
    }

    public void EndInteraction(Vector3 worldPos)
    {
        if (releaseSound != null) releaseSound.Play();
        isInteracting = false;
    }

    public void SetMinMax(float min, float max)
    {
        minValue = min;
        maxValue = max;
        value = Mathf.Clamp(value, minValue, maxValue);
        UpsertSnapshot();
    }

    void UpsertSnapshot()
    {
        if (snapshots.Count > 0)
        {
            var prev = snapshots.Peek();
            if (Mathf.Approximately(prev.value, value))
                return;
            if (TimeLoop.CurrentTime - prev.time < snapshotDebounceTime)
            {
                snapshots.Pop();
            }
        }
        snapshots.Push(new Snapshot
        {
            time = TimeLoop.CurrentTime,
            value = value
        });
    }

    void ApplySnapshot(Snapshot snapshot)
    {
        value = snapshot.value;
    }

    void BlendSnapshots(Snapshot from, Snapshot to, float time)
    {
        var t = Mathf.InverseLerp(from.time, to.time, time);
        value = Mathf.Lerp(from.value, to.value, t);
    }

    struct Snapshot
    {
        public float time;
        public float value;
    }
}
