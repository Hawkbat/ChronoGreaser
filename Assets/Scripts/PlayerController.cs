using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Vector2 lookSpeed = new Vector2(90f, 90f);
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float snapshotRecordInterval = 0.1f;

    InputAction lookAction;
    InputAction moveAction;
    float cameraAngleX;
    Stack<Snapshot> history;
    Snapshot currentSnapshot;
    Snapshot initialSnapshot;

    CharacterController ctrl;
    Camera cam;

    void Awake()
    {
        cameraAngleX = 0f;
        history = new();

        UpdateCurrentSnapshot();
        initialSnapshot = currentSnapshot;

        lookAction = InputSystem.actions.FindAction("Look");
        moveAction = InputSystem.actions.FindAction("Move");
        InputSystem.actions.Enable();

        ctrl = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;

        var timeLoop = TimeLoop.GetInstance();
        timeLoop.OnReset.AddListener(OnTimeLoopReset);
        timeLoop.OnRewinding.AddListener(OnTimeLoopRewinding);
        timeLoop.OnFastForwarded.AddListener(OnTimeLoopFastForwarded);
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;

        var timeLoop = TimeLoop.GetInstance();
        if (timeLoop != null)
        {
            timeLoop.OnReset.RemoveListener(OnTimeLoopReset);
            timeLoop.OnRewinding.RemoveListener(OnTimeLoopRewinding);
            timeLoop.OnFastForwarded.RemoveListener(OnTimeLoopFastForwarded);
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(100f);
        GUILayout.Label("Snapshots: " + history.Count);
        GUILayout.Label("Estimated Memory Usage: " + history.Count * sizeof(float) * 6f / 1024f + " KB");
    }

    void Update()
    {
        if (TimeLoop.IsRewinding)
        {
            if (history.Count == 0)
            {
                ApplySnapshot(currentSnapshot);
                return;
            }
            var previous = history.Peek();
            while (previous.time >= TimeLoop.CurrentTime)
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
        else if (TimeLoop.IsFastForwarding || TimeLoop.IsResetting)
        {
            ctrl.SimpleMove(Vector3.zero);
            return;
        }
        else if (TimeLoop.IsStopped)
        {
            return;
        }

        var dt = TimeLoop.TimeScale * Time.deltaTime;

        var moveInput = moveAction.ReadValue<Vector2>();
        var lookInput = lookAction.ReadValue<Vector2>();

        var adjustedLookSpeed = new Vector2(lookSpeed.x * Save.Instance.cameraSensitivityX, lookSpeed.y * Save.Instance.cameraSensitivityY);

        transform.Rotate(Vector3.up, lookInput.x * dt * adjustedLookSpeed.x);

        var movement = transform.right * moveInput.x + transform.forward * moveInput.y;
        movement = moveSpeed * TimeLoop.TimeScale * Vector3.ClampMagnitude(movement, 1f);
        ctrl.SimpleMove(movement);

        cameraAngleX -= lookInput.y * dt * adjustedLookSpeed.y;
        cameraAngleX = Mathf.Clamp(cameraAngleX, -80f, 80f);
        cam.transform.localRotation = Quaternion.Euler(cameraAngleX, 0f, 0f);

        UpdateCurrentSnapshot();

        if (history.Count == 0 || currentSnapshot.time - history.Peek().time >= snapshotRecordInterval)
         {
             RecordSnapshot();
        }
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

    void OnTimeLoopReset()
    {
        history.Clear();
        ApplySnapshot(initialSnapshot);
        UpdateCurrentSnapshot();
        RecordSnapshot();
    }

    void OnTimeLoopRewinding()
    {
        UpdateCurrentSnapshot();
        RecordSnapshot();
    }

    void OnTimeLoopFastForwarded()
    {
        UpdateCurrentSnapshot();
        RecordSnapshot();
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
