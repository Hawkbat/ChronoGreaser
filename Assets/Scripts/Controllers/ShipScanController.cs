using System.Linq;
using TMPro;
using UnityEngine;

public class ShipScanController : MonoBehaviour
{
    [SerializeField] float scanDuration;
    [SerializeField] TextMeshProUGUI scanText;
    [SerializeField] StarMapController starMap;
    [SerializeField] ShipController ship;

    IScannable nearestScannable;
    IScannable currentScannable;
    bool isScanning;
    float scanStartTime;

    public bool IsScanning => isScanning;
    public bool CanScan => nearestScannable != null && nearestScannable.CanScan && !IsScanning;
    public float ScanProgress => IsScanning ? Mathf.Clamp01((TimeLoop.CurrentTime - scanStartTime) / scanDuration) : 0f;

    void Update()
    {
        if (isScanning && TimeLoop.CurrentTime < scanStartTime)
        {
            currentScannable = null;
            isScanning = false;
        }

        if (isScanning && ScanProgress >= 1f)
        {
            currentScannable.Scan();
            currentScannable = null;
            isScanning = false;
        }

        var scannables = starMap.GetScannables();
        nearestScannable = scannables.OrderBy(s => Vector3.Distance(s.Position, ship.transform.position)).FirstOrDefault();
        if (nearestScannable != null)
        {
            var dist = Vector3.Distance(nearestScannable.Position, ship.transform.position);
            if (dist > nearestScannable.ScanRadius)
            {
                nearestScannable = null;
            }
        }

        var scanName = nearestScannable != null ? (nearestScannable.Scanned ? nearestScannable.ScanName : "Unidentified") : "No Target";
        var scanMessage = nearestScannable != null && nearestScannable.Scanned ? nearestScannable.ScanMessage : "";
        scanText.text = $"Scan Target:\n{scanName}\n{scanMessage}{(IsScanning ? $"\n\nScan Progress:\n{Mathf.RoundToInt(ScanProgress * 100)}%" : "")}";
    }

    public bool TryStartScan()
    {
        if (!CanScan) return false;
        currentScannable = nearestScannable;
        isScanning = true;
        scanStartTime = TimeLoop.CurrentTime;
        return true;
    }
}