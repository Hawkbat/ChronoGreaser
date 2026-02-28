using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TimeLoop : MonoBehaviour
{
    static TimeLoop instance;

    [SerializeField] float totalDuration = 120f;
    [SerializeField] float timeScale = 1f;
    [SerializeField] float timeScaleMultiplier = 1f;
    [SerializeField] float rewindSpeed = 20f;
    [SerializeField] float fastForwardSpeed = 20f;

    float elapsedTime = 0f;
    float targetTime = 0f;
    bool isStopped = false;
    bool isResetting = false;
    bool isRewinding = false;
    bool isFastForwarding = false;

    public UnityEvent OnStopped = new();
    public UnityEvent OnResetting = new();
    public UnityEvent OnReset = new();
    public UnityEvent OnRewinding = new();
    public UnityEvent OnRewound = new();
    public UnityEvent OnFastForwarding = new();
    public UnityEvent OnFastForwarded = new();
    public UnityEvent OnResume = new();
    public UnityEvent<float> OnTimeScaleChanged = new();

    public static float CurrentTime => instance != null ? instance.elapsedTime : 0f;
    public static float TimeScale
    {
        get
        {
            if (instance == null) return 1f;
            if (IsStopped) return 0f;
            if (IsRewinding) return -instance.rewindSpeed;
            if (IsFastForwarding) return instance.fastForwardSpeed;
            return instance.timeScale * instance.timeScaleMultiplier;
        }
    }
    public static float DeltaTime => instance != null ? Time.deltaTime * TimeScale : Time.deltaTime;
    public static float TotalDuration => instance != null ? instance.totalDuration : 0f;
    public static float NormalizedTime => instance != null ? Mathf.Clamp01(instance.elapsedTime / instance.totalDuration) : 0f;
    public static bool IsStopped => instance != null ? instance.isStopped : false;
    public static bool IsResetting => instance != null ? instance.isResetting : false;
    public static bool IsRewinding => instance != null ? instance.isRewinding : false;
    public static bool IsFastForwarding => instance != null ? instance.isFastForwarding : false;
    public static bool IsPlaying => instance != null ? !instance.isStopped && !instance.isResetting && !instance.isRewinding && !instance.isFastForwarding : false;

    public static void SetTargetTime(float targetTime)
    {
        if (instance == null) return;
        targetTime = Mathf.Clamp(targetTime, 0f, instance.totalDuration);
        if (targetTime < instance.elapsedTime)
        {
            instance.RewindToTime(targetTime);
        }
        else if (targetTime > instance.elapsedTime)
        {
            instance.FastForwardToTime(targetTime);
        }
    }

    public static void SetTimeScale(float newTimeScale)
    {
        if (instance == null) return;
        instance.timeScale = newTimeScale;
    }

    public static void SetTimeScaleMultiplier(float mult)
    {
        if (instance == null) return;
        instance.timeScaleMultiplier = mult;
    }

    public static TimeLoop GetInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<TimeLoop>();
        }
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Update()
    {
        if (IsFastForwarding)
        {
            elapsedTime = Mathf.MoveTowards(elapsedTime, targetTime, DeltaTime);
            if (elapsedTime == targetTime)
            {
                EndFastForward();
            }
        }
        else if (IsRewinding)
        {
            elapsedTime = Mathf.MoveTowards(elapsedTime, targetTime, -DeltaTime);
            if (elapsedTime == targetTime)
            {
                EndRewind();
            }
        }
        else
        {
            elapsedTime = Mathf.Clamp(elapsedTime + DeltaTime, 0f, totalDuration);
        }
        if (timeScale > 0f && elapsedTime == totalDuration && !isResetting)
        {
            ResetTimeLoop();
        }
        else if (timeScale < 0f && elapsedTime == 0f)
        {
            ResumeTimeLoop();
        }
        else if (timeScale == 0f && !isStopped)
        {
            StopTimeLoop();
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RewindToTime(0f);
        }
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            FastForwardToTime(totalDuration - 10f);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label($"Time: {CurrentTime:F2} / {TotalDuration:F2} (Scale: {TimeScale:F2})");
        GUILayout.HorizontalSlider(NormalizedTime, 0f, 1f, GUILayout.Width(400f));
        if (IsResetting)
        {
            GUILayout.Label("Resetting...");
        }
        else if (IsStopped)
        {
            GUILayout.Label("Stopped");
        }
        else if (IsRewinding)
        {
            GUILayout.Label("Rewinding...");
        }
        else if (IsFastForwarding)
        {
            GUILayout.Label("Fast Forwarding...");
        }
        else
        {
            GUILayout.Label("Playing");
        }
    }

    void StopTimeLoop()
    {
        if (isStopped) return;
        isStopped = true;
        timeScale = 0f;
        OnStopped.Invoke();
        StartCoroutine(DoStoppedTimeLoopEnding());
    }

    IEnumerator DoStoppedTimeLoopEnding()
    {
        AudioController.GetInstance().SetMasterVolumeMultiplier(0f);
        yield return new WaitForSeconds(5f);
        AudioController.GetInstance().SetMasterVolumeMultiplier(1f);
        Save.Instance.didTimeStopEnding = true;
        Save.SaveFile();
        SceneManager.LoadScene("Startup");
    }

    void ResetTimeLoop()
    {
        if (isResetting) return;
        isResetting = true;
        OnResetting.Invoke();
        StartCoroutine(DoResetTimeLoop());
    }

    IEnumerator DoResetTimeLoop()
    {
        yield return new WaitForSeconds(1f);
        isResetting = false;
        OnReset.Invoke();
        RewindToTime(0f);
    }

    void ResumeTimeLoop()
    {
        isStopped = false;
        isRewinding = false;
        isFastForwarding = false;
        timeScale = 1f;
        OnResume.Invoke();
    }

    void RewindToTime(float targetTime)
    {
        if (isRewinding) return;
        ResumeTimeLoop();
        this.targetTime = targetTime;
        isRewinding = true;
        OnRewinding.Invoke();
    }

    void EndRewind()
    {
        isRewinding = false;
        OnRewound.Invoke();
        ResumeTimeLoop();
    }

    void FastForwardToTime(float targetTime)
    {
        if (isFastForwarding) return;
        ResumeTimeLoop();
        this.targetTime = targetTime;
        isFastForwarding = true;
        OnFastForwarding.Invoke();
    }

    void EndFastForward()
    {
        isFastForwarding = false;
        OnFastForwarded.Invoke();
        ResumeTimeLoop();
    }
}
