using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StarController : MonoBehaviour, IScannable, IHarvestable
{
    [SerializeField] float radius;
    [SerializeField] bool scanned;
    [SerializeField] float scanRadius;
    [SerializeField] string scanName;
    [SerializeField] string scanMessage;
    [SerializeField] CargoType cargoType;
    [SerializeField] Gradient lifeRimColorCurve;
    [SerializeField] Gradient lifeInnerColorCurve;
    [SerializeField] AnimationCurve lifeSizeCurve;
    [SerializeField] float startDecayTime;
    [SerializeField] float deathTime;
    [SerializeField] Gradient decayRimColorCurve;
    [SerializeField] Gradient decayInnerColorCurve;
    [SerializeField] AnimationCurve decaySizeCurve;

    [SerializeField] StarVisual starVisual;
    [SerializeField] RadiusVisual radiusVisual;

    float scannedTime;
    StarMapController starMap;
    StarRemnantController remnant;
    SphereCollider col;

    public Vector3 Position => transform.position;
    public bool Scanned => scanned;
    public bool CanScan => (IsAlive || IsDecaying) && !scanned;
    public float ScanRadius => scanRadius;
    public string ScanName => scanName;
    public string ScanMessage => scanMessage;
    public bool CanHarvest => (IsAlive || IsDecaying) && cargoType != CargoType.None && scanned;
    public float HarvestRadius => scanRadius;
    public CargoType CargoType => cargoType;

    public bool IsAlive => TimeLoop.CurrentTime < startDecayTime;
    public bool IsDecaying => TimeLoop.CurrentTime >= startDecayTime && TimeLoop.CurrentTime < deathTime;
    public bool IsDead => TimeLoop.CurrentTime >= deathTime;
    public float DecayStartTime => startDecayTime;
    public float DeathTime => deathTime;
    public float LifeProgress => Mathf.Clamp01(Mathf.InverseLerp(0f, startDecayTime, TimeLoop.CurrentTime));
    public Color RimColor => IsAlive ? lifeRimColorCurve.Evaluate(LifeProgress) : decayRimColorCurve.Evaluate(DecayProgress);
    public Color InnerColor => IsAlive ? lifeInnerColorCurve.Evaluate(LifeProgress) : decayInnerColorCurve.Evaluate(DecayProgress);
    public float Radius => radius * (IsAlive ? lifeSizeCurve.Evaluate(LifeProgress) : decaySizeCurve.Evaluate(DecayProgress));
    public float DecayProgress => Mathf.Clamp01(Mathf.InverseLerp(startDecayTime, deathTime, TimeLoop.CurrentTime));

    public StarRemnantController Remnant => remnant;
    public float RemnantRadius => remnant != null ? remnant.Radius : 0f;
    public float RemnantProgress => remnant != null ? remnant.Progress : 0f;
    public bool IsNebula => remnant is NebulaRemnantController;
    public bool IsBlackHole => remnant is BlackHoleRemnantController;
    public bool IsSupernova => remnant is SupernovaRemnantController;

    public void Scan()
    {
        scanned = true;
        scannedTime = TimeLoop.CurrentTime;
    }

    public CargoType Harvest()
    {
        return cargoType;
    }

    void Awake()
    {
        starMap = GetComponentInParent<StarMapController>();
        col = GetComponent<SphereCollider>();
        remnant = GetComponentInChildren<StarRemnantController>();

        if (starMap)
        {
            starMap.RegisterStar(this);
        }
    }

    void Update()
    {
        if (scanned && TimeLoop.CurrentTime < scannedTime)
        {
            scanned = false;
        }

        starVisual.enabled = IsAlive || IsDecaying;
        starVisual.rimColor = RimColor;
        starVisual.innerColor = InnerColor;
        starVisual.transform.localScale = Radius * Vector3.one;

        radiusVisual.enabled = scanned;
        radiusVisual.transform.localScale = Mathf.Max(Radius, RemnantRadius) * Vector3.one;

        col.radius = Mathf.Max(Radius, RemnantRadius);

        var visibilityPosition = starMap ? starMap.transform.position : transform.position;
        var visibilityRadius = starMap ? starMap.radius : 8f;

        starVisual.visibilityPosition = visibilityPosition;
        starVisual.visibilityRadius = visibilityRadius;
        radiusVisual.visibilityPosition = visibilityPosition;
        radiusVisual.visibilityRadius = visibilityRadius;
    }
}
