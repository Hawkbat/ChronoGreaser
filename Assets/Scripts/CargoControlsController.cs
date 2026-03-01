using UnityEngine;

public class CargoControlsController : MonoBehaviour
{
    [SerializeField] ShipCargoController shipCargo;
    [SerializeField] ButtonControl collectButton;

    void Update()
    {
        collectButton.Locked = !shipCargo.CanCollect;
        if (collectButton.Pressed)
        {
            shipCargo.TryStartCollecting();
        }
    }
}
