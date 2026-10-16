using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using ElosApp.Models;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace ElosApp.Services;

public sealed class BackupService
{
    private static readonly byte[] Magic = "ELOS1"u8.ToArray();
    private readonly DatabaseService _database;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public BackupService(DatabaseService database) => _database = database;

    public async Task<string> ExportAsync(string password)
    {
        ValidatePassword(password);
        var snapshot = new BackupSnapshot
        {
            Profile = await _database.GetBabyProfileAsync(),
            MedicalLogs = (await _database.GetMedicalLogsAsync()).ToList(),
            MedicationReminders = (await _database.GetMedicationRemindersAsync()).ToList(),
            CareLogs = (await _database.GetCareLogsAsync()).ToList(),
            MilkStocks = (await _database.GetMilkStocksAsync()).ToList()
        };

        await using var plain = new MemoryStream();
        using (var archive = new ZipArchive(plain, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("data.json", CompressionLevel.Optimal);
            await using var stream = entry.Open();
            await JsonSerializer.SerializeAsync(stream, snapshot, _jsonOptions);
        }

        var encrypted = Encrypt(plain.ToArray(), password);
        var backupDirectory = Path.Combine(FileSystem.AppDataDirectory, "backups");
        Directory.CreateDirectory(backupDirectory);
        var path = Path.Combine(backupDirectory, $"elos-backup-{DateTime.Now:yyyyMMdd-HHmm}.elos");
        await File.WriteAllBytesAsync(path, encrypted);
        return path;
    }

    public async Task ShareAsync(string path)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "E.L.O.S. yedek dosyası",
            File = new ShareFile(path)
        });
    }

    public async Task ImportAsync(string path, string password)
    {
        ValidatePassword(password);
        var snapshot = Decrypt(await File.ReadAllBytesAsync(path), password);
        using var archive = new ZipArchive(new MemoryStream(snapshot), ZipArchiveMode.Read);
        var entry = archive.GetEntry("data.json") ?? throw new InvalidDataException("Geçersiz .elos dosyası.");
        await using var stream = entry.Open();
        var data = await JsonSerializer.DeserializeAsync<BackupSnapshot>(stream, _jsonOptions)
            ?? throw new InvalidDataException("Yedek içeriği okunamadı.");

        if (data.Profile is not null)
            await _database.SaveBabyProfileAsync(data.Profile);
        foreach (var item in data.MedicalLogs)
            await _database.SaveMedicalLogAsync(item);
        foreach (var item in data.MedicationReminders)
            await _database.SaveMedicationReminderAsync(item);
        foreach (var item in data.CareLogs)
            await _database.SaveCareLogAsync(item);
        foreach (var item in data.MilkStocks)
            await _database.SaveMilkStockAsync(item);
    }

    private static byte[] Encrypt(byte[] data, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 150_000, HashAlgorithmName.SHA256, 32);
        var cipher = new byte[data.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, data, cipher, tag);
        return Magic.Concat(salt).Concat(nonce).Concat(tag).Concat(cipher).ToArray();
    }

    private static byte[] Decrypt(byte[] payload, string password)
    {
        if (payload.Length < 49 || !payload.AsSpan(0, 5).SequenceEqual(Magic))
            throw new InvalidDataException("Geçersiz veya bozuk .elos dosyası.");

        var salt = payload[5..21];
        var nonce = payload[21..33];
        var tag = payload[33..49];
        var cipher = payload[49..];
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 150_000, HashAlgorithmName.SHA256, 32);
        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(nonce, cipher, tag, plain);
        return plain;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Yedek şifresi en az 6 karakter olmalıdır.", nameof(password));
    }

    private sealed class BackupSnapshot
    {
        public BabyProfile? Profile { get; set; }
        public List<MedicalLog> MedicalLogs { get; set; } = new();
        public List<MedicationReminder> MedicationReminders { get; set; } = new();
        public List<DailyCareLog> CareLogs { get; set; } = new();
        public List<MilkStockEntry> MilkStocks { get; set; } = new();
    }
}
