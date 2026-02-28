using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public abstract class TimedVisibilityVisual : VisibilityVisual
{
    protected override void UpdateVisual(MaterialPropertyBlock propBlock)
    {
        base.UpdateVisual(propBlock);
        if (Application.isPlaying)
        {
            propBlock.SetFloat("_CurrentTime", TimeLoop.CurrentTime);
        }
        else
        {
#if UNITY_EDITOR
            propBlock.SetFloat("_CurrentTime", (float)EditorApplication.timeSinceStartup);
#endif
        }
    }
}
