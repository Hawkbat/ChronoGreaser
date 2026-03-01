using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ShipCargoController : MonoBehaviour
{
    [SerializeField] List<CargoType> cargo = new();
    [SerializeField] float collectionDuration;
    [SerializeField] TextMeshProUGUI cargoText;
    [SerializeField] StarMapController starMap;
    [SerializeField] ShipController ship;

    Stack<CargoCollection> cargoCollections = new();
    IHarvestable nearestHarvestable;
    IHarvestable currentHarvestable;
    bool isCollecting;
    float collectionStartTime;

    public bool IsCollecting => isCollecting;
    public bool CanCollect => nearestHarvestable != null && nearestHarvestable.CanHarvest && !IsCollecting && !ship.IsTraveling();
    public float CollectionProgress => IsCollecting ? Mathf.Clamp01((TimeLoop.CurrentTime - collectionStartTime) / collectionDuration) : 0f;

    public IEnumerable<CargoType> GetCargo() => cargo;

    void Update()
    {
        while (cargoCollections.Count > 0 && cargoCollections.Peek().time > TimeLoop.CurrentTime)
        {
            var collection = cargoCollections.Pop();
            cargo.Remove(collection.cargoType);
        }

        if (isCollecting && TimeLoop.CurrentTime < collectionStartTime)
        {
            currentHarvestable = null;
            isCollecting = false;
        }

        if (isCollecting && ship.IsTraveling())
        {
            currentHarvestable = null;
            isCollecting = false;
        }

        if (isCollecting && CollectionProgress >= 1f)
        {
            var cargoType = currentHarvestable.Harvest();
            cargo.Add(cargoType);
            cargoCollections.Push(new CargoCollection
            {
                time = TimeLoop.CurrentTime,
                cargoType = cargoType
            });
            currentHarvestable = null;
            isCollecting = false;
        }

        var harvestables = starMap.GetHarvestables();
        nearestHarvestable = harvestables.OrderBy(h => Vector3.Distance(h.Position, ship.transform.position)).FirstOrDefault();
        if (nearestHarvestable != null)
        {
            var dist = Vector3.Distance(nearestHarvestable.Position, ship.transform.position);
            if (dist > nearestHarvestable.HarvestRadius)
            {
                nearestHarvestable = null;
            }
        }

        var collectionTargetText = "Not Found";
        if (ship.IsTraveling())
        {
            collectionTargetText = "Cannot Collect While In Motion";
        }
        else if (isCollecting)
        {
            collectionTargetText = $"{currentHarvestable.CargoType.GetDisplayName()} ({Mathf.RoundToInt(CollectionProgress * 100)}%)";
        }
        else if (nearestHarvestable != null)
        {
            collectionTargetText = nearestHarvestable.CanHarvest ? nearestHarvestable.CargoType.GetDisplayName() : "Unidentified";
        }

        var cargoListText = "None";
        if (cargo.Count > 0)
        {
            cargoListText = string.Join("\n", cargo.Select(c => c.GetDisplayName()));
        }

        cargoText.text = $"Collection Target:\n{collectionTargetText}\n\nCurrent Cargo:\n{cargoListText}";
    }

    public bool TryStartCollecting()
    {
        if (!CanCollect) return false;
        currentHarvestable = nearestHarvestable;
        isCollecting = true;
        collectionStartTime = TimeLoop.CurrentTime;
        return true;
    }

    struct CargoCollection
    {
        public float time;
        public CargoType cargoType;
    }
}
