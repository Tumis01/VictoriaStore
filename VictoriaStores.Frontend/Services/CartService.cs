using Blazored.LocalStorage;
using VictoriaStores.Frontend.Models;

namespace VictoriaStores.Frontend.Services;

public class CartService
{
    private readonly ILocalStorageService _localStorage;
    private const string CartKey = "victoria_store_cart";

    // Event to notify the UI (like the header cart icon) when the cart changes
    public event Action? OnCartChanged;

    public CartService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<CartItemDto>> GetCartAsync()
    {
        return await _localStorage.GetItemAsync<List<CartItemDto>>(CartKey) ?? new List<CartItemDto>();
    }

    public async Task AddToCartAsync(CartItemDto item)
    {
        var cart = await GetCartAsync();
        var existingItem = cart.FirstOrDefault(x => x.ProductId == item.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            cart.Add(item);
        }

        await _localStorage.SetItemAsync(CartKey, cart);
        OnCartChanged?.Invoke();
    }

    public async Task ClearCartAsync()
    {
        await _localStorage.RemoveItemAsync(CartKey);
        OnCartChanged?.Invoke();
    }
}