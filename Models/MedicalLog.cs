using System.ComponentModel.DataAnnotations;

namespace ElosApp.Models;

public sealed class MedicalLog
{
    public int Id { get; set; }

    public DateTime TarihSaat { get; set; } = DateTime.Now;

    public double AtesDerecesi { get; set; }

    public double KiloKg { get; set; }

    [MaxLength(200)]
    public string Belirti { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Not { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Ilaclar { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Beslenme { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Uyku { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BezDurumu { get; set; } = string.Empty;

    public string? FotoğrafYolu { get; set; }
}
