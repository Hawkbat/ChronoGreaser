using UnityEngine;

public enum CargoType
{
    None,
    IonizedHydrogenPlasma,
    Helium3,
    Uranium,
    Thorium,
    Gold,
    Platinum,
    Hydrogen,
    Methane,
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
        CargoType.Uranium => "Uranium",
        CargoType.Thorium => "Thorium",
        CargoType.Gold => "Gold",
        CargoType.Platinum => "Platinum",
        CargoType.Hydrogen => "Hydrogen",
        CargoType.Methane => "Methane",
        CargoType.NeutroniumDust => "Neutronium Dust",
        CargoType.RareEarthMetals => "Rare Earth Metals",
        CargoType.SuperconductingAlloys => "Superconducting Alloys",
        CargoType.TachyonResonantGammaRays => "Tachyon Resonant Gamma Rays",
        _ => cargoType.ToString(),
    };
}
