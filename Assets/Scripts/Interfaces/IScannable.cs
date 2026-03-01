using UnityEngine;

public interface IScannable
{
    public Vector3 Position { get; }
    public bool Scanned { get; }
    public bool CanScan { get; }
    public float ScanRadius { get; }
    public string ScanName { get; }
    public string ScanMessage { get; }
    public void Scan();
}
