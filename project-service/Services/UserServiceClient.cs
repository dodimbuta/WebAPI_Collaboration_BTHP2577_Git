using System.Net;
using System.Net.Http.Json;
using user_service.Models.DTOs;

namespace project_service.Services;

/// <summary>HTTP client for the user microservice.</summary>
public sealed class UserServiceClient(HttpClient httpClient)
{
    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/user/user/{userId}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken);
    }
}
