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

    public async Task UpdateRacerAsync(int id, UpdateRacerRequest request) =>
        await PutEmptyAsync($"api/racers/{id}", request);

    public async Task<List<RacetrackDto>> GetRacetracksAsync() =>
        await GetAsync<List<RacetrackDto>>("api/racetracks") ?? new();

    public async Task<RacetrackDto?> CreateRacetrackAsync(CreateRacetrackRequest request) =>
        await PostAsync<RacetrackDto>("api/racetracks", request);

    public async Task<List<StableDto>> GetStablesAsync() =>
        await GetAsync<List<StableDto>>("api/stables") ?? new();

    public async Task<StableDto?> GetMyStableAsync() =>
        await GetAsync<StableDto>("api/stables/mine");

    public async Task<StableDto?> CreateStableAsync(CreateStableRequest request) =>
        await PostAsync<StableDto>("api/stables", request);

    public async Task<List<AvailabilityDto>> GetAvailabilitiesAsync(int? trackId = null, bool? available = null)
    {
        var url = "api/availabilities";
        var query = new List<string>();
        if (trackId.HasValue) query.Add($"trackId={trackId.Value}");
        if (available.HasValue) query.Add($"available={available.Value.ToString().ToLower()}");
        if (query.Count > 0) url += "?" + string.Join("&", query);
        return await GetAsync<List<AvailabilityDto>>(url) ?? new();
    }

    public async Task SetAvailabilityAsync(SetAvailabilityRequest request) =>
        await PutEmptyAsync("api/availabilities", request);

    public async Task<List<InjuryDto>> GetInjuriesAsync(int? racerId = null) =>
        await GetAsync<List<InjuryDto>>(racerId.HasValue ? $"api/injuries?racerId={racerId.Value}" : "api/injuries") ?? new();

    public async Task<InjuryDto?> CreateInjuryAsync(CreateInjuryRequest request) =>
        await PostAsync<InjuryDto>("api/injuries", request);

    public async Task SetInjuryStatusAsync(int id, InjuryStatus status) =>
        await PutEmptyAsync($"api/injuries/{id}/status?status={status}", new { });

    public async Task<TreatmentDto?> CreateTreatmentAsync(CreateTreatmentRequest request) =>
        await PostAsync<TreatmentDto>("api/treatments", request);

    public async Task<List<UserDto>> GetUsersAsync() =>
        await GetAsync<List<UserDto>>("api/users") ?? new();

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request) =>
        await PostAsync<UserDto>("api/users", request);

    public async Task SetUserRoleAsync(string id, string role) =>
        await PutEmptyAsync($"api/users/{id}/role", new SetRoleRequest { Role = role });

    public async Task<List<TeamDto>> GetTeamsAsync() =>
        await GetAsync<List<TeamDto>>("api/teams") ?? new();

    public async Task<TeamDto?> CreateTeamAsync(CreateTeamRequest request) =>
        await PostAsync<TeamDto>("api/teams", request);

    public async Task AddTeamMemberAsync(int teamId, AddTeamMemberRequest request) =>
        await PostEmptyAsync($"api/teams/{teamId}/members", request);

    public async Task RemoveTeamMemberAsync(int teamId, int personId) =>
        await DeleteAsync($"api/teams/{teamId}/members/{personId}");

    public async Task<List<RecordShareDto>> GetSharesAsync(string entityType, Guid entityKey) =>
        await GetAsync<List<RecordShareDto>>($"api/shares?entityType={entityType}&entityKey={entityKey}") ?? new();

    public async Task<RecordShareDto?> GrantShareAsync(CreateShareRequest request) =>
        await PostAsync<RecordShareDto>("api/shares", request);

    public async Task RevokeShareAsync(int shareId) =>
        await DeleteAsync($"api/shares/{shareId}");

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

    private async Task PutEmptyAsync(string url, object body)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync(url, body);
        await EnsureSuccessAsync(response);
    }

    private async Task PostEmptyAsync(string url, object body)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync(url, body);
        await EnsureSuccessAsync(response);
    }

    private async Task DeleteAsync(string url)
    {
        await SetAuthHeaderAsync();
        var response = await _http.DeleteAsync(url);
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
