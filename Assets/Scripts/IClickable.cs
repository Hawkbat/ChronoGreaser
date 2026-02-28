using UnityEngine;

public interface IClickable
{
    void StartClicking();
    void OnClick(Vector3 worldTarget);
    void EndClicking();
}
