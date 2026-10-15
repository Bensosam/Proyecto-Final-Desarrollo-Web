using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Clinica.Web.Models;

namespace Clinica.Web.Services;

public sealed class ApiClient(IConfiguration configuration, TokenStorageService storage)
{
    private readonly string _apiBaseUrl = EnsureTrailingSlash(configuration["ApiBaseUrl"] ?? "http://localhost:5002/");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<JsonElement>> GetCollectionAsync(string endpoint)
    {
        var response = await SendAsync(HttpMethod.Get, endpoint);
        if (!response.IsSuccess || response.Data is null)
            return [];

        var root = response.Data.Value;
        if (root.ValueKind == JsonValueKind.Array)
            return root.EnumerateArray().Select(x => x.Clone()).ToList();

        if (root.ValueKind == JsonValueKind.Object && TryGetProperty(root, "value", out var value) && value.ValueKind == JsonValueKind.Array)
            return value.EnumerateArray().Select(x => x.Clone()).ToList();

        return [];
    }

    public Task<ApiResponse> PostAsync(string endpoint, object payload)
        => SendAsync(HttpMethod.Post, endpoint, payload);

    public Task<ApiResponse> PutAsync(string endpoint, object payload)
        => SendAsync(HttpMethod.Put, endpoint, payload);

    public Task<ApiResponse> DeleteAsync(string endpoint)
        => SendAsync(HttpMethod.Delete, endpoint);

    public async Task<ApiResponse> GetAsync(string endpoint)
        => await SendAsync(HttpMethod.Get, endpoint);

    public async Task<ApiResponse> SendAsync(HttpMethod method, string endpoint, object? payload = null)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(_apiBaseUrl) };
            var token = await storage.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var request = new HttpRequestMessage(method, endpoint.TrimStart('/'));
            if (payload is not null)
            {
                var json = JsonSerializer.Serialize(payload, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await client.SendAsync(request);
            var text = response.Content is null ? string.Empty : await response.Content.ReadAsStringAsync();
            JsonElement? data = null;

            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    data = doc.RootElement.Clone();
                }
                catch
                {
                    // Algunas respuestas exitosas (204) o errores pueden no ser JSON.
                }
            }

            return new ApiResponse
            {
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Data = data,
                Message = response.IsSuccessStatusCode ? null : ExtractMessage(data, text, response.StatusCode)
            };
        }
        catch (HttpRequestException)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                StatusCode = 0,
                Message = "No fue posible conectar con la API. Verifica que Clinica.API esté ejecutándose en http://localhost:5002."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                StatusCode = 0,
                Message = ex.Message
            };
        }
    }

    public static bool TryGetProperty(JsonElement element, string key, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    public static string GetDisplayValue(JsonElement element, string key)
    {
        if (!TryGetProperty(element, key, out var value))
            return "—";

        return value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => "—",
            JsonValueKind.True => "Sí",
            JsonValueKind.False => "No",
            JsonValueKind.String => value.GetString() ?? "—",
            _ => value.ToString()
        };
    }

    public static decimal GetDecimal(JsonElement element, string key)
    {
        if (!TryGetProperty(element, key, out var value))
            return 0m;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
            return number;

        return decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m;
    }

    public static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(text))
            return response.StatusCode == HttpStatusCode.Unauthorized ? "Sesión no autorizada." : null;

        try
        {
            using var doc = JsonDocument.Parse(text);
            return ExtractMessage(doc.RootElement, text, response.StatusCode);
        }
        catch
        {
            return text;
        }
    }

    private static string ExtractMessage(JsonElement? data, string raw, HttpStatusCode statusCode)
    {
        if (data is { } element && element.ValueKind == JsonValueKind.Object)
        {
            if (TryGetProperty(element, "mensaje", out var mensaje))
                return mensaje.ToString();
            if (TryGetProperty(element, "message", out var message))
                return message.ToString();
            if (TryGetProperty(element, "title", out var title))
                return title.ToString();
        }

        if (statusCode == HttpStatusCode.Unauthorized)
            return "Tu sesión expiró o no es válida. Vuelve a iniciar sesión.";
        if (statusCode == HttpStatusCode.Forbidden)
            return "No tienes permisos para realizar esta operación.";
        if (statusCode == HttpStatusCode.NotFound)
            return "El recurso solicitado no fue encontrado.";

        return string.IsNullOrWhiteSpace(raw) ? $"La API respondió con {(int)statusCode}." : raw;
    }

    private static string EnsureTrailingSlash(string value) => value.EndsWith('/') ? value : value + "/";
}
