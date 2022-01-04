using System.Collections.ObjectModel;
using Avalonia.Layout;
using ReactiveUI;

namespace TestPanel.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<string>? _items;
    private Orientation _orientation;
    private double _spacing;
    private double _itemSize;

    public ObservableCollection<string>? Items
    {
        get => _items;
        set => this.RaiseAndSetIfChanged(ref _items, value);
    }

    public Orientation Orientation
    {
        get => _orientation;
        set => this.RaiseAndSetIfChanged(ref _orientation, value);
    }
    
    public double Spacing
    {
        get => _spacing;
        set => this.RaiseAndSetIfChanged(ref _spacing, value);
    }

    public double ItemSize
    {
        get => _itemSize;
        set => this.RaiseAndSetIfChanged(ref _itemSize, value);
    }

    public MainWindowViewModel()
    {
        var totalItems = 1_000_000;

        _items = new ObservableCollection<string>();

        for (var i = 0; i < totalItems; i++)
        {
            _items.Add($"Item {i}");
        }

        _orientation = Orientation.Vertical;
        _spacing = 1;
        _itemSize = 100;
    }
}
