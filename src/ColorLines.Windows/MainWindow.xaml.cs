using System.Windows;
using System.Windows.Input;
using ColorLines.Windows.Services;
using ColorLines.Windows.Themes;
using ColorLines.Windows.ViewModels;

namespace ColorLines.Windows;

public partial class MainWindow : Window
{
    private readonly LocalSaveService saveService;
    private readonly ShellViewModel shellViewModel;

    public MainWindow()
        : this(LocalSaveService.CreateDefault())
    {
    }

    public MainWindow(LocalSaveService saveService)
    {
        InitializeComponent();

        this.saveService = saveService;
        LocalSaveData? save = null;
        var loadFailed = false;
        try
        {
            save = this.saveService.Load();
        }
        catch
        {
            loadFailed = true;
        }

        shellViewModel = new ShellViewModel(GameViewModel.CreateFromSave(save));
        if (loadFailed)
        {
            shellViewModel.SetMenuNotice("Save file could not be loaded. Starting fresh.");
        }

        shellViewModel.ExitRequested += (_, _) => Close();
        shellViewModel.Game.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameViewModel.ThemeId))
            {
                ApplyThemeResources(shellViewModel.Game.ThemeId);
            }
        };
        shellViewModel.SaveRequested += (_, args) =>
        {
            if (SaveCurrentGame())
            {
                args.MarkSucceeded();
                return;
            }

            args.MarkFailed();
        };
        ApplyThemeResources(shellViewModel.Game.ThemeId);
        DataContext = shellViewModel;

        if (save?.Window is not null)
        {
            Width = Math.Max(MinWidth, save.Window.Width);
            Height = Math.Max(MinHeight, save.Window.Height);
        }
    }

    private static void ApplyThemeResources(string themeId)
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
        {
            return;
        }

        var existingThemeDictionaries = resources.MergedDictionaries
            .Where(dictionary => dictionary.Source?.OriginalString.Contains("/Themes/", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        foreach (var dictionary in existingThemeDictionaries)
        {
            resources.MergedDictionaries.Remove(dictionary);
        }

        resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = ThemeCatalog.GetResourceUri(themeId)
        });
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (shellViewModel.Game.IsAutoSaveEnabled)
        {
            SaveCurrentGame();
        }

        base.OnClosing(e);
    }

    private bool SaveCurrentGame()
    {
        try
        {
            saveService.Save(shellViewModel.Game.CreateSaveData(new WindowPlacementData(Width, Height)));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void BoardCellPointerEntered(object sender, MouseEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is FrameworkElement element)
        {
            shell.Game.PreviewPathCommand.Execute(element.DataContext);
        }
    }

    private void BoardCellPointerLeft(object sender, MouseEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Game.ClearPreviewPathCommand.Execute(null);
        }
    }
}
