using UnityEngine;

[ExecuteInEditMode]
public abstract class VisibilityVisual : Visual
{
    public Vector3 visibilityPosition;
    public float visibilityRadius;

    protected override void UpdateVisual(MaterialPropertyBlock propBlock)
    {
        propBlock.SetVector("_VisibilityPosition", visibilityPosition);
        propBlock.SetFloat("_VisibilityRadius", visibilityRadius);
    }
}
