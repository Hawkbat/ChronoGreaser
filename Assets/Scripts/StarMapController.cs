using System.Collections.Generic;
using UnityEngine;

public class StarMapController : MonoBehaviour
{
    public float radius;
    public Vector3 pan;
    public Vector3 rotate;

    [SerializeField] Transform pivot;
    [SerializeField] ShipController ship;

    List<StarController> stars = new();
    List<ScanBeaconController> scanBeacons = new();

    public ShipController GetShip() => ship;
    public IEnumerable<StarController> GetStars() => stars;
    public IEnumerable<ScanBeaconController> GetScanBeacons() => scanBeacons;
    public IEnumerable<IScannable> GetScannables()
    {
        foreach (var star in stars)
            yield return star;
        foreach (var scanBeacon in scanBeacons)
            yield return scanBeacon;
    }
    public IEnumerable<IHarvestable> GetHarvestables()
    {
        foreach (var star in stars)
            yield return star;
        foreach (var scanBeacon in scanBeacons)
            yield return scanBeacon;
    }

    public void RegisterStar(StarController star)
    {
        stars.Add(star);
    }

    public void RegisterScanBeacon(ScanBeaconController scanBeacon)
    {
        scanBeacons.Add(scanBeacon);
    }

    public void SetRotation(Vector3 angles)
    {
        pivot.localRotation = Quaternion.Euler(angles);
    }

    void Update()
    {
        pivot.localPosition = -pan;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
