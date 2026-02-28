using System.Collections.Generic;
using UnityEngine;

public class StarMapController : MonoBehaviour
{
    public float radius;
    public Vector3 pan;

    [SerializeField] Transform pivot;
    [SerializeField] ShipController ship;
    List<StarController> stars = new();

    public void RegisterStar(StarController star)
    {
        stars.Add(star);
    }

    public ShipController GetShip() => ship;
    public IEnumerable<StarController> GetStars() => stars;

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
