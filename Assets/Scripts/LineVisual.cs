using UnityEngine;

[ExecuteInEditMode]
public class LineVisual : TimedVisibilityVisual
{
    public Color color = Color.white;
    public float length = 1f;

    protected override void UpdateVisual(MaterialPropertyBlock propBlock)
    {
        base.UpdateVisual(propBlock);
        propBlock.SetColor("_Color", color);
        propBlock.SetFloat("_Length", length);
    }
}
