using UnityEngine;

public interface IHarvestable
{
    public Vector3 Position { get; }
    public float HarvestRadius { get; }
    public HarvestStatus HarvestStatus { get; }
    public CargoType CargoType { get; }
    public CargoType Harvest();
}
