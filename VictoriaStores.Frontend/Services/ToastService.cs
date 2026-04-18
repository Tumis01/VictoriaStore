namespace VictoriaStores.Frontend.Services;

public class ToastService
{
    public event Action<string, string, string>? OnShow;

    public void ShowSuccess(string title, string message)
    {
        OnShow?.Invoke("success", title, message);
    }

    public void ShowError(string title, string message)
    {
        OnShow?.Invoke("error", title, message);
    }

    public void ShowInfo(string title, string message)
    {
        OnShow?.Invoke("info", title, message);
    }
}
