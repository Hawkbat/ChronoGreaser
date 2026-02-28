using UnityEngine;

public class StarController : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] bool radiusKnown;
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

    StarMapController starMap;
    SphereCollider col;

    public bool IsAlive => TimeLoop.CurrentTime < deathTime;
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
    public float NebulaRadius => hasNebula ? radius * nebulaSizeCurve.Evaluate(SupernovaProgress) : 0f;
    public float BlackHoleRadius => hasBlackHole ? radius * blackHoleSizeCurve.Evaluate(SupernovaProgress) : 0f;

    public float DangerRadius => Mathf.Max(SupernovaRadius, NebulaRadius, BlackHoleRadius);

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
        starVisual.enabled = IsAlive;
        starVisual.rimColor = RimColor;
        starVisual.innerColor = InnerColor;
        starVisual.transform.localScale = Radius * Vector3.one;

        supernovaVisual.enabled = IsSupernova;
        supernovaVisual.transform.localScale = SupernovaRadius * Vector3.one;

        nebulaVisual.enabled = IsNebula;
        nebulaVisual.transform.localScale = NebulaRadius * Vector3.one;

        blackHoleVisual.enabled = IsBlackHole;
        blackHoleVisual.transform.localScale = BlackHoleRadius * Vector3.one;

        radiusVisual.enabled = radiusKnown && DangerRadius > 0f;
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
