using UnityEngine;

public class ScanBeaconController : MonoBehaviour
{
    [SerializeField] bool scanned;
    [SerializeField] bool harvested;
    [SerializeField] float scanRadius;
    [SerializeField] string scanName;
    [SerializeField] string scanMessage;
    [SerializeField] CargoType cargoType;
    [SerializeField] ShipVisual beaconVisual;
    [SerializeField] Color unscannedColor;
    [SerializeField] Color scannedColor;

    float scannedTime;
    float harvestedTime;

    public bool Scanned => scanned;
    public bool CanScan => !scanned;
    public bool Harvested => harvested;
    public bool CanHarvest => cargoType != CargoType.None && scanned && !harvested;
    public float ScanRadius => scanRadius;
    public string ScanName => scanName;
    public string ScanMessage => scanMessage;
    public CargoType CargoType => cargoType;

    public void Scan()
    {
        scanned = true;
        scannedTime = TimeLoop.CurrentTime;
    }

    public CargoType Harvest()
    {
        harvested = true;
        harvestedTime = TimeLoop.CurrentTime;
        return cargoType;
    }

    void Update()
    {
        if (scanned && TimeLoop.CurrentTime < scannedTime)
        {
            scanned = false;
        }
        if (harvested && TimeLoop.CurrentTime < harvestedTime)
        {
            harvested = false;
        }
        beaconVisual.color = scanned ? scannedColor : unscannedColor;
        beaconVisual.enabled = !harvested;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = scannedColor;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
