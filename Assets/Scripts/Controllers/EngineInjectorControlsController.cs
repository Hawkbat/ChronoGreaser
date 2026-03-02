using UnityEngine;

public class EngineInjectorControlsController : MonoBehaviour
{
    [SerializeField] ButtonControl activateButton;
    [SerializeField] ButtonControl shieldModeButton;
    [SerializeField] ButtonControl engineModeButton;
    [SerializeField] EngineInjectorController injector;

    void Update()
    {
        activateButton.Locked = !injector.CanInject;
        if (activateButton.Pressed)
        {
            injector.TryInject();
        }
        shieldModeButton.Locked = injector.Mode == InjectorMode.Shield;
        if (shieldModeButton.Pressed)
        {
            injector.Mode = InjectorMode.Shield;
        }
        engineModeButton.Locked = injector.Mode == InjectorMode.Engine;
        if (engineModeButton.Pressed)
        {
            injector.Mode = InjectorMode.Engine;
        }
    }
}
