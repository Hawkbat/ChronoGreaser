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

    public ShipController GetShip() => ship;
    public IEnumerable<StarController> GetStars() => stars;

    public void RegisterStar(StarController star)
    {
        stars.Add(star);
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
