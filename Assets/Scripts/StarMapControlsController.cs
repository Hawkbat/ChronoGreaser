using UnityEngine;

public class StarMapControlsController : MonoBehaviour
{
    [SerializeField] StarMapController starMap;
    [SerializeField] ShipController ship;
    [SerializeField] ShipTravelController shipTravel;
    [SerializeField] TrackballControl mapRotationTrackball;
    [SerializeField] ButtonControl brakeButton;
    [SerializeField] ButtonControl confirmButton;
    [SerializeField] SliderControl distanceSlider;
    [SerializeField] SliderControl shipXSlider;
    [SerializeField] SliderControl shipYSlider;

    void Update()
    {
        var mapRotationRaw = mapRotationTrackball.Value;
        var mapRotationAngles = new Vector3(mapRotationRaw.y, mapRotationRaw.x, 0f);
        starMap.SetRotation(mapRotationAngles);
        brakeButton.Locked = !ship.IsTraveling();
        if (brakeButton.Pressed)
        {
            ship.InterruptTravel(false);
        }
        confirmButton.Locked = ship.IsTraveling();
        if (confirmButton.Pressed)
        {
            shipTravel.InitiateTravel();
        }
        distanceSlider.Locked = ship.IsTraveling();
        shipTravel.SetDistance(distanceSlider.Value);
        shipXSlider.Locked = ship.IsTraveling();
        shipYSlider.Locked = ship.IsTraveling();
        if (!ship.IsTraveling())
        {
            var shipRotationAngles = new Vector3(shipYSlider.Value * 180f, shipXSlider.Value * 180f, 0f);
            ship.SetRotation(shipRotationAngles);
        }
    }
}
