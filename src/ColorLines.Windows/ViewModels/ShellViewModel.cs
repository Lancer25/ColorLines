using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ColorLines.Windows.ViewModels;

public sealed class ShellViewModel : INotifyPropertyChanged
{
    private GameViewModel game;
    private ShellScreen currentScreen;

    public ShellViewModel(GameViewModel game)
    {
        this.game = game;
        currentScreen = ShellScreen.MainMenu;
        ContinueCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.Playing);
        NewGameCommand = new RelayCommand(_ => StartNewGame());
        OpenSettingsCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.Settings);
        BackToMenuCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.MainMenu);
        BackToGameCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.Playing);
        ExitCommand = new RelayCommand(_ => ExitRequested?.Invoke(this, EventArgs.Empty));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? ExitRequested;

    public GameViewModel Game
    {
        get => game;
        private set
        {
            if (!ReferenceEquals(game, value))
            {
                game = value;
                OnPropertyChanged();
            }
        }
    }

    public ShellScreen CurrentScreen
    {
        get => currentScreen;
        private set
        {
            if (currentScreen != value)
            {
                currentScreen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMainMenuVisible));
                OnPropertyChanged(nameof(IsPlayingVisible));
                OnPropertyChanged(nameof(IsSettingsVisible));
            }
        }
    }

    public bool IsMainMenuVisible => CurrentScreen == ShellScreen.MainMenu;

    public bool IsPlayingVisible => CurrentScreen == ShellScreen.Playing;

    public bool IsSettingsVisible => CurrentScreen == ShellScreen.Settings;

    public ICommand ContinueCommand { get; }

    public ICommand NewGameCommand { get; }

    public ICommand OpenSettingsCommand { get; }

    public ICommand BackToMenuCommand { get; }

    public ICommand BackToGameCommand { get; }

    public ICommand ExitCommand { get; }

    private void StartNewGame()
    {
        Game.NewGameCommand.Execute(null);
        CurrentScreen = ShellScreen.Playing;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
