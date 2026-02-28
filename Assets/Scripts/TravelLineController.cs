using UnityEngine;

public class TravelLineController : MonoBehaviour
{
    [SerializeField] Color activeColor;
    [SerializeField] Color pastColor;

    [SerializeField] CursorVisual startVisual;
    [SerializeField] CursorVisual endVisual;
    [SerializeField] LineVisual lineVisual;

    ShipController.Travel travel;

    public void SetTravel(ShipController.Travel newTravel)
    {
        travel = newTravel;
    }

    public ShipController.Travel GetTravel() => travel;

    public void ClearTravel()
    {
        travel = new();
    }

    private void Awake()
    {
        travel = new();
    }

    void Update()
    {
        if (travel.IsInitialized && !travel.IsFuture(TimeLoop.CurrentTime))
        {
            startVisual.enabled = true;
            endVisual.enabled = true;
            lineVisual.enabled = true;

            startVisual.color = TimeLoop.CurrentTime > travel.startTime ? pastColor : activeColor;
            endVisual.color = TimeLoop.CurrentTime < travel.endTime ? activeColor : pastColor;
            lineVisual.color = travel.IsActive(TimeLoop.CurrentTime) ? activeColor : pastColor;

            startVisual.transform.localPosition = travel.start;
            endVisual.transform.localPosition = travel.end;
            lineVisual.transform.localPosition = travel.start;
            lineVisual.transform.localRotation = Quaternion.LookRotation((travel.end - travel.start).normalized, Vector3.up);
            lineVisual.length = Vector3.Distance(travel.start, travel.end);
        }
        else
        {
            startVisual.enabled = false;
            endVisual.enabled = false;
            lineVisual.enabled = false;
        }
    }
}
