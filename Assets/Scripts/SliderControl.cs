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

    Snapshot currentSnapshot;
    Stack<Snapshot> snapshots = new();

    public float Value
    {
        get => value;
        set => this.value = Mathf.Clamp(value, minValue, maxValue);
    }

    public bool Locked
    {
        get => locked;
        set => locked = value;
    }

    void Awake()
    {
        currentSnapshot = new Snapshot
        {
            time = TimeLoop.CurrentTime,
            value = value
        };
        snapshots = new();
        snapshots.Push(currentSnapshot);
        InteractableProxy.Make(handle.gameObject, this);
    }

    void Update()
    {
        if (TimeLoop.IsRewinding)
        {
            if (snapshots.Count > 0)
            {
                var prev = snapshots.Peek();
                while (prev.time > currentSnapshot.time)
                {
                    ApplySnapshot(prev);
                    currentSnapshot = prev;
                    snapshots.Pop();
                }
                if (snapshots.Count > 0)
                {
                    prev = snapshots.Peek();
                    BlendSnapshots(prev, currentSnapshot, TimeLoop.CurrentTime);
                }
                else
                {
                    ApplySnapshot(currentSnapshot);
                }
            }
        }
        else
        {
            while (snapshots.Count > 0 && snapshots.Peek().time > TimeLoop.CurrentTime)
            {
                snapshots.Pop();
            }

            currentSnapshot.time = TimeLoop.CurrentTime;
            currentSnapshot.value = value;
            var hasChanged = snapshots.Count == 0 || currentSnapshot.value != snapshots.Peek().value;
            var debounceTimePassed = currentSnapshot.time - snapshots.Peek().time >= snapshotDebounceTime;
            if (hasChanged && debounceTimePassed)
            {
                snapshots.Push(currentSnapshot);
            }
        }

        var t = Mathf.InverseLerp(minValue, maxValue, value);
        handle.position = Vector3.Lerp(minTransform.position, maxTransform.position, t);
    }

    public bool AllowInteraction() => TimeLoop.IsPlaying && !locked;
    public bool InteractionLocksCamera() => false;

    public void StartInteraction(Vector3 worldPos)
    {

    }

    public void UpdateInteraction(Vector3 worldPos, Vector2 moveDelta)
    {
        var minToMax = maxTransform.position - minTransform.position;
        var minToHandle = worldPos - minTransform.position;
        var projectedLength = Vector3.Dot(minToHandle, minToMax.normalized);
        var t = Mathf.Clamp01(projectedLength / minToMax.magnitude);
        value = Mathf.Lerp(minValue, maxValue, t);
    }

    public void EndInteraction(Vector3 worldPos)
    {

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
