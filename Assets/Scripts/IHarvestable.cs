using UnityEngine;

public interface IHarvestable
{
    public Vector3 Position { get; }
    public float HarvestRadius { get; }
    public bool CanHarvest { get; }
    public CargoType CargoType { get; }
    public CargoType Harvest();
}
