using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CubaseDrumMapEditor.ViewModels;
using System.Diagnostics;

namespace CubaseDrumMapEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() is { } fileNames)
        {
            foreach (var file in fileNames)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.ProcessDroppedFile(file.TryGetLocalPath()!);
                }
                break;
            }
        }
    }
}
