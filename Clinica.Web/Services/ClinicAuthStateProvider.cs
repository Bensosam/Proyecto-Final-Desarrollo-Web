using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Clinica.Web.Services;

public sealed class ClinicAuthStateProvider(TokenStorageService storage) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await storage.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(Anonymous);

        try
        {
            var claims = ParseClaims(token);
            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            await storage.ClearTokenAsync();
            return new AuthenticationState(Anonymous);
        }
    }

    public async Task SignInAsync(string token)
    {
        await storage.SetTokenAsync(token);
        var claims = ParseClaims(token);
        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task SignOutAsync()
    {
        await storage.ClearTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
            throw new InvalidOperationException("Token JWT inválido.");

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
        var bytes = Convert.FromBase64String(payload);
        using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(bytes));

        var claims = new List<Claim>();
        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                    claims.Add(new Claim(NormalizeClaimType(property.Name), item.ToString()));
            }
            else
            {
                claims.Add(new Claim(NormalizeClaimType(property.Name), property.Value.ToString()));
            }
        }

        if (!claims.Any(c => c.Type == ClaimTypes.Name))
        {
            var name = claims.FirstOrDefault(c => c.Type is "unique_name" or "name" or "UsuarioLogin" or "usuario")?.Value;
            if (!string.IsNullOrWhiteSpace(name))
                claims.Add(new Claim(ClaimTypes.Name, name));
        }

        return claims;
    }

    private static string NormalizeClaimType(string type)
        => type switch
        {
            "role" or "roles" => ClaimTypes.Role,
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
            _ => type
        };
}
