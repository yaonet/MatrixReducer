using System.Windows;
using System.Windows.Controls;

namespace MatrixReducer;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    private void Reduce_Click(object sender, RoutedEventArgs e)
    {
        _vm.ExecuteReduce();
    }

    private void Example_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int index))
        {
            _vm.LoadExample(index);
        }
    }
}
