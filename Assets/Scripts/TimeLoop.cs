using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TimeLoop : MonoBehaviour
{
    static TimeLoop instance;

    [SerializeField] float totalDuration = 120f;
    [SerializeField] float timeScale = 1f;
    [SerializeField] float timeScaleMultiplier = 1f;
    [SerializeField] float rewindSpeed = 20f;
    [SerializeField] float fastForwardSpeed = 20f;

    float elapsedTime = 0f;
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
    public static float TimeScale => instance != null ? instance.timeScale * instance.timeScaleMultiplier : 1f;
    public static float TotalDuration => instance != null ? instance.totalDuration : 0f;
    public static float NormalizedTime => instance != null ? Mathf.Clamp01(instance.elapsedTime / instance.totalDuration) : 0f;
    public static bool IsStopped => instance != null ? instance.isStopped : false;
    public static bool IsResetting => instance != null ? instance.isResetting : false;
    public static bool IsRewinding => instance != null ? instance.isRewinding : false;
    public static bool IsFastForwarding => instance != null ? instance.isFastForwarding : false;

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
        elapsedTime = Mathf.Clamp(elapsedTime + Time.deltaTime * timeScale * timeScaleMultiplier, 0f, totalDuration);
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
        yield return new WaitForSeconds(5f);
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
        elapsedTime = 0f;
        isResetting = false;
        OnReset.Invoke();
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
        isRewinding = true;
        OnRewinding.Invoke();
        StartCoroutine(DoRewindToTime(targetTime));
    }

    IEnumerator DoRewindToTime(float targetTime)
    {
        float startTime = elapsedTime;
        float duration = Mathf.Abs(targetTime - startTime) / rewindSpeed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            elapsedTime = Mathf.Lerp(startTime, targetTime, elapsed / duration);
            yield return null;
        }
        elapsedTime = targetTime;
        isRewinding = false;
        OnRewound.Invoke();
        ResumeTimeLoop();
    }

    void FastForwardToTime(float targetTime)
    {
        if (isFastForwarding) return;
        isFastForwarding = true;
        OnFastForwarding.Invoke();
        StartCoroutine(DoFastForwardToTime(targetTime));
    }

    IEnumerator DoFastForwardToTime(float targetTime)
    {
        float startTime = elapsedTime;
        float duration = Mathf.Abs(targetTime - startTime) / fastForwardSpeed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            elapsedTime = Mathf.Lerp(startTime, targetTime, elapsed / duration);
            yield return null;
        }
        elapsedTime = targetTime;
        isFastForwarding = false;
        OnFastForwarded.Invoke();
        ResumeTimeLoop();
    }
}
