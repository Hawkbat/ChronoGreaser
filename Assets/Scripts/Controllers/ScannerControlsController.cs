using UnityEngine;

public class ScannerControlsController : MonoBehaviour
{
    [SerializeField] ShipScanController shipScanner;
    [SerializeField] ButtonControl scanButton;

    private void Update()
    {
        scanButton.Locked = !shipScanner.CanScan;
        if (scanButton.Pressed)
        {
            shipScanner.TryStartScan();
        }
    }
}
