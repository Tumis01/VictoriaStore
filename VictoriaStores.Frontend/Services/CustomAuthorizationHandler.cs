using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace VictoriaStores.Frontend.Services;

public class CustomAuthorizationHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public CustomAuthorizationHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // Attempt to grab the token securely from local storage
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (InvalidOperationException)
        {
            // Prerendering phase: JavaScript interop is not available yet.
            // We safely ignore this; the real call will happen once the browser connects.
        }

        return await base.SendAsync(request, cancellationToken);
    }
}