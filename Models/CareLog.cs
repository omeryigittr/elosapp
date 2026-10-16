using System.ComponentModel.DataAnnotations;

namespace ElosApp.Models;

public sealed class DailyCareLog
{
    public int Id { get; set; }
    public DateTime TarihSaat { get; set; } = DateTime.Now;

    [MaxLength(40)]
    public string BakimTuru { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Durum { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Detay { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Not { get; set; } = string.Empty;

    public string? FotoğrafYolu { get; set; }
}
