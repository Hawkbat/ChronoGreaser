using UnityEngine;

[ExecuteInEditMode]
public abstract class Visual : MonoBehaviour
{
    public bool billboard = true;

    protected Renderer r;
    protected MaterialPropertyBlock propBlock;

    protected virtual void OnEnable()
    {
        if (r != null) r.enabled = true;
    }

    protected virtual void OnDisable()
    {
        if (r != null) r.enabled = false;
    }

    protected virtual void LateUpdate()
    {
        if (r == null)
        {
            r = GetComponent<Renderer>();
            r.enabled = enabled;
        }
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }
        UpdateVisual(propBlock);
        r.SetPropertyBlock(propBlock);
        if (Application.isPlaying && billboard)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
        }
    }

    protected abstract void UpdateVisual(MaterialPropertyBlock propBlock);
}
