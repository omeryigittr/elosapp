using System.ComponentModel.DataAnnotations;

namespace ElosApp.Models;

public sealed class MilkStockEntry
{
    public int Id { get; set; }
    public DateTime SagimTarihi { get; set; } = DateTime.Now;
    public double MiktarMl { get; set; }

    [MaxLength(30)]
    public string SaklamaYeri { get; set; } = "Buzdolabı";

    public DateTime SonKullanmaTarihi { get; set; } = DateTime.Today.AddDays(4);
    public bool Tuketildi { get; set; }
}
