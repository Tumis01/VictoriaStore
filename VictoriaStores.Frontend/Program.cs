using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using VictoriaStores.Frontend.Components;
using VictoriaStores.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register the custom authorization handler
builder.Services.AddScoped<CustomAuthorizationHandler>();

// Create a Scoped HttpClient tied directly to the active user's Blazor circuit
builder.Services.AddScoped(sp =>
{
    var authHandler = sp.GetRequiredService<CustomAuthorizationHandler>();
    authHandler.InnerHandler = new HttpClientHandler(); // Set the base handler

    var client = new HttpClient(authHandler)
    {
        BaseAddress = new Uri("https://api.vjstores.shifts.com.ng/"),
        Timeout = TimeSpan.FromMinutes(10)
    };

    return client;
});
// Set it as the default HttpClient for the app
//builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("VictoriaApi"));

// Local storage
builder.Services.AddBlazoredLocalStorage();

// Custom services and authentication
builder.Services.AddScoped<ProductApiClient>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddAuthenticationCore();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
