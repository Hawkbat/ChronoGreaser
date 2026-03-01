using UnityEngine;

public enum CargoType
{
    None,
    IonizedHydrogenPlasma,
    Helium3,
    RadioactiveIsotopes,
    HeavyMetals,
    HydrogenGas,
    NeutroniumDust,
    RareEarthMetals,
    SuperconductingAlloys,
    TachyonResonantGammaRays,
}

public static class CargoTypeExtensions
{
    public static string GetDisplayName(this CargoType cargoType) => cargoType switch
    {
        CargoType.None => "None",
        CargoType.IonizedHydrogenPlasma => "Ionized Hydrogen Plasma",
        CargoType.Helium3 => "Helium-3",
        CargoType.RadioactiveIsotopes => "Radioactive Isotopes",
        CargoType.HeavyMetals => "Heavy Metals",
        CargoType.HydrogenGas => "Hydrogen Gas",
        CargoType.NeutroniumDust => "Neutronium Dust",
        CargoType.RareEarthMetals => "Rare Earth Metals",
        CargoType.SuperconductingAlloys => "Superconducting Alloys",
        CargoType.TachyonResonantGammaRays => "Tachyon Resonant Gamma Rays",
        _ => cargoType.ToString(),
    };
}
