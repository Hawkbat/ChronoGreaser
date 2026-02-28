using UnityEngine;

public class InteractableProxy : MonoBehaviour, IInteractable
{
    [SerializeField] IInteractable target;

    bool colliderWasEnabled;
    Collider col;

    public static InteractableProxy Make(GameObject go, IInteractable target)
    {
        var proxy = go.AddComponent<InteractableProxy>();
        proxy.target = target;
        return proxy;
    }

    void Awake()
    {
        col = GetComponent<Collider>();
    }

    public bool AllowInteraction() => target != null && target.AllowInteraction();
    public bool InteractionLocksCamera() => target != null && target.InteractionLocksCamera();

    public void StartInteraction(Vector3 worldPos)
    {
        if (col != null)
        {
            colliderWasEnabled = col.enabled;
            col.enabled = false;
        }
        if (target != null) target.StartInteraction(worldPos);
    }

    public void UpdateInteraction(Vector3 worldPos, Vector2 moveDelta)
    {
        if (target != null) target.UpdateInteraction(worldPos, moveDelta);
    }

    public void EndInteraction(Vector3 worldPos)
    {
        if (target != null) target.EndInteraction(worldPos);
        if (col != null) col.enabled = colliderWasEnabled;
    }
}
