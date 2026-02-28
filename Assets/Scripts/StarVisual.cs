using UnityEngine;

[ExecuteInEditMode]
public class StarVisual : VisibilityVisual
{
    [ColorUsage(false, true)]
    public Color rimColor;
    [ColorUsage(false, true)]
    public Color innerColor;
    [Range(0, 0.5f)]
    public float rim;

    protected override void UpdateVisual(MaterialPropertyBlock propBlock)
    {
        base.UpdateVisual(propBlock);
        propBlock.SetColor("_RimColor", rimColor);
        propBlock.SetColor("_InnerColor", innerColor);
        propBlock.SetFloat("_Rim", rim);
    }
}
