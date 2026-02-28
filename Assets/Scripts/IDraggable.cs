using UnityEngine;

public interface IDraggable
{
    void StartDragging();
    void ApplyDrag(Vector3 worldTarget, Vector2 dragDelta);
    void EndDragging();
}
