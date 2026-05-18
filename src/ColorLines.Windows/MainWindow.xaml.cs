using System.Windows;
using System.Windows.Input;
using ColorLines.Windows.Services;
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
        var save = this.saveService.Load();
        shellViewModel = new ShellViewModel(GameViewModel.CreateFromSave(save));
        shellViewModel.ExitRequested += (_, _) => Close();
        DataContext = shellViewModel;

        if (save?.Window is not null)
        {
            Width = Math.Max(MinWidth, save.Window.Width);
            Height = Math.Max(MinHeight, save.Window.Height);
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        saveService.Save(shellViewModel.Game.CreateSaveData(new WindowPlacementData(Width, Height)));
        base.OnClosing(e);
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
