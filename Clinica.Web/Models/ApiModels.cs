using System.Net;
using System.Text.Json;

namespace Clinica.Web.Models;

public sealed class ApiResponse
{
    public bool IsSuccess { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public string? Message { get; init; }
    public JsonElement? Data { get; init; }
}

public sealed class LoginRequest
{
    public string UsuarioLogin { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}

public sealed class DashboardMetric
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Helper { get; set; } = string.Empty;
    public string Icon { get; set; } = "activity";
    public string Tone { get; set; } = "blue";
}
