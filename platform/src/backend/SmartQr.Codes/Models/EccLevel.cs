namespace SmartQr.Codes.Models;

/// <summary>QR error-correction level. Higher = more redundancy = tolerates a center logo / damage, at the cost of density.</summary>
public enum EccLevel
{
    /// <summary>~7% recovery.</summary>
    L,

    /// <summary>~15% recovery.</summary>
    M,

    /// <summary>~25% recovery — default; safe for a center logo.</summary>
    Q,

    /// <summary>~30% recovery — use with large logos.</summary>
    H,
}
