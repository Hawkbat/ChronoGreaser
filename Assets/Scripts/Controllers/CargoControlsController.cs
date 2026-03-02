using UnityEngine;

public class CargoControlsController : MonoBehaviour
{
    [SerializeField] ShipCargoController shipCargo;
    [SerializeField] EngineInjectorController injector;
    [SerializeField] ButtonControl collectButton;
    [SerializeField] ButtonControl[] cargoInjectButtons;

    void Update()
    {
        collectButton.Locked = !shipCargo.CanCollect;
        if (collectButton.Pressed)
        {
            shipCargo.TryStartCollecting();
        }
        for (int i = 0; i < cargoInjectButtons.Length; i++)
        {
            cargoInjectButtons[i].Locked = shipCargo.GetCargoAt(i) == CargoType.None;
            if (cargoInjectButtons[i].Pressed)
            {
                if (injector.AddCargo(shipCargo.GetCargoAt(i)))
                {
                    shipCargo.RemoveCargoAt(i);
                }
            }
        }
    }
}
