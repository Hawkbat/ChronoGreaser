using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ShipCargoController : MonoBehaviour
{
    [SerializeField] CargoType[] cargo = new CargoType[8];
    [SerializeField] float collectionDuration;
    [SerializeField] TextMeshProUGUI cargoText;
    [SerializeField] TextMeshProUGUI[] cargoSlotTexts;
    [SerializeField] StarMapController starMap;
    [SerializeField] ShipController ship;

    Stack<CargoCollection> cargoCollections = new();
    IHarvestable nearestHarvestable;
    IHarvestable currentHarvestable;
    bool isCollecting;
    float collectionStartTime;

    public bool IsCollecting => isCollecting;
    public bool CanCollect => nearestHarvestable != null && nearestHarvestable.HarvestStatus == HarvestStatus.Ready && !IsCollecting && !ship.IsTraveling();
    public float CollectionProgress => IsCollecting ? Mathf.Clamp01((TimeLoop.CurrentTime - collectionStartTime) / collectionDuration) : 0f;

    public IEnumerable<CargoType> GetCargo() => cargo;
    public CargoType GetCargoAt(int index) => cargo[index];
    public int CargoCount => cargo.Length;
    public bool IsEmpty => cargo.All(c => c == CargoType.None);

    void Update()
    {
        while (cargoCollections.Count > 0 && cargoCollections.Peek().time > TimeLoop.CurrentTime)
        {
            var collection = cargoCollections.Pop();
            cargo[collection.index] = collection.prev;
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
            var emptyIndex = System.Array.FindIndex(cargo, c => c == CargoType.None);
            SetCargoAt(emptyIndex, cargoType);
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
            collectionTargetText = nearestHarvestable.HarvestStatus switch
            {
                HarvestStatus.NotScanned => "Not Scanned",
                HarvestStatus.Ready => nearestHarvestable.CargoType.GetDisplayName(),
                _ => "Not Found"
            };
        }

        cargoText.text = $"Collection Target:\n{collectionTargetText}\n\nCurrent Cargo:{(IsEmpty ? "\nEmpty" : "")}";

        for (int i = 0; i < cargoSlotTexts.Length; i++)
        {
            cargoSlotTexts[i].text = cargo[i] != CargoType.None ? cargo[i].GetDisplayName() : string.Empty;
        }
    }

    public bool TryStartCollecting()
    {
        if (!CanCollect) return false;
        currentHarvestable = nearestHarvestable;
        isCollecting = true;
        collectionStartTime = TimeLoop.CurrentTime;
        return true;
    }

    public void SetCargoAt(int index, CargoType cargoType)
    {
        var prevType = cargo[index];
        cargo[index] = cargoType;
        cargoCollections.Push(new CargoCollection
        {
            time = TimeLoop.CurrentTime,
            prev = prevType,
            next = cargoType,
            index = index
        });
    }

    public CargoType RemoveCargoAt(int index)
    {
        var cargoType = cargo[index];
        SetCargoAt(index, CargoType.None);
        return cargoType;
    }

    struct CargoCollection
    {
        public float time;
        public CargoType prev;
        public CargoType next;
        public int index;
    }
}
