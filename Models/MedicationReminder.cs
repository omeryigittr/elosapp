using System.ComponentModel.DataAnnotations;

namespace ElosApp.Models;

public sealed class MedicationReminder
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Dose { get; set; } = string.Empty;

    public DateTime NextDose { get; set; } = DateTime.Now;

    public bool IsVaccine { get; set; }

    public bool IsCompleted { get; set; }
}
