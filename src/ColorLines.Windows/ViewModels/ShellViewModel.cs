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
    private bool isReturnToMenuConfirmVisible;
    private bool isEndGameConfirmVisible;

    public ShellViewModel(GameViewModel game)
    {
        this.game = game;
        this.game.PropertyChanged += GamePropertyChanged;
        currentScreen = ShellScreen.MainMenu;
        settingsReturnScreen = ShellScreen.MainMenu;
        pauseSaveStatusText = string.Empty;
        ContinueCommand = new RelayCommand(_ => ReturnToGame(), _ => CanContinueGame);
        NewGameCommand = new RelayCommand(_ => StartNewGame());
        OpenPauseMenuCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.PauseMenu);
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        CloseSettingsCommand = new RelayCommand(_ => CurrentScreen = settingsReturnScreen);
        BackToMenuCommand = new RelayCommand(_ => ReturnToMainMenu());
        RequestBackToMenuCommand = new RelayCommand(_ => RequestBackToMenu());
        ConfirmBackToMenuCommand = new RelayCommand(_ => ReturnToMainMenu());
        CancelBackToMenuCommand = new RelayCommand(_ => IsReturnToMenuConfirmVisible = false);
        BackToGameCommand = new RelayCommand(_ => ReturnToGame());
        SaveGameCommand = new RelayCommand(_ => SaveGame());
        EndGameCommand = new RelayCommand(_ => RequestEndGame());
        ConfirmEndGameCommand = new RelayCommand(_ => EndGame());
        CancelEndGameCommand = new RelayCommand(_ => IsEndGameConfirmVisible = false);
        EscapeCommand = new RelayCommand(_ => HandleEscape());
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

    public string SaveSummaryText => CanContinueGame
        ? $"Continue available: Score {Game.Score} | Best {Game.HighScore}"
        : $"Start a new game: Score {Game.Score} | Best {Game.HighScore}";

    public bool CanContinueGame => !Game.IsGameOver;

    public bool IsReturnToMenuConfirmVisible
    {
        get => isReturnToMenuConfirmVisible;
        private set
        {
            if (isReturnToMenuConfirmVisible != value)
            {
                isReturnToMenuConfirmVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEndGameConfirmVisible
    {
        get => isEndGameConfirmVisible;
        private set
        {
            if (isEndGameConfirmVisible != value)
            {
                isEndGameConfirmVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand ContinueCommand { get; }

    public ICommand NewGameCommand { get; }

    public ICommand OpenPauseMenuCommand { get; }

    public ICommand OpenSettingsCommand { get; }

    public ICommand CloseSettingsCommand { get; }

    public ICommand BackToMenuCommand { get; }

    public ICommand RequestBackToMenuCommand { get; }

    public ICommand ConfirmBackToMenuCommand { get; }

    public ICommand CancelBackToMenuCommand { get; }

    public ICommand BackToGameCommand { get; }

    public ICommand SaveGameCommand { get; }

    public ICommand EndGameCommand { get; }

    public ICommand ConfirmEndGameCommand { get; }

    public ICommand CancelEndGameCommand { get; }

    public ICommand EscapeCommand { get; }

    public ICommand ExitCommand { get; }

    private void OpenSettings()
    {
        IsReturnToMenuConfirmVisible = false;
        IsEndGameConfirmVisible = false;
        settingsReturnScreen = CurrentScreen == ShellScreen.PauseMenu
            ? ShellScreen.PauseMenu
            : ShellScreen.MainMenu;
        CurrentScreen = ShellScreen.Settings;
    }

    private void StartNewGame()
    {
        IsReturnToMenuConfirmVisible = false;
        IsEndGameConfirmVisible = false;
        Game.NewGameCommand.Execute(null);
        OnContinueStateChanged();
        CurrentScreen = ShellScreen.Playing;
    }

    private void SaveGame()
    {
        IsReturnToMenuConfirmVisible = false;
        IsEndGameConfirmVisible = false;
        SaveRequested?.Invoke(this, EventArgs.Empty);
        PauseSaveStatusText = "Game saved.";
    }

    private void RequestBackToMenu()
    {
        IsEndGameConfirmVisible = false;
        IsReturnToMenuConfirmVisible = true;
    }

    private void RequestEndGame()
    {
        IsReturnToMenuConfirmVisible = false;
        IsEndGameConfirmVisible = true;
    }

    private void EndGame()
    {
        IsReturnToMenuConfirmVisible = false;
        IsEndGameConfirmVisible = false;
        Game.EndGameCommand.Execute(null);
        OnContinueStateChanged();
        CurrentScreen = ShellScreen.Playing;
    }

    private void HandleEscape()
    {
        if (IsEndGameConfirmVisible)
        {
            IsEndGameConfirmVisible = false;
            return;
        }

        if (IsReturnToMenuConfirmVisible)
        {
            IsReturnToMenuConfirmVisible = false;
            return;
        }

        if (CurrentScreen == ShellScreen.Playing)
        {
            CurrentScreen = ShellScreen.PauseMenu;
            return;
        }

        if (CurrentScreen == ShellScreen.PauseMenu)
        {
            ReturnToGame();
            return;
        }

        if (CurrentScreen == ShellScreen.Settings)
        {
            CurrentScreen = settingsReturnScreen;
        }
    }

    private void ReturnToMainMenu()
    {
        IsReturnToMenuConfirmVisible = false;
        IsEndGameConfirmVisible = false;
        CurrentScreen = ShellScreen.MainMenu;
    }

    private void ReturnToGame()
    {
        IsReturnToMenuConfirmVisible = false;
        IsEndGameConfirmVisible = false;
        CurrentScreen = ShellScreen.Playing;
    }

    private void GamePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GameViewModel.Score) or nameof(GameViewModel.HighScore))
        {
            OnPropertyChanged(nameof(SaveSummaryText));
        }

        if (e.PropertyName is nameof(GameViewModel.IsGameOver))
        {
            OnContinueStateChanged();
        }
    }

    private void OnContinueStateChanged()
    {
        OnPropertyChanged(nameof(CanContinueGame));
        OnPropertyChanged(nameof(SaveSummaryText));
        if (ContinueCommand is RelayCommand continueCommand)
        {
            continueCommand.RaiseCanExecuteChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
