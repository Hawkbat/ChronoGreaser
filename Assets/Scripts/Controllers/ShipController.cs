using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] ShipVisual shipVisual;
    [SerializeField] float travelSpeed = 5f;
    [SerializeField] float blackHoleSpeed = 2f;
    [SerializeField] float blackHoleBias = 0.8f;
    [SerializeField] AnimationCurve blackHoleTimeScaleCurve;
    [SerializeField] float supernovaSpeed = 0.1f;
    [SerializeField] float supernovaBias = 0.6f;
    [SerializeField] float supernovaKillDuration = 10f;

    StarMapController starMap;
    Stack<Travel> travelHistory;
    int nextTravelID;

    void Awake()
    {
        starMap = GetComponentInParent<StarMapController>();
        travelHistory = new();
        nextTravelID = 1;
    }

    void Update()
    {
        var visibilityPosition = starMap ? starMap.transform.position : transform.position;
        var visibilityRadius = starMap ? starMap.radius : 8f;
        shipVisual.visibilityPosition = visibilityPosition;
        shipVisual.visibilityRadius = visibilityRadius;

        if (travelHistory.Count > 0)
        {
            var currentTravel = travelHistory.Peek();
            while (currentTravel.IsFuture)
            {
                travelHistory.Pop();
                if (travelHistory.Count == 0) break;
                currentTravel = travelHistory.Peek();
            }
            transform.localPosition = currentTravel.GetPosition(TimeLoop.CurrentTime);
            if (currentTravel.IsActive)
            {
                transform.localRotation = Quaternion.LookRotation((currentTravel.end - currentTravel.start).normalized, Vector3.up);
            }
            if (currentTravel.IsActive && currentTravel.source != null && currentTravel.source.IsBlackHole)
            {
                TimeLoop.SetTimeScaleMultiplier(blackHoleTimeScaleCurve.Evaluate(currentTravel.GetProgress(TimeLoop.CurrentTime)));
            }
            else
            {
                TimeLoop.SetTimeScaleMultiplier(1f);
            }
        }
        else
        {
            TimeLoop.SetTimeScaleMultiplier(1f);
        }

        foreach (var star in starMap.GetStars())
        {
            if (star.IsBlackHole || star.IsSupernova)
            {
                float distanceToDangerousStar = Vector3.Distance(transform.localPosition, star.transform.localPosition);
                if (distanceToDangerousStar < star.RemnantRadius)
                {
                    if (IsTravelingTo(star))
                    {
                        break;
                    }
                    if (star.IsBlackHole)
                    {
                        TravelTo(star.transform.localPosition, blackHoleSpeed, blackHoleBias, star);
                    }
                    else if (star.IsSupernova)
                    {
                        TravelTo(star.transform.localPosition, supernovaSpeed, supernovaBias, star);
                        FadeController.Instance.StartFade(Color.white, supernovaKillDuration, fadeIn: false);
                    }
                    break;
                }
            }
        }
    }

    public float GetTravelSpeed() => travelSpeed;

    public bool IsTraveling() => travelHistory.Count > 0 && travelHistory.Peek().IsActive;

    public Travel GetActiveTravel() => IsTraveling() ? travelHistory.Peek() : default;

    public IEnumerable<Travel> GetTravelHistory() => travelHistory;

    public void SetRotation(Vector3 angles)
    {
        transform.localRotation = Quaternion.Euler(angles);
    }

    public bool InterruptTravel(bool overrideNaturalTravel)
    {
        if (travelHistory.Count == 0 || TimeLoop.CurrentTime >= travelHistory.Peek().endTime)
        {
            // Nothing to interrupt
            return true;
        }
        else if (travelHistory.Count > 0 && travelHistory.Peek().source != null && !overrideNaturalTravel)
        {
            // Can't interrupt natural travel (e.g. being pulled into a black hole)
            return false;
        }
        else
        {
            var lastTravel = travelHistory.Pop();
            lastTravel.end = lastTravel.GetPosition(TimeLoop.CurrentTime);
            lastTravel.endTime = TimeLoop.CurrentTime;
            travelHistory.Push(lastTravel);
            transform.localPosition = lastTravel.GetPosition(TimeLoop.CurrentTime);
            return true;
        }
    }

    public bool TravelTo(Vector3 destination, float speed, float bias, StarController source)
    {
        var canOverrideNaturalTravel = source != null;
        if (!InterruptTravel(canOverrideNaturalTravel)) return false;
        float distance = Vector3.Distance(transform.localPosition, destination);
        float duration = distance / speed;
        var travel = new Travel(
            id: nextTravelID++,
            startTime: TimeLoop.CurrentTime,
            endTime: TimeLoop.CurrentTime + duration,
            bias: bias,
            start: transform.localPosition,
            end: destination,
            source: source
        );
        travelHistory.Push(travel);
        return true;
    }

    public bool IsTravelingTo(StarController star)
    {
        return IsTraveling() && travelHistory.Peek().source == star;
    }

    public struct Travel
    {
        public int id;
        public float startTime;
        public float endTime;
        public float bias;
        public Vector3 start;
        public Vector3 end;
        public StarController source;

        public readonly bool IsInitialized => id != 0;

        public Travel(int id, float startTime, float endTime, float bias, Vector3 start, Vector3 end, StarController source)
        {
            this.id = id;
            this.startTime = startTime;
            this.endTime = endTime;
            this.bias = bias;
            this.start = start;
            this.end = end;
            this.source = source;
        }

        public readonly bool IsActive => TimeLoop.CurrentTime >= startTime && TimeLoop.CurrentTime <= endTime;
        public readonly bool IsPast => TimeLoop.CurrentTime > endTime;
        public readonly bool IsFuture => TimeLoop.CurrentTime < startTime;

        public readonly float GetProgress(float time)
        {
            if (time <= startTime) return 0f;
            if (time >= endTime) return 1f;
            float t = Mathf.Clamp01(Mathf.InverseLerp(startTime, endTime, time));
            return Bias(t, bias);
        }

        public readonly Vector3 GetPosition(float time)
        {
            return Vector3.Lerp(start, end, GetProgress(time));
        }

        static float Bias(float t, float bias) => t / ((1f / bias - 2f) * (1f - t) + 1f);
    }
}
