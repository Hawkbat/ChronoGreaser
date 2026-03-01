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
    [SerializeField] List<CargoType> validShields = new();
    [SerializeField] Gradient lifeRimColorCurve;
    [SerializeField] Gradient lifeInnerColorCurve;
    [SerializeField] AnimationCurve lifeSizeCurve;
    [SerializeField] float startDecayTime;
    [SerializeField] float deathTime;
    [SerializeField] Gradient decayRimColorCurve;
    [SerializeField] Gradient decayInnerColorCurve;
    [SerializeField] AnimationCurve decaySizeCurve;
    [SerializeField] bool hasSupernova;
    [SerializeField] float supernovaDuration;
    [SerializeField] Gradient supernovaColorCurve;
    [SerializeField] AnimationCurve supernovaSizeCurve;
    [SerializeField] bool hasNebula;
    [SerializeField] float nebulaDuration;
    [SerializeField] Gradient nebulaColorCurve;
    [SerializeField] AnimationCurve nebulaSizeCurve;
    [SerializeField] bool hasBlackHole;
    [SerializeField] float blackHoleDuration;
    [SerializeField] Gradient blackHoleColorCurve;
    [SerializeField] AnimationCurve blackHoleSizeCurve;

    [SerializeField] StarVisual starVisual;
    [SerializeField] RadiusVisual radiusVisual;
    [SerializeField] SupernovaVisual supernovaVisual;
    [SerializeField] NebulaVisual nebulaVisual;
    [SerializeField] BlackHoleVisual blackHoleVisual;

    float scannedTime;
    StarMapController starMap;
    SphereCollider col;

    public Vector3 Position => transform.position;
    public bool Scanned => scanned;
    public bool CanScan => !scanned;
    public float ScanRadius => scanRadius;
    public string ScanName => scanName;
    public string ScanMessage => scanMessage;
    public bool CanHarvest => cargoType != CargoType.None && scanned;
    public float HarvestRadius => scanRadius;
    public CargoType CargoType => cargoType;

    public bool IsAlive => TimeLoop.CurrentTime < startDecayTime;
    public bool IsDecaying => TimeLoop.CurrentTime >= startDecayTime && TimeLoop.CurrentTime < deathTime;
    public bool IsDead => TimeLoop.CurrentTime >= deathTime;
    public float LifeProgress => Mathf.Clamp01(Mathf.InverseLerp(0f, startDecayTime, TimeLoop.CurrentTime));
    public Color RimColor => IsAlive ? lifeRimColorCurve.Evaluate(LifeProgress) : decayRimColorCurve.Evaluate(DecayProgress);
    public Color InnerColor => IsAlive ? lifeInnerColorCurve.Evaluate(LifeProgress) : decayInnerColorCurve.Evaluate(DecayProgress);
    public float Radius => radius * (IsAlive ? lifeSizeCurve.Evaluate(LifeProgress) : decaySizeCurve.Evaluate(DecayProgress));
    public float DecayProgress => Mathf.Clamp01(Mathf.InverseLerp(startDecayTime, deathTime, TimeLoop.CurrentTime));
    public bool IsSupernova => hasSupernova && TimeLoop.CurrentTime >= deathTime && TimeLoop.CurrentTime < deathTime + supernovaDuration;
    public bool IsNebula => hasNebula && TimeLoop.CurrentTime >= deathTime && TimeLoop.CurrentTime < deathTime + nebulaDuration;
    public bool IsBlackHole => hasBlackHole && TimeLoop.CurrentTime >= deathTime && TimeLoop.CurrentTime < deathTime + blackHoleDuration;
    public float SupernovaProgress => hasSupernova ? Mathf.Clamp01((TimeLoop.CurrentTime - deathTime) / supernovaDuration) : 0f;
    public float NebulaProgress => hasNebula ? Mathf.Clamp01((TimeLoop.CurrentTime - deathTime) / nebulaDuration) : 0f;
    public float BlackHoleProgress => hasBlackHole ? Mathf.Clamp01((TimeLoop.CurrentTime - deathTime) / blackHoleDuration) : 0f;
    public float SupernovaRadius => hasSupernova ? radius * supernovaSizeCurve.Evaluate(SupernovaProgress) : 0f;
    public float NebulaRadius => hasNebula ? radius * nebulaSizeCurve.Evaluate(NebulaProgress) : 0f;
    public float BlackHoleRadius => hasBlackHole ? radius * blackHoleSizeCurve.Evaluate(BlackHoleProgress) : 0f;

    public float DangerRadius => Mathf.Max(SupernovaRadius, NebulaRadius, BlackHoleRadius);

    public void Scan()
    {
        scanned = true;
        scannedTime = TimeLoop.CurrentTime;
    }

    public CargoType Harvest()
    {
        return cargoType;
    }

    public bool NeedsShield() => validShields.Count > 0f;
    public bool HasShields(IEnumerable<CargoType> shieldTypes) => validShields.Count == 0 || (shieldTypes.Any() && shieldTypes.Any(st => validShields.Contains(st)));

    void Awake()
    {
        starMap = GetComponentInParent<StarMapController>();
        col = GetComponent<SphereCollider>();

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

        supernovaVisual.enabled = IsSupernova;
        supernovaVisual.transform.localScale = SupernovaRadius * Vector3.one;

        nebulaVisual.enabled = IsNebula;
        nebulaVisual.transform.localScale = NebulaRadius * Vector3.one;

        blackHoleVisual.enabled = IsBlackHole;
        blackHoleVisual.transform.localScale = BlackHoleRadius * Vector3.one;

        radiusVisual.enabled = scanned && DangerRadius > 0f;
        radiusVisual.transform.localScale = DangerRadius * Vector3.one;

        col.radius = Radius;

        var visibilityPosition = starMap ? starMap.transform.position : transform.position;
        var visibilityRadius = starMap ? starMap.radius : 8f;

        starVisual.visibilityPosition = visibilityPosition;
        starVisual.visibilityRadius = visibilityRadius;
        radiusVisual.visibilityPosition = visibilityPosition;
        radiusVisual.visibilityRadius = visibilityRadius;
        supernovaVisual.visibilityPosition = visibilityPosition;
        supernovaVisual.visibilityRadius = visibilityRadius;
        nebulaVisual.visibilityPosition = visibilityPosition;
        nebulaVisual.visibilityRadius = visibilityRadius;
        blackHoleVisual.visibilityPosition = visibilityPosition;
        blackHoleVisual.visibilityRadius = visibilityRadius;
    }
}
