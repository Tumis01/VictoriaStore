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

// Configure a named HttpClient for the API
builder.Services.AddHttpClient("VictoriaApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7145/");
    client.Timeout = TimeSpan.FromMinutes(10);
}).AddHttpMessageHandler<CustomAuthorizationHandler>();

// Set it as the default HttpClient for the app
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("VictoriaApi"));

// Local storage
builder.Services.AddBlazoredLocalStorage();

// Custom services and authentication
builder.Services.AddScoped<ProductApiClient>();
builder.Services.AddScoped<CartService>();
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
