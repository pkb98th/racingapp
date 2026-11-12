using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;

namespace RacingApp.Client.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly LocalStorageService _storage;

    public ApiClient(HttpClient http, LocalStorageService storage)
    {
        _http = http;
        _storage = storage;
    }

    public async Task<List<RaceMeetingDto>> GetMeetingsAsync() =>
        await GetAsync<List<RaceMeetingDto>>("api/meetings") ?? new();

    public async Task<RaceMeetingDto?> GetMeetingAsync(int id) =>
        await GetAsync<RaceMeetingDto>($"api/meetings/{id}");

    public async Task<RaceMeetingDto?> CreateMeetingAsync(CreateMeetingRequest request) =>
        await PostAsync<RaceMeetingDto>("api/meetings", request);

    public async Task<RaceDto?> CreateRaceAsync(CreateRaceRequest request) =>
        await PostAsync<RaceDto>("api/races", request);

    public async Task<EntryDto?> CreateEntryAsync(CreateEntryRequest request) =>
        await PostAsync<EntryDto>("api/entries", request);

    public async Task SetResultAsync(int entryId, SetResultRequest request) =>
        await PutEmptyAsync($"api/entries/{entryId}/result", request);

    public async Task ScratchEntryAsync(int entryId) =>
        await PutEmptyAsync($"api/entries/{entryId}/scratch", new { });

    public async Task<List<RacerDto>> GetRacersAsync(string? search = null) =>
        await GetAsync<List<RacerDto>>(string.IsNullOrWhiteSpace(search) ? "api/racers" : $"api/racers?search={Uri.EscapeDataString(search)}") ?? new();

    public async Task<List<RacerDto>> GetMyRacersAsync() =>
        await GetAsync<List<RacerDto>>("api/racers/mine") ?? new();

    public async Task<RacerDetailDto?> GetRacerAsync(int id) =>
        await GetAsync<RacerDetailDto>($"api/racers/{id}");

    public async Task<RacerDto?> CreateRacerAsync(CreateRacerRequest request) =>
        await PostAsync<RacerDto>("api/racers", request);

    public async Task<List<RacetrackDto>> GetRacetracksAsync() =>
        await GetAsync<List<RacetrackDto>>("api/racetracks") ?? new();

    public async Task<RacetrackDto?> CreateRacetrackAsync(CreateRacetrackRequest request) =>
        await PostAsync<RacetrackDto>("api/racetracks", request);

    public async Task<List<InjuryDto>> GetInjuriesAsync(int? racerId = null) =>
        await GetAsync<List<InjuryDto>>(racerId.HasValue ? $"api/injuries?racerId={racerId.Value}" : "api/injuries") ?? new();

    public async Task<InjuryDto?> CreateInjuryAsync(CreateInjuryRequest request) =>
        await PostAsync<InjuryDto>("api/injuries", request);

    public async Task SetInjuryStatusAsync(int id, InjuryStatus status) =>
        await PutEmptyAsync($"api/injuries/{id}/status?status={status}", new { });

    public async Task<TreatmentDto?> CreateTreatmentAsync(CreateTreatmentRequest request) =>
        await PostAsync<TreatmentDto>("api/treatments", request);

    public async Task<List<OwnerDto>> GetOwnersAsync() =>
        await GetAsync<List<OwnerDto>>("api/owners") ?? new();

    public async Task<OwnerDto?> CreateOwnerAsync(CreateOwnerRequest request) =>
        await PostAsync<OwnerDto>("api/owners", request);

    public async Task<List<TrainerDto>> GetTrainersAsync() =>
        await GetAsync<List<TrainerDto>>("api/trainers") ?? new();

    public async Task<TrainerDto?> CreateTrainerAsync(CreateTrainerRequest request) =>
        await PostAsync<TrainerDto>("api/trainers", request);

    public async Task<List<VetDto>> GetVetsAsync() =>
        await GetAsync<List<VetDto>>("api/vets") ?? new();

    public async Task<VetDto?> CreateVetAsync(CreateVetRequest request) =>
        await PostAsync<VetDto>("api/vets", request);

    public async Task<List<HandlerDto>> GetHandlersAsync() =>
        await GetAsync<List<HandlerDto>>("api/handlers") ?? new();

    public async Task<HandlerDto?> CreateHandlerAsync(CreateHandlerRequest request) =>
        await PostAsync<HandlerDto>("api/handlers", request);

    public async Task<List<StableDto>> GetStablesAsync() =>
        await GetAsync<List<StableDto>>("api/stables") ?? new();

    public async Task<StableDto?> CreateStableAsync(CreateStableRequest request) =>
        await PostAsync<StableDto>("api/stables", request);

    private async Task SetAuthHeaderAsync()
    {
        var token = await _storage.GetAsync(CustomAuthStateProvider.TokenKey);
        _http.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token) ? null : new("Bearer", token);
    }

    private async Task<T?> GetAsync<T>(string url)
    {
        await SetAuthHeaderAsync();
        var response = await _http.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T?> PostAsync<T>(string url, object body)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync(url, body);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task PostEmptyAsync(string url, object body)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync(url, body);
        await EnsureSuccessAsync(response);
    }

    private async Task PutEmptyAsync(string url, object body)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync(url, body);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string message;
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            message = doc.RootElement.TryGetProperty("message", out var m)
                ? m.GetString() ?? $"Request failed ({(int)response.StatusCode})."
                : $"Request failed ({(int)response.StatusCode}).";
        }
        catch
        {
            message = $"Request failed ({(int)response.StatusCode}).";
        }

        throw new ApiError(message);
    }
}
