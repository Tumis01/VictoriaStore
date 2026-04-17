using System.Net.Http.Json;
// You will need to copy your DTO classes (ProductDto, etc.) from the API to a Models/DTOs folder in the frontend, 
// or create a shared class library. For now, assuming they are in VictoriaStore.Frontend.Models
using VictoriaStores.Frontend.Models;

namespace VictoriaStores.Frontend.Services;

public class ProductApiClient
{
    private readonly HttpClient _httpClient;

    public ProductApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProductDto>?> GetActiveProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/product");
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<ProductDto>($"api/product/{id}");
    }
}