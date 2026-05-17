using System.Windows;
using ColorLines.Windows.Services;
using ColorLines.Windows.ViewModels;

namespace ColorLines.Windows;

public partial class MainWindow : Window
{
    private readonly LocalSaveService saveService;
    private readonly GameViewModel viewModel;

    public MainWindow()
        : this(LocalSaveService.CreateDefault())
    {
    }

    public MainWindow(LocalSaveService saveService)
    {
        InitializeComponent();

        this.saveService = saveService;
        var save = this.saveService.Load();
        viewModel = GameViewModel.CreateFromSave(save);
        DataContext = viewModel;

        if (save?.Window is not null)
        {
            Width = Math.Max(MinWidth, save.Window.Width);
            Height = Math.Max(MinHeight, save.Window.Height);
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        saveService.Save(viewModel.CreateSaveData(new WindowPlacementData(Width, Height)));
        base.OnClosing(e);
    }
}
