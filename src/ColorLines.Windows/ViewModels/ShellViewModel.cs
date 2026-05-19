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
        OpenPauseMenuCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.PauseMenu, _ => CanOpenPauseMenu);
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        CloseSettingsCommand = new RelayCommand(_ => CurrentScreen = settingsReturnScreen);
        SetLanguageCommand = new RelayCommand(SetLanguage);
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
                OnPauseMenuStateChanged();
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
        ? (Language == "zh"
            ? $"继续游戏：分数 {Game.Score} | 最高 {Game.HighScore}"
            : $"Continue available: Score {Game.Score} | Best {Game.HighScore}")
        : (Language == "zh"
            ? $"开始新游戏：分数 {Game.Score} | 最高 {Game.HighScore}"
            : $"Start a new game: Score {Game.Score} | Best {Game.HighScore}");

    public string Language => Game.Language;

    public string ContinueText => IsChinese ? "继续游戏" : "Continue";

    public string NewGameText => IsChinese ? "新游戏" : "New Game";

    public string SettingsText => IsChinese ? "设置" : "Settings";

    public string ExitText => IsChinese ? "退出" : "Exit";

    public string MenuText => IsChinese ? "菜单" : "Menu";

    public string SaveGameText => IsChinese ? "保存游戏" : "Save Game";

    public string EndGameText => IsChinese ? "结束游戏" : "End Game";

    public string ReturnToMainMenuText => IsChinese ? "返回主菜单" : "Return to Main Menu";

    public string BackText => IsChinese ? "返回" : "Back";

    public string GameMenuTitle => IsChinese ? "游戏菜单" : "Game Menu";

    public string PauseSubtitle => IsChinese ? "棋盘会在这里等你。" : "The board is waiting.";

    public string SettingsTitle => IsChinese ? "设置" : "Settings";

    public string SettingsSubtitle => IsChinese ? "在下一局开始前调整棋盘。" : "Tune the table before the next run.";

    public string AnimationText => IsChinese ? "动效" : "Animation";

    public string SoundText => IsChinese ? "声音" : "Sound";

    public string ToggleSoundText => IsChinese ? "切换声音" : "Toggle Sound";

    public string LanguageText => IsChinese ? "语言" : "Language";

    public string DifficultyText => IsChinese ? "难度" : "Difficulty";

    private bool IsChinese => Language == "zh";

    public bool CanContinueGame => !Game.IsGameOver;

    public bool CanOpenPauseMenu => CurrentScreen == ShellScreen.Playing && !Game.IsGameOver;

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

    public ICommand SetLanguageCommand { get; }

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
        PauseSaveStatusText = Language == "zh" ? "游戏已保存。" : "Game saved.";
    }

    private void SetLanguage(object? parameter)
    {
        Game.SetLanguageCommand.Execute(parameter);
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(SaveSummaryText));
        OnLocalizedTextChanged();
        if (!string.IsNullOrEmpty(PauseSaveStatusText))
        {
            PauseSaveStatusText = Language == "zh" ? "游戏已保存。" : "Game saved.";
        }
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

        if (CanOpenPauseMenu)
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
        if (!Game.IsGameOver)
        {
            Game.EndGameCommand.Execute(null);
            OnContinueStateChanged();
        }

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

        if (e.PropertyName is nameof(GameViewModel.Language))
        {
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(SaveSummaryText));
            OnLocalizedTextChanged();
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

        OnPauseMenuStateChanged();
    }

    private void OnLocalizedTextChanged()
    {
        OnPropertyChanged(nameof(ContinueText));
        OnPropertyChanged(nameof(NewGameText));
        OnPropertyChanged(nameof(SettingsText));
        OnPropertyChanged(nameof(ExitText));
        OnPropertyChanged(nameof(MenuText));
        OnPropertyChanged(nameof(SaveGameText));
        OnPropertyChanged(nameof(EndGameText));
        OnPropertyChanged(nameof(ReturnToMainMenuText));
        OnPropertyChanged(nameof(BackText));
        OnPropertyChanged(nameof(GameMenuTitle));
        OnPropertyChanged(nameof(PauseSubtitle));
        OnPropertyChanged(nameof(SettingsTitle));
        OnPropertyChanged(nameof(SettingsSubtitle));
        OnPropertyChanged(nameof(AnimationText));
        OnPropertyChanged(nameof(SoundText));
        OnPropertyChanged(nameof(ToggleSoundText));
        OnPropertyChanged(nameof(LanguageText));
        OnPropertyChanged(nameof(DifficultyText));
    }

    private void OnPauseMenuStateChanged()
    {
        OnPropertyChanged(nameof(CanOpenPauseMenu));
        if (OpenPauseMenuCommand is RelayCommand openPauseMenuCommand)
        {
            openPauseMenuCommand.RaiseCanExecuteChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
