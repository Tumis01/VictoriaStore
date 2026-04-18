using Blazored.LocalStorage;
using VictoriaStores.Frontend.Models;

namespace VictoriaStores.Frontend.Services;

public class CartService
{
    private readonly ILocalStorageService _localStorage;
    private const string CartKey = "victoria_store_cart";

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

    // NEW: Update the quantity of a specific item
    public async Task UpdateQuantityAsync(Guid productId, int newQuantity)
    {
        var cart = await GetCartAsync();
        var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            if (newQuantity <= 0)
            {
                cart.Remove(existingItem); // Remove if quantity drops to 0
            }
            else
            {
                existingItem.Quantity = newQuantity;
            }

            await _localStorage.SetItemAsync(CartKey, cart);
            OnCartChanged?.Invoke();
        }
    }

    // NEW: Completely remove an item from the cart
    public async Task RemoveFromCartAsync(Guid productId)
    {
        var cart = await GetCartAsync();
        var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            cart.Remove(existingItem);
            await _localStorage.SetItemAsync(CartKey, cart);
            OnCartChanged?.Invoke();
        }
    }

    public async Task ClearCartAsync()
    {
        await _localStorage.RemoveItemAsync(CartKey);
        OnCartChanged?.Invoke();
    }
}