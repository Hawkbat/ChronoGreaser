using UnityEngine;

public class StarMapControlsController : MonoBehaviour
{
    [SerializeField] ShipController ship;
    [SerializeField] ButtonControl brakeButton;
    [SerializeField] ButtonControl confirmButton;

    void Update()
    {
        brakeButton.Locked = !ship.IsTraveling();
        confirmButton.Locked = ship.IsTraveling();
    }
}
