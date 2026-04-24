namespace VictoriaStore.API.Models;

public class StoreSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string StoreName { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;

    public bool NewOrderEmail { get; set; } = true;
    public bool LowStockAlert { get; set; } = true;

    public string SeoTitle { get; set; } = string.Empty;
    public string SeoDescription { get; set; } = string.Empty;
    public string SeoKeywords { get; set; } = string.Empty;
}
