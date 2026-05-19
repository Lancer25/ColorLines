using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ColorLines.Windows.ViewModels;

public sealed class ShellViewModel : INotifyPropertyChanged
{
    private GameViewModel game;
    private ShellScreen currentScreen;
    private ShellScreen settingsReturnScreen;
    private string pauseSaveStatusText;

    public ShellViewModel(GameViewModel game)
    {
        this.game = game;
        this.game.PropertyChanged += GamePropertyChanged;
        currentScreen = ShellScreen.MainMenu;
        settingsReturnScreen = ShellScreen.MainMenu;
        pauseSaveStatusText = string.Empty;
        ContinueCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.Playing);
        NewGameCommand = new RelayCommand(_ => StartNewGame());
        OpenPauseMenuCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.PauseMenu);
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        CloseSettingsCommand = new RelayCommand(_ => CurrentScreen = settingsReturnScreen);
        BackToMenuCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.MainMenu);
        BackToGameCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.Playing);
        SaveGameCommand = new RelayCommand(_ => SaveGame());
        ExitCommand = new RelayCommand(_ => ExitRequested?.Invoke(this, EventArgs.Empty));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? ExitRequested;

    public event EventHandler? SaveRequested;

    public GameViewModel Game
    {
        get => game;
        private set
        {
            if (!ReferenceEquals(game, value))
            {
                game.PropertyChanged -= GamePropertyChanged;
                game = value;
                game.PropertyChanged += GamePropertyChanged;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SaveSummaryText));
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
                OnPropertyChanged(nameof(IsPauseMenuVisible));
                OnPropertyChanged(nameof(IsSettingsVisible));
            }
        }
    }

    public bool IsMainMenuVisible => CurrentScreen == ShellScreen.MainMenu;

    public bool IsPlayingVisible => CurrentScreen == ShellScreen.Playing;

    public bool IsPauseMenuVisible => CurrentScreen == ShellScreen.PauseMenu;

    public bool IsSettingsVisible => CurrentScreen == ShellScreen.Settings;

    public string PauseSaveStatusText
    {
        get => pauseSaveStatusText;
        private set
        {
            if (pauseSaveStatusText != value)
            {
                pauseSaveStatusText = value;
                OnPropertyChanged();
            }
        }
    }

    public string SaveSummaryText => $"Saved run: Score {Game.Score} | Best {Game.HighScore}";

    public ICommand ContinueCommand { get; }

    public ICommand NewGameCommand { get; }

    public ICommand OpenPauseMenuCommand { get; }

    public ICommand OpenSettingsCommand { get; }

    public ICommand CloseSettingsCommand { get; }

    public ICommand BackToMenuCommand { get; }

    public ICommand BackToGameCommand { get; }

    public ICommand SaveGameCommand { get; }

    public ICommand ExitCommand { get; }

    private void OpenSettings()
    {
        settingsReturnScreen = CurrentScreen == ShellScreen.PauseMenu
            ? ShellScreen.PauseMenu
            : ShellScreen.MainMenu;
        CurrentScreen = ShellScreen.Settings;
    }

    private void StartNewGame()
    {
        Game.NewGameCommand.Execute(null);
        OnPropertyChanged(nameof(SaveSummaryText));
        CurrentScreen = ShellScreen.Playing;
    }

    private void SaveGame()
    {
        SaveRequested?.Invoke(this, EventArgs.Empty);
        PauseSaveStatusText = "Game saved.";
    }

    private void GamePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GameViewModel.Score) or nameof(GameViewModel.HighScore))
        {
            OnPropertyChanged(nameof(SaveSummaryText));
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
