using UnityEngine;

public class EngineInjectorControlsController : MonoBehaviour
{
    [SerializeField] ButtonControl activateButton;
    [SerializeField] EngineInjectorController injector;

    void Update()
    {
        activateButton.Locked = !injector.CanInject;
        if (activateButton.Pressed)
        {
            injector.TryInject();
        }
    }
}
