using UnityEngine;

public class ScanBeaconController : MonoBehaviour, IScannable, IHarvestable
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

    StarMapController starMap;
    float scannedTime;
    float harvestedTime;

    public Vector3 Position => transform.position;
    public bool Scanned => scanned;
    public bool CanScan => !scanned;
    public float ScanRadius => scanRadius;
    public string ScanName => scanName;
    public string ScanMessage => scanMessage;
    public bool Harvested => harvested;
    public HarvestStatus HarvestStatus =>
        !scanned ? HarvestStatus.NotScanned :
        harvested ? HarvestStatus.Depleted :
        HarvestStatus.Ready;
    public float HarvestRadius => scanRadius;
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

    void Awake()
    {
        starMap = GetComponentInParent<StarMapController>();
         if (starMap != null)
        {
            starMap.RegisterScanBeacon(this);
        }
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

        var visibilityPosition = starMap ? starMap.transform.position : transform.position;
        var visibilityRadius = starMap ? starMap.radius : 8f;
        beaconVisual.visibilityPosition = visibilityPosition;
        beaconVisual.visibilityRadius = visibilityRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = scannedColor;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
