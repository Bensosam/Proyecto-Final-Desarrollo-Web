using System.Net.Http.Json;
using Clinica.Web.Models;

namespace Clinica.Web.Services;

public sealed class AuthService(
    IConfiguration configuration,
    ClinicAuthStateProvider authStateProvider)
{
    private readonly string _apiBaseUrl = EnsureTrailingSlash(configuration["ApiBaseUrl"] ?? "http://localhost:5002/");

    public async Task<(bool Success, string? Error)> LoginAsync(string usuario, string password)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(_apiBaseUrl) };
            var response = await client.PostAsJsonAsync("api/Auth/login", new LoginRequest
            {
                UsuarioLogin = usuario,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                var message = await ApiClient.ReadErrorMessageAsync(response);
                return (false, message ?? "Usuario o contraseña incorrectos.");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (string.IsNullOrWhiteSpace(result?.Token))
                return (false, "La API no devolvió un token válido.");

            await authStateProvider.SignInAsync(result.Token);
            return (true, null);
        }
        catch (HttpRequestException)
        {
            return (false, "No fue posible conectar con la API. Confirma que Clinica.API esté corriendo en http://localhost:5002.");
        }
        catch (Exception ex)
        {
            return (false, $"No se pudo iniciar sesión: {ex.Message}");
        }
    }

    public Task LogoutAsync() => authStateProvider.SignOutAsync();

    private static string EnsureTrailingSlash(string value) => value.EndsWith('/') ? value : value + "/";
}
