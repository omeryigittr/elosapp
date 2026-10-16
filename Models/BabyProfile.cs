using System.ComponentModel.DataAnnotations;

namespace ElosApp.Models;

public sealed class BabyProfile
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Required]
    public string Ad { get; set; } = string.Empty;

    public DateTime DogumTarihi { get; set; } = DateTime.Today;

    [MaxLength(20)]
    public string KanGrubu { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Alerjiler { get; set; } = string.Empty;

    [MaxLength(30)]
    [Required]
    public string AcilDurumNumarasi { get; set; } = string.Empty;
}
