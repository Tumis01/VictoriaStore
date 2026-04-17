using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using VictoriaStores.Frontend.Components;
using VictoriaStores.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // Enables real-time UI updates

// 1. Register the custom authorization handler
builder.Services.AddScoped<CustomAuthorizationHandler>();

// 2. Configure a named HttpClient that uses our handler
builder.Services.AddHttpClient("VictoriaApi", client =>
{
    // Make sure this matches your API port
    client.BaseAddress = new Uri("https://localhost:7145/");
}).AddHttpMessageHandler<CustomAuthorizationHandler>();

// 3. Set it as the default HttpClient for the entire app
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("VictoriaApi"));

// 4. Register Local Storage for the Shopping Cart
builder.Services.AddBlazoredLocalStorage();

// 5. Register our Custom Services and Authentication
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