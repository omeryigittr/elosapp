using Microsoft.Maui.Storage;

namespace ElosApp.Services;

public sealed class LocalizationService
{
    private readonly Dictionary<string, (string Tr, string En)> _texts = new()
    {
        ["home"] = ("Ana Sayfa", "Home"),
        ["panic"] = ("Acil Durum", "Emergency"),
        ["medications"] = ("İlaç Takibi", "Medication Tracker"),
        ["care"] = ("Bakım", "Care"),
        ["medical"] = ("Tıbbi Günlük", "Medical Log"),
        ["settings"] = ("Ayarlar", "Settings"),
        ["offline"] = ("Çevrimdışı", "Offline"),
        ["heroKicker"] = ("Ebeveynler için akıllı güvenlik", "Smart safety for parents"),
        ["heroTitle"] = ("En değerliniz", "Your most precious"),
        ["heroTitleAccent"] = ("güvende.", "one is safe."),
        ["heroSubtitle"] = ("E.L.O.S. ile bebeğinizin güvenliği her an elinizde. Tamamen çevrimdışı, tamamen size özel.", "With E.L.O.S., your baby's safety is always in your hands. Fully offline, fully private."),
        ["emergency"] = ("Acil Durum", "Emergency"),
        ["cloudless"] = ("Bulut yok", "No cloud"),
        ["private"] = ("%100 özel", "100% private"),
        ["ready"] = ("Her zaman hazır", "Always ready"),
        ["ecosystem"] = ("E.L.O.S. EKOSİSTEMİ", "E.L.O.S. ECOSYSTEM"),
        ["everything"] = ("İhtiyacınız olan her şey,", "Everything you need,"),
        ["onePlace"] = ("tek bir yerde.", "in one place."),
        ["deviceOnly"] = ("Verileriniz sadece bu cihazda", "Your data stays on this device"),
        ["language"] = ("Dil", "Language")
        ,["settingsTitle"] = ("Ayarlar ve Profil", "Settings and Profile")
        ,["savedOnDevice"] = ("Bilgileriniz yalnızca bu cihazda saklanır.", "Your information stays only on this device.")
        ,["website"] = ("Resmi web sitesi", "Official website")
        ,["support"] = ("E.L.O.S.'u destekle", "Support E.L.O.S.")
        ,["supportText"] = ("Uygulamanın geliştirilmesine katkıda bulunmak için kahve ısmarlayabilirsiniz.", "You can support the development with a coffee.")
        ,["coffee"] = ("Kahve ısmarla", "Buy me a coffee")
        ,["backupReady"] = ("Şifreli .elos yedeği hazırlandı.", "Encrypted .elos backup is ready.")
        ,["imported"] = ("Yedek içe aktarıldı.", "Backup imported.")
    };

    public event Action? Changed;
    public bool IsEnglish { get; private set; } = Preferences.Default.Get("elos.language", "tr") == "en";
    public string CurrentCode => IsEnglish ? "EN" : "TR";

    public string T(string key)
    {
        if (!_texts.TryGetValue(key, out var text))
            return key;

        return IsEnglish ? text.En : text.Tr;
    }

    public void ToggleLanguage()
    {
        IsEnglish = !IsEnglish;
        Preferences.Default.Set("elos.language", IsEnglish ? "en" : "tr");
        Changed?.Invoke();
    }
}
