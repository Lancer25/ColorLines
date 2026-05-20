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
    private string menuNoticeText;
    private SaveStatus lastSaveStatus;
    private bool isReturnToMenuConfirmVisible;
    private bool isEndGameConfirmVisible;

    public ShellViewModel(GameViewModel game)
    {
        this.game = game;
        this.game.PropertyChanged += GamePropertyChanged;
        currentScreen = ShellScreen.MainMenu;
        settingsReturnScreen = ShellScreen.MainMenu;
        pauseSaveStatusText = string.Empty;
        menuNoticeText = string.Empty;
        lastSaveStatus = SaveStatus.None;
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

    public event EventHandler<SaveRequestedEventArgs>? SaveRequested;

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

    public string MenuNoticeText
    {
        get => menuNoticeText;
        private set
        {
            if (menuNoticeText != value)
            {
                menuNoticeText = value;
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

    public string ToggleSoundText => Game.IsSoundEnabled
        ? (IsChinese ? "关闭声音" : "Turn Sound Off")
        : (IsChinese ? "开启声音" : "Turn Sound On");

    public string GameplayTabText => IsChinese ? "玩法" : "Gameplay";

    public string AudioTabText => IsChinese ? "音频" : "Audio";

    public string DisplayTabText => IsChinese ? "显示" : "Display";

    public string PathHintsText => IsChinese ? "路径提示" : "Path Hints";

    public string PathHintsStatusText => IsChinese ? $"状态：{DisplayPathHintsEnabled}" : $"Status: {Game.IsPathHintsEnabled}";

    public string TogglePathHintsText => Game.IsPathHintsEnabled
        ? (IsChinese ? "关闭提示" : "Turn Hints Off")
        : (IsChinese ? "开启提示" : "Turn Hints On");

    public string AutoSaveText => IsChinese ? "自动保存" : "Auto Save";

    public string AutoSaveStatusText => IsChinese ? $"状态：{DisplayAutoSaveEnabled}" : $"Status: {Game.IsAutoSaveEnabled}";

    public string ToggleAutoSaveText => Game.IsAutoSaveEnabled
        ? (IsChinese ? "关闭自动保存" : "Turn Auto Save Off")
        : (IsChinese ? "开启自动保存" : "Turn Auto Save On");

    public string LanguageText => IsChinese ? "语言" : "Language";

    public string DifficultyText => IsChinese ? "难度" : "Difficulty";

    public string ThemeText => IsChinese ? "主题" : "Theme";

    public string ThemeCurrentText => IsChinese ? "当前：温馨棋盘" : $"Current: {Game.SelectedThemeName}";

    public string ThemeOptionText => IsChinese ? "温馨棋盘" : Game.SelectedThemeName;

    public string ThemeUnavailableText => IsChinese ? "更多主题稍后加入。" : "More themes will be added later.";

    public string ReadyToPlayText => IsChinese ? "准备开始" : "Ready to play";

    public string MenuTaglineText => IsChinese ? "连成五只猫咪，清空棋盘，刷新最高分。" : "Match five cats. Clear the board. Chase your best score.";

    public string ScoreText => IsChinese ? "分数" : "Score";

    public string BestText => IsChinese ? $"最高：{Game.HighScore}" : $"Best: {Game.HighScore}";

    public string BestScoreSummaryText => IsChinese ? $"最高分：{Game.HighScore}" : $"Best Score: {Game.HighScore}";

    public string NextCatsText => IsChinese ? "下批猫咪" : "Next Cats";

    public string ThemeSummaryText => IsChinese ? "主题：温馨棋盘" : $"Theme: {Game.SelectedThemeName}";

    public string AnimationSummaryText => IsChinese ? $"动效：{DisplayAnimationIntensity}" : $"Animation: {Game.AnimationIntensity}";

    public string SoundSummaryText => IsChinese ? $"声音：{DisplaySoundEnabled}" : $"Sound: {Game.IsSoundEnabled}";

    public string CurrentAnimationText => IsChinese ? $"当前：{DisplayAnimationIntensity}" : $"Current: {Game.AnimationIntensity}";

    public string SoundEnabledText => IsChinese ? $"启用：{DisplayYesNo}" : $"Enabled: {Game.IsSoundEnabled}";

    public string LanguageSummaryText => IsChinese ? "当前：中文" : "Current: English";

    public string DifficultySummaryText => IsChinese ? $"下一局棋盘：{DisplayDifficulty}" : $"Next board: {Game.Difficulty}";

    public string EnglishLanguageText => "English";

    public string ChineseLanguageText => IsChinese ? "中文" : "中文";

    public string EnglishLanguageButtonTag => Language == "en" ? "SettingsSelectedButton" : "MenuSecondaryButton";

    public string ChineseLanguageButtonTag => Language == "zh" ? "SettingsSelectedButton" : "MenuSecondaryButton";

    public string EasyText => IsChinese ? "简单" : "Easy";

    public string NormalText => IsChinese ? "普通" : "Normal";

    public string HardText => IsChinese ? "困难" : "Hard";

    public string EasyDifficultyButtonTag => Game.Difficulty == DifficultyCatalog.Easy ? "SettingsSelectedButton" : "MenuSecondaryButton";

    public string NormalDifficultyButtonTag => Game.Difficulty == DifficultyCatalog.Normal ? "SettingsSelectedButton" : "MenuSecondaryButton";

    public string HardDifficultyButtonTag => Game.Difficulty == DifficultyCatalog.Hard ? "SettingsSelectedButton" : "MenuSecondaryButton";

    public string EndGameConfirmTitle => IsChinese ? "结束当前游戏？" : "End current game?";

    public string EndGameConfirmBody => IsChinese
        ? "本局会结束，并打开最终分数界面。"
        : "This run will end and the final score screen will open.";

    public string KeepPlayingText => IsChinese ? "继续游戏" : "Keep Playing";

    public string ReturnToMenuConfirmTitle => IsChinese ? "返回主菜单？" : "Return to main menu?";

    public string ReturnToMenuConfirmBody => IsChinese
        ? "当前这一局未保存的进度会丢失。"
        : "Unsaved progress in the current run will be lost.";

    public string StayInGameText => IsChinese ? "留在游戏" : "Stay in Game";

    public string ReturnAnywayText => IsChinese ? "仍然返回" : "Return Anyway";

    public string BackToMenuText => IsChinese ? "返回菜单" : "Back to Menu";

    private bool IsChinese => Language == "zh";

    private string DisplayAnimationIntensity => Game.AnimationIntensity == "Full"
        ? (IsChinese ? "完整" : "Full")
        : (IsChinese ? "精简" : "Reduced");

    private string DisplaySoundEnabled => Game.IsSoundEnabled
        ? (IsChinese ? "开" : "True")
        : (IsChinese ? "关" : "False");

    private string DisplayPathHintsEnabled => Game.IsPathHintsEnabled
        ? (IsChinese ? "开" : "True")
        : (IsChinese ? "关" : "False");

    private string DisplayAutoSaveEnabled => Game.IsAutoSaveEnabled
        ? (IsChinese ? "开" : "True")
        : (IsChinese ? "关" : "False");

    private string DisplayYesNo => Game.IsSoundEnabled
        ? (IsChinese ? "是" : "True")
        : (IsChinese ? "否" : "False");

    private string DisplayDifficulty => Game.Difficulty switch
    {
        DifficultyCatalog.Easy => IsChinese ? "简单" : "Easy",
        DifficultyCatalog.Hard => IsChinese ? "困难" : "Hard",
        _ => IsChinese ? "普通" : "Normal"
    };

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
        ClearSaveStatus();
        Game.NewGameCommand.Execute(null);
        OnContinueStateChanged();
        CurrentScreen = ShellScreen.Playing;
    }

    private void SaveGame()
    {
        IsReturnToMenuConfirmVisible = false;
        IsEndGameConfirmVisible = false;
        var args = new SaveRequestedEventArgs();
        SaveRequested?.Invoke(this, args);
        lastSaveStatus = args.WasSuccessful ? SaveStatus.Success : SaveStatus.Failure;
        RefreshPauseSaveStatusText();
    }

    public void SetMenuNotice(string message)
    {
        MenuNoticeText = message;
    }

    private void SetLanguage(object? parameter)
    {
        Game.SetLanguageCommand.Execute(parameter);
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(SaveSummaryText));
        OnLocalizedTextChanged();
        RefreshPauseSaveStatusText();
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
        ClearSaveStatus();
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
            ClearSaveStatus();
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
            OnPropertyChanged(nameof(BestText));
            OnPropertyChanged(nameof(BestScoreSummaryText));
        }

        if (e.PropertyName is nameof(GameViewModel.AnimationIntensity))
        {
            OnPropertyChanged(nameof(AnimationSummaryText));
            OnPropertyChanged(nameof(CurrentAnimationText));
        }

        if (e.PropertyName is nameof(GameViewModel.IsSoundEnabled))
        {
            OnPropertyChanged(nameof(SoundSummaryText));
            OnPropertyChanged(nameof(SoundEnabledText));
            OnPropertyChanged(nameof(ToggleSoundText));
        }

        if (e.PropertyName is nameof(GameViewModel.IsPathHintsEnabled))
        {
            OnPropertyChanged(nameof(PathHintsStatusText));
            OnPropertyChanged(nameof(TogglePathHintsText));
        }

        if (e.PropertyName is nameof(GameViewModel.IsAutoSaveEnabled))
        {
            OnPropertyChanged(nameof(AutoSaveStatusText));
            OnPropertyChanged(nameof(ToggleAutoSaveText));
        }

        if (e.PropertyName is nameof(GameViewModel.Difficulty))
        {
            OnPropertyChanged(nameof(DifficultySummaryText));
            OnDifficultySelectionChanged();
        }

        if (e.PropertyName is nameof(GameViewModel.Language))
        {
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(SaveSummaryText));
            OnLocalizedTextChanged();
            OnLanguageSelectionChanged();
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
        OnPropertyChanged(nameof(GameplayTabText));
        OnPropertyChanged(nameof(AudioTabText));
        OnPropertyChanged(nameof(DisplayTabText));
        OnPropertyChanged(nameof(PathHintsText));
        OnPropertyChanged(nameof(PathHintsStatusText));
        OnPropertyChanged(nameof(TogglePathHintsText));
        OnPropertyChanged(nameof(AutoSaveText));
        OnPropertyChanged(nameof(AutoSaveStatusText));
        OnPropertyChanged(nameof(ToggleAutoSaveText));
        OnPropertyChanged(nameof(LanguageText));
        OnPropertyChanged(nameof(DifficultyText));
        OnPropertyChanged(nameof(ThemeText));
        OnPropertyChanged(nameof(ThemeCurrentText));
        OnPropertyChanged(nameof(ThemeOptionText));
        OnPropertyChanged(nameof(ThemeUnavailableText));
        OnPropertyChanged(nameof(ReadyToPlayText));
        OnPropertyChanged(nameof(MenuTaglineText));
        OnPropertyChanged(nameof(ScoreText));
        OnPropertyChanged(nameof(BestText));
        OnPropertyChanged(nameof(BestScoreSummaryText));
        OnPropertyChanged(nameof(NextCatsText));
        OnPropertyChanged(nameof(ThemeSummaryText));
        OnPropertyChanged(nameof(AnimationSummaryText));
        OnPropertyChanged(nameof(SoundSummaryText));
        OnPropertyChanged(nameof(CurrentAnimationText));
        OnPropertyChanged(nameof(SoundEnabledText));
        OnPropertyChanged(nameof(LanguageSummaryText));
        OnPropertyChanged(nameof(DifficultySummaryText));
        OnPropertyChanged(nameof(EnglishLanguageText));
        OnPropertyChanged(nameof(ChineseLanguageText));
        OnLanguageSelectionChanged();
        OnPropertyChanged(nameof(EasyText));
        OnPropertyChanged(nameof(NormalText));
        OnPropertyChanged(nameof(HardText));
        OnDifficultySelectionChanged();
        OnPropertyChanged(nameof(EndGameConfirmTitle));
        OnPropertyChanged(nameof(EndGameConfirmBody));
        OnPropertyChanged(nameof(KeepPlayingText));
        OnPropertyChanged(nameof(ReturnToMenuConfirmTitle));
        OnPropertyChanged(nameof(ReturnToMenuConfirmBody));
        OnPropertyChanged(nameof(StayInGameText));
        OnPropertyChanged(nameof(ReturnAnywayText));
        OnPropertyChanged(nameof(BackToMenuText));
    }

    private void OnLanguageSelectionChanged()
    {
        OnPropertyChanged(nameof(EnglishLanguageButtonTag));
        OnPropertyChanged(nameof(ChineseLanguageButtonTag));
    }

    private void OnDifficultySelectionChanged()
    {
        OnPropertyChanged(nameof(EasyDifficultyButtonTag));
        OnPropertyChanged(nameof(NormalDifficultyButtonTag));
        OnPropertyChanged(nameof(HardDifficultyButtonTag));
    }

    private void OnPauseMenuStateChanged()
    {
        OnPropertyChanged(nameof(CanOpenPauseMenu));
        if (OpenPauseMenuCommand is RelayCommand openPauseMenuCommand)
        {
            openPauseMenuCommand.RaiseCanExecuteChanged();
        }
    }

    private void RefreshPauseSaveStatusText()
    {
        PauseSaveStatusText = lastSaveStatus switch
        {
            SaveStatus.Success => Language == "zh" ? "游戏已保存。" : "Game saved.",
            SaveStatus.Failure => Language == "zh" ? "保存失败，请重试。" : "Save failed. Please try again.",
            _ => string.Empty
        };
    }

    private void ClearSaveStatus()
    {
        lastSaveStatus = SaveStatus.None;
        RefreshPauseSaveStatusText();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private enum SaveStatus
    {
        None,
        Success,
        Failure
    }
}
