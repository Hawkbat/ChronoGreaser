using UnityEngine;

[ExecuteInEditMode]
public class RadiusVisual : TimedVisibilityVisual
{
    [ColorUsage(false, true)]
    public Color color;

    protected override void UpdateVisual(MaterialPropertyBlock propBlock)
    {
        base.UpdateVisual(propBlock);
        propBlock.SetColor("_Color", color);
    }
}
