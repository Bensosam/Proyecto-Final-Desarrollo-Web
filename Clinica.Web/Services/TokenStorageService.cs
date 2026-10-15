using Microsoft.JSInterop;

namespace Clinica.Web.Services;

public sealed class TokenStorageService(IJSRuntime js)
{
    private const string TokenKey = "clinica.jwt";

    public async ValueTask<string?> GetTokenAsync()
        => await js.InvokeAsync<string?>("sessionStorage.getItem", TokenKey);

    public async ValueTask SetTokenAsync(string token)
        => await js.InvokeVoidAsync("sessionStorage.setItem", TokenKey, token);

    public async ValueTask ClearTokenAsync()
        => await js.InvokeVoidAsync("sessionStorage.removeItem", TokenKey);
}
