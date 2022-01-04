using System.Collections.ObjectModel;
using ReactiveUI;

namespace TestPanel.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<string>? _items;

        public ObservableCollection<string>? Items
        {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        public MainWindowViewModel()
        {
            var totalItems = 1_000_000;

            _items = new ObservableCollection<string>();

            for (var i = 0; i < totalItems; i++)
            {
                _items.Add($"Item {i}");
            }
        }
    }
}
