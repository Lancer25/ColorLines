namespace ColorLines.Windows.ViewModels;

public sealed class SaveRequestedEventArgs : EventArgs
{
    public bool WasHandled { get; private set; }

    public bool WasSuccessful { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void MarkSucceeded()
    {
        WasHandled = true;
        WasSuccessful = true;
        ErrorMessage = null;
    }

    public void MarkFailed(string? errorMessage = null)
    {
        WasHandled = true;
        WasSuccessful = false;
        ErrorMessage = errorMessage;
    }
}
