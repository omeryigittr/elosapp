using System.Text.Json;
using ElosApp.Models;
using Microsoft.Maui.Storage;
using SQLite;

namespace ElosApp.Services;

public sealed class DatabaseService
{
    private const string MigrationKey = "elos.sqlite-migration-v1";
    private const string ProfileKey = "elos.baby-profile";
    private const string MedicalLogsKey = "elos.medical-logs";
    private const string MedicationRemindersKey = "elos.medication-reminders";
    private const string CareLogsKey = "elos.care-logs";
    private readonly SQLiteAsyncConnection _connection;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private bool _initialized;

    public DatabaseService()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "elos.db3");
        _connection = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
    }

    public async Task<BabyProfile?> GetBabyProfileAsync()
    {
        await EnsureInitializedAsync();
        return await _connection.Table<BabyProfile>().FirstOrDefaultAsync();
    }

    public async Task<int> SaveBabyProfileAsync(BabyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            if (profile.Id == 0)
                profile.Id = 1;
            return await _connection.InsertOrReplaceAsync(profile);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<IReadOnlyList<MedicalLog>> GetMedicalLogsAsync()
    {
        await EnsureInitializedAsync();
        return await _connection.Table<MedicalLog>().OrderByDescending(item => item.TarihSaat).ToListAsync();
    }

    public async Task<int> SaveMedicalLogAsync(MedicalLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        await EnsureInitializedAsync();
        return await SaveAsync(log);
    }

    public async Task<bool> DeleteMedicalLogAsync(int id)
    {
        await EnsureInitializedAsync();
        return await DeleteAsync<MedicalLog>(id);
    }

    public async Task<IReadOnlyList<MedicationReminder>> GetMedicationRemindersAsync()
    {
        await EnsureInitializedAsync();
        return await _connection.Table<MedicationReminder>().OrderBy(item => item.IsCompleted).ThenBy(item => item.NextDose).ToListAsync();
    }

    public async Task<int> SaveMedicationReminderAsync(MedicationReminder reminder)
    {
        ArgumentNullException.ThrowIfNull(reminder);
        await EnsureInitializedAsync();
        return await SaveAsync(reminder);
    }

    public async Task<bool> DeleteMedicationReminderAsync(int id)
    {
        await EnsureInitializedAsync();
        return await DeleteAsync<MedicationReminder>(id);
    }

    public async Task<IReadOnlyList<DailyCareLog>> GetCareLogsAsync()
    {
        await EnsureInitializedAsync();
        return await _connection.Table<DailyCareLog>().OrderByDescending(item => item.TarihSaat).ToListAsync();
    }

    public async Task<int> SaveCareLogAsync(DailyCareLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        await EnsureInitializedAsync();
        return await SaveAsync(log);
    }

    public async Task<bool> DeleteCareLogAsync(int id)
    {
        await EnsureInitializedAsync();
        return await DeleteAsync<DailyCareLog>(id);
    }

    private async Task<int> SaveAsync<T>(T item) where T : class, new()
    {
        await _writeLock.WaitAsync();
        try
        {
            return await _connection.InsertOrReplaceAsync(item);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task<bool> DeleteAsync<T>(int id) where T : class, new()
    {
        await _writeLock.WaitAsync();
        try
        {
            return await _connection.DeleteAsync<T>(id) > 0;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
                return;

            await _connection.CreateTableAsync<BabyProfile>();
            await _connection.CreateTableAsync<MedicalLog>();
            await _connection.CreateTableAsync<MedicationReminder>();
            await _connection.CreateTableAsync<DailyCareLog>();
            await _connection.CreateTableAsync<MilkStockEntry>();
            await MigrateLegacyJsonAsync();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<IReadOnlyList<MilkStockEntry>> GetMilkStocksAsync()
    {
        await EnsureInitializedAsync();
        return await _connection.Table<MilkStockEntry>().OrderBy(item => item.Tuketildi).ThenBy(item => item.SagimTarihi).ToListAsync();
    }

    public async Task<int> SaveMilkStockAsync(MilkStockEntry stock)
    {
        ArgumentNullException.ThrowIfNull(stock);
        await EnsureInitializedAsync();
        return await SaveAsync(stock);
    }

    public async Task<bool> DeleteMilkStockAsync(int id)
    {
        await EnsureInitializedAsync();
        return await DeleteAsync<MilkStockEntry>(id);
    }

    private async Task MigrateLegacyJsonAsync()
    {
        if (Preferences.Default.Get(MigrationKey, false))
            return;

        var profile = ReadLegacy<BabyProfile>(ProfileKey);
        if (profile is not null && await _connection.Table<BabyProfile>().CountAsync() == 0)
            await _connection.InsertOrReplaceAsync(profile);

        await MigrateListAsync<MedicalLog>(MedicalLogsKey);
        await MigrateListAsync<MedicationReminder>(MedicationRemindersKey);
        await MigrateListAsync<DailyCareLog>(CareLogsKey);
        Preferences.Default.Set(MigrationKey, true);
    }

    private async Task MigrateListAsync<T>(string key) where T : class, new()
    {
        var legacy = ReadLegacy<List<T>>(key);
        if (legacy is null || legacy.Count == 0 || await _connection.Table<T>().CountAsync() > 0)
            return;

        foreach (var item in legacy)
            await _connection.InsertOrReplaceAsync(item);
    }

    private T? ReadLegacy<T>(string key)
    {
        var json = Preferences.Default.Get(key, string.Empty);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }
}
