using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace CubaseDrumMapEditor.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private IDropHandler _dndDropHandler = null!;

    public static readonly DirectProperty<MainView, IDropHandler> DndDropHandlerProperty =
      AvaloniaProperty.RegisterDirect<MainView, IDropHandler>(
        nameof(DndDropHandler), o => o.DndDropHandler, (o, v) => o.DndDropHandler = v);

    public IDropHandler DndDropHandler
    {
        get => _dndDropHandler;
        set => SetAndRaise(DndDropHandlerProperty, ref _dndDropHandler, value);
    }
}
