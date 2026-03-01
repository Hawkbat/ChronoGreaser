using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class StarRemnantController : MonoBehaviour, IScannable, IHarvestable
{
    [SerializeField] protected float radius;
    [SerializeField] protected float duration;
    [SerializeField] protected AnimationCurve sizeCurve;
    [SerializeField] protected bool scanned;
    [SerializeField] protected float scanRadius;
    [SerializeField] protected string scanName;
    [SerializeField] protected string scanMessage;
    [SerializeField] protected CargoType cargoType;
    [SerializeField] protected List<CargoType> validShields = new();

    protected float scannedTime;
    protected StarController star;
    protected StarMapController starMap;

    public bool IsActive => TimeLoop.CurrentTime >= star.DeathTime && TimeLoop.CurrentTime < star.DeathTime + duration;
    public float Progress => Mathf.Clamp01(Mathf.InverseLerp(star.DeathTime, star.DeathTime + duration, TimeLoop.CurrentTime));
    public float Radius => radius * sizeCurve.Evaluate(Progress);

    public Vector3 Position => transform.position;
    public bool Scanned => scanned;
    public bool CanScan => IsActive && !scanned;
    public float ScanRadius => scanRadius;
    public string ScanName => scanName;
    public string ScanMessage => scanMessage;

    public float HarvestRadius => scanRadius;
    public HarvestStatus HarvestStatus =>
        !IsActive ? HarvestStatus.Missing :
        !scanned ? HarvestStatus.NotScanned :
        cargoType == CargoType.None ? HarvestStatus.Depleted :
        HarvestStatus.Ready;
    public CargoType CargoType => cargoType;

    public bool NeedsShield() => validShields.Count > 0f;
    public bool HasShields(IEnumerable<CargoType> shieldTypes) => validShields.Count == 0 || (shieldTypes.Any() && shieldTypes.Any(st => validShields.Contains(st)));

    protected virtual void Awake()
    {
        star = GetComponentInParent<StarController>();
        starMap = GetComponentInParent<StarMapController>();
    }

    public void Scan()
    {
        scanned = true;
        scannedTime = TimeLoop.CurrentTime;
    }

    public CargoType Harvest()
    {
        return cargoType;
    }

    protected virtual void Update()
    {
        if (scanned && TimeLoop.CurrentTime < scannedTime)
        {
            scanned = false;
        }
    }
}

public abstract class StarRemnantController<T> : StarRemnantController where T : VisibilityVisual
{
    [SerializeField] protected T visual;

    protected override void Update()
    {
        base.Update();
        visual.enabled = IsActive;
        visual.transform.localScale = Vector3.one * Radius;
        visual.visibilityPosition = starMap ? starMap.transform.position : transform.position;
        visual.visibilityRadius = starMap ? starMap.radius : 8f;
    }
}
