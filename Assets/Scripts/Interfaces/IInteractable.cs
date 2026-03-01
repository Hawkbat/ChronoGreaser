using UnityEngine;

public interface IInteractable
{
    public bool AllowInteraction();
    public bool InteractionLocksCamera();
    public float GetInteractionCameraSpeedMultiplier();
    public void StartInteraction(Vector3 worldPos);
    public void UpdateInteraction(Vector3 worldPos, Vector2 moveDelta);
    public void EndInteraction(Vector3 worldPos);
}
