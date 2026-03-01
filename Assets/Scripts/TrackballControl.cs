using System.Collections.Generic;
using UnityEngine;

public class TrackballControl : MonoBehaviour, IInteractable
{
    [SerializeField] Vector2 value;
    [SerializeField] Vector2 rotationScale;
    [SerializeField] Transform ball;
    [SerializeField] float snapshotDebounceTime = 0.1f;
    [SerializeField] bool locked;
    [SerializeField] SoundController pressSound;
    [SerializeField] SoundController releaseSound;

    Snapshot currentSnapshot;
    Stack<Snapshot> snapshots = new();

    public Vector2 Value
    {
        get => value;
        set => this.value = value;
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
        InteractableProxy.Make(ball.gameObject, this);
    }

    void Update()
    {
        if (TimeLoop.IsRewinding)
        {
            if (snapshots.Count > 1)
            {
                var prev = snapshots.Peek();
                while (snapshots.Count > 1 && prev.time > currentSnapshot.time)
                {
                    ApplySnapshot(prev);
                    currentSnapshot = prev;
                    snapshots.Pop();
                }
                if (snapshots.Count > 1)
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
            while (snapshots.Count > 1 && snapshots.Peek().time > TimeLoop.CurrentTime)
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

        ball.localRotation = Quaternion.Euler(value.x * rotationScale.x, value.y * rotationScale.y, 0f);
    }

    public bool AllowInteraction() => TimeLoop.IsPlaying && !locked;
    public bool InteractionLocksCamera() => true;
    public float GetInteractionCameraSpeedMultiplier() => 0.5f;

    public void StartInteraction(Vector3 worldPos)
    {
        if (pressSound != null) pressSound.Play();
    }

    public void UpdateInteraction(Vector3 worldPos, Vector2 moveDelta)
    {
        value += moveDelta;
    }

    public void EndInteraction(Vector3 worldPos)
    {
        if (releaseSound != null) releaseSound.Play();
    }

    void ApplySnapshot(Snapshot snapshot)
    {
        value = snapshot.value;
    }

    void BlendSnapshots(Snapshot from, Snapshot to, float time)
    {
        var t = Mathf.InverseLerp(from.time, to.time, time);
        value = Vector2.Lerp(from.value, to.value, t);
    }

    struct Snapshot
    {
        public float time;
        public Vector2 value;
    }
}
