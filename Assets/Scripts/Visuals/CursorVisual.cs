using UnityEngine;

[ExecuteInEditMode]
public class CursorVisual : VisibilityVisual
{
    public Color color = Color.white;

    protected override void UpdateVisual(MaterialPropertyBlock propBlock)
    {
        base.UpdateVisual(propBlock);
        propBlock.SetColor("_Color", color);
    }
}
