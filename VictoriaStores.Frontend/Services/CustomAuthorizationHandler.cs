using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace VictoriaStores.Frontend.Services;

public class CustomAuthorizationHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private const string TokenKey = "authToken";

    public CustomAuthorizationHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
