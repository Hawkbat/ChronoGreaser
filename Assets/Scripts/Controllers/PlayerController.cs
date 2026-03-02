using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Vector2 lookSpeed = new Vector2(90f, 90f);
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float snapshotRecordInterval = 0.1f;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] float interactRange = 2f;
    [SerializeField] Image cursor;
    [SerializeField] Color cursorDefaultColor;
    [SerializeField] Color cursorHoverColor;

    InputAction lookAction;
    InputAction moveAction;
    InputAction attackAction;
    InputAction interactAction;
    float cameraAngleX;
    Stack<Snapshot> history;
    Snapshot currentSnapshot;
    IInteractable interactTarget;
    Vector3 lastInteractPos;
    IInteractable hoverTarget;

    CharacterController ctrl;
    Camera cam;

    void Awake()
    {
        cameraAngleX = 0f;
        history = new();

        UpdateCurrentSnapshot();

        lookAction = InputSystem.actions.FindAction("Look");
        moveAction = InputSystem.actions.FindAction("Move");
        attackAction = InputSystem.actions.FindAction("Attack");
        interactAction = InputSystem.actions.FindAction("Interact");
        InputSystem.actions.Enable();

        ctrl = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;

        var timeLoop = TimeLoop.Instance;
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        // Emergency reset if player falls out of the world
        if (transform.position.y < -100f)
        {
            TimeLoop.EmergencyRewind();
        }

        if (!TimeLoop.IsPlaying && interactTarget != null)
        {
            interactTarget.EndInteraction(lastInteractPos);
            interactTarget = null;
        }
        cursor.enabled = TimeLoop.IsPlaying;
        if (TimeLoop.IsRewinding)
        {
            if (history.Count == 0)
            {
                ApplySnapshot(currentSnapshot);
                return;
            }
            var previous = history.Peek();
            while (history.Count > 0 && previous.time >= TimeLoop.CurrentTime)
            {
                currentSnapshot = previous;
                previous = history.Pop();
            }
            if (history.Count > 0)
            {
                ApplySnapshotBlend(previous, currentSnapshot, TimeLoop.CurrentTime);
            }
            else
            {
                ApplySnapshot(currentSnapshot);
            }
            return;
        }
        else if (TimeLoop.IsFastForwarding)
        {
            ctrl.SimpleMove(Vector3.zero);
            return;
        }
        else if (TimeLoop.IsStopped)
        {
            return;
        }

        var dt = TimeLoop.DeltaTime;

        var moveInput = moveAction.ReadValue<Vector2>();
        var lookInput = lookAction.ReadValue<Vector2>();

        var adjustedLookSpeed = new Vector2(lookSpeed.x * Save.Instance.cameraSensitivityX, lookSpeed.y * Save.Instance.cameraSensitivityY);

        if (interactTarget != null)
        {
            var cameraSpeedMultiplier = interactTarget.GetInteractionCameraSpeedMultiplier();
            adjustedLookSpeed *= cameraSpeedMultiplier;
        }

        if (interactTarget == null || !interactTarget.InteractionLocksCamera())
        {
            transform.Rotate(Vector3.up, lookInput.x * dt * adjustedLookSpeed.x);
            cameraAngleX -= lookInput.y * dt * adjustedLookSpeed.y;
            cameraAngleX = Mathf.Clamp(cameraAngleX, -80f, 80f);
            cam.transform.localRotation = Quaternion.Euler(cameraAngleX, 0f, 0f);
        }

        var movement = transform.right * moveInput.x + transform.forward * moveInput.y;
        movement = moveSpeed * TimeLoop.TimeScale * Vector3.ClampMagnitude(movement, 1f);
        ctrl.SimpleMove(movement);

        UpdateCurrentSnapshot();

        if (history.Count == 0 || currentSnapshot.time - history.Peek().time >= snapshotRecordInterval)
        {
            RecordSnapshot();
        }

        hoverTarget = null;
        Vector3 hoverPos = Vector3.zero;
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hitInfo, interactRange, interactableLayer, QueryTriggerInteraction.Collide))
        {

            hoverTarget = hitInfo.collider.GetComponentInParent<IInteractable>();
            hoverPos = hitInfo.point;
        }
        else
        {
            hoverTarget = null;
            hoverPos = ray.GetPoint(interactRange);
        }

        if (interactTarget != null && !interactTarget.AllowInteraction())
        {
            interactTarget.EndInteraction(hoverPos);
            interactTarget = null;
        }

        var interactPressed = attackAction.WasPressedThisFrame() || interactAction.WasPressedThisFrame();
        var interactHeld = attackAction.IsPressed() || interactAction.IsPressed();
        var interactReleased = attackAction.WasReleasedThisFrame() || interactAction.WasReleasedThisFrame();

        if (interactPressed && interactTarget == null && hoverTarget != null && hoverTarget.AllowInteraction())
        {
            interactTarget = hoverTarget;
            interactTarget.StartInteraction(hoverPos);
            lastInteractPos = hoverPos;
        }
        if (interactHeld && interactTarget != null)
        {
            interactTarget.UpdateInteraction(hoverPos, lookInput);
            lastInteractPos = hoverPos;
        }
        if (interactReleased && interactTarget != null)
        {
            interactTarget.EndInteraction(hoverPos);
            interactTarget = null;
        }

        cursor.color = hoverTarget != null ? cursorHoverColor : cursorDefaultColor;
        cursor.enabled = interactTarget == null;
    }

    void UpdateCurrentSnapshot()
    {
        currentSnapshot.time = TimeLoop.CurrentTime;
        currentSnapshot.angleY = transform.eulerAngles.y;
        currentSnapshot.cameraAngleX = cameraAngleX;
        currentSnapshot.positionX = transform.position.x;
        currentSnapshot.positionY = transform.position.y;
        currentSnapshot.positionZ = transform.position.z;
    }

    void RecordSnapshot()
    {
        history.Push(currentSnapshot);
    }

    void ApplySnapshotBlend(Snapshot a, Snapshot b, float time)
    {
        float t = Mathf.InverseLerp(a.time, b.time, time);
        var blendedPosition = Vector3.Lerp(a.Position, b.Position, t);
        var blendedAngleY = Mathf.LerpAngle(a.angleY, b.angleY, t);
        var blendedCameraAngleX = Mathf.Lerp(a.cameraAngleX, b.cameraAngleX, t);
        transform.position = blendedPosition;
        transform.rotation = Quaternion.Euler(0f, blendedAngleY, 0f);
        cam.transform.localRotation = Quaternion.Euler(blendedCameraAngleX, 0f, 0f);
    }

    void ApplySnapshot(Snapshot snapshot)
    {
        transform.position = snapshot.Position;
        transform.rotation = Quaternion.Euler(0f, snapshot.angleY, 0f);
        cam.transform.localRotation = Quaternion.Euler(snapshot.cameraAngleX, 0f, 0f);
    }

    struct Snapshot
    {
        public float time;
        public float angleY;
        public float cameraAngleX;
        public float positionX;
        public float positionY;
        public float positionZ;

        public Vector3 Position
        {
            readonly get => new(positionX, positionY, positionZ);
            set
            {
                positionX = value.x;
                positionY = value.y;
                positionZ = value.z;
            }
        }
    }
}
