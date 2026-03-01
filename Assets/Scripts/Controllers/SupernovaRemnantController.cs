using UnityEngine;

public class SupernovaRemnantController : StarRemnantController<SupernovaVisual>
{
    [GradientUsage(true)]
    [SerializeField] Gradient rimColorCurve;
    [GradientUsage(true)]
    [SerializeField] Gradient innerColorCurve;

    protected override void Update()
    {
        base.Update();
        visual.rimColor = rimColorCurve.Evaluate(Progress);
        visual.innerColor = innerColorCurve.Evaluate(Progress);
    }
}
