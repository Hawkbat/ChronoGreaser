using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShipTravelController : MonoBehaviour
{
    [SerializeField] float travelDistance = 0.5f;

    [SerializeField] ShipController ship;
    [SerializeField] TravelLineController simLine;
    [SerializeField] GameObject linePrefab;
    [SerializeField] SoundController engineRumbleSound;

    List<TravelLineController> activeLines = new();
    Stack<TravelLineController> linePool = new();
    List<ShipController.Travel> travelsToReview = new();

    public Vector3 GetTravelStart() => ship.IsTraveling() ? ship.GetActiveTravel().start : ship.transform.localPosition;
    public Vector3 GetTravelEnd() => GetTravelStart() + transform.InverseTransformDirection(ship.transform.forward) * travelDistance;
    public float GetTravelDuration() => travelDistance / ship.GetTravelSpeed();

    void Awake()
    {
        activeLines = new();
        linePool = new();
        travelsToReview = new();
    }

    void Update()
    {
        if (ship.IsTraveling())
        {
            simLine.ClearTravel();
        }
        else
        {
            simLine.SetTravel(new ShipController.Travel
            {
                start = GetTravelStart(),
                startTime = TimeLoop.CurrentTime,
                end = GetTravelEnd(),
                endTime = TimeLoop.CurrentTime + GetTravelDuration(),
                bias = 0.5f,
                id = -1,
                source = null
            });
        }

        travelsToReview.Clear();
        travelsToReview.AddRange(ship.GetTravelHistory().Where(t => t.source == null));

        foreach (var activeLine in activeLines)
        {
            if (!travelsToReview.Any(t => t.id == activeLine.GetTravel().id))
            {
                activeLine.ClearTravel();
                linePool.Push(activeLine);
            }
        }
        foreach (var travel in travelsToReview)
        {
            if (travel.IsInitialized && !travel.IsFuture(TimeLoop.CurrentTime))
            {
                var line = activeLines.FirstOrDefault(l => l.GetTravel().id == travel.id);
                if (line == null)
                {
                    line = linePool.Count > 0 ? linePool.Pop() : Instantiate(linePrefab, transform).GetComponent<TravelLineController>();
                    activeLines.Add(line);
                }
                line.SetTravel(travel);
            }
        }

        engineRumbleSound.SetPlaying(ship.IsTraveling());
    }

    public void SetDistance(float newDistance)
    {
        travelDistance = newDistance;
    }

    public void InitiateTravel()
    {
        ship.TravelTo(GetTravelEnd(), ship.GetTravelSpeed(), 0.5f, null);
    }
}
