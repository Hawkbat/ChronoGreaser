using System.Collections.Generic;
using UnityEngine;

public class SliderControl : MonoBehaviour, IDraggable
{
    [SerializeField] float value;
    [SerializeField] float minValue = 0f;
    [SerializeField] float maxValue = 1f;
    [SerializeField] Transform handle;
    [SerializeField] Transform minTransform;
    [SerializeField] Transform maxTransform;
    [SerializeField] float snapshotDebounceTime = 0.1f;

    bool isDragging = false;
    Snapshot currentSnapshot;
    Stack<Snapshot> snapshots = new();

    void Awake()
    {
        isDragging = false;
        currentSnapshot = new Snapshot
        {
            time = TimeLoop.CurrentTime,
            value = value
        };
        snapshots = new();
        snapshots.Push(currentSnapshot);
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

    public void StartDragging()
    {
        isDragging = true;
    }

    public void ApplyDrag(Vector3 worldTarget, Vector2 dragDelta)
    {
        var dragAxisOffset = maxTransform.position - minTransform.position;
        var dragAxisLength = dragAxisOffset.magnitude;
        var dragAxis = dragAxisOffset.normalized;
        var pointOnLine = Vector3.Project(worldTarget - minTransform.position, dragAxis) + minTransform.position;
        var projectedLength = Vector3.Distance(minTransform.position, pointOnLine);
        var t = Mathf.Clamp01(projectedLength / dragAxisLength);
        value = Mathf.Lerp(minValue, maxValue, t);
    }

    public void EndDragging()
    {
        isDragging = false;
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
