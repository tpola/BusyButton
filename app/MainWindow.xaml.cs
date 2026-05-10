using System.Threading.Tasks;
using System.Windows;

namespace BusyButton;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SuccessButton_Click(object sender, RoutedEventArgs e)
    {
        SuccessButton.IsBusy = true;
        await Task.Delay(1400);
        SuccessButton.ShowSuccess();
    }

    private async void ErrorButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorButton.IsBusy = true;
        await Task.Delay(1400);
        ErrorButton.ShowError();
    }

    private async void CompactButton_Click(object sender, RoutedEventArgs e)
    {
        CompactButton.IsBusy = true;
        await Task.Delay(1400);
        CompactButton.ShowSuccess();
    }

    private async void CompactButton2_Click(object sender, RoutedEventArgs e)
    {
        CompactButton2.IsBusy = true;
        await Task.Delay(1400);
        CompactButton2.ShowSuccess();
    }

    private async void IconContentButton_Click(object sender, RoutedEventArgs e)
    {
        IconContentButton.IsBusy = true;
        await Task.Delay(1600);
        IconContentButton.ShowSuccess();
    }

    private async void NoIndicatorButton_Click(object sender, RoutedEventArgs e)
    {
        NoIndicatorButton.IsBusy = true;
        await Task.Delay(1200);
        NoIndicatorButton.ShowSuccess();
    }

    private async void TemplatedContentButton_Click(object sender, RoutedEventArgs e)
    {
        TemplatedContentButton.IsBusy = true;
        await Task.Delay(1500);
        TemplatedContentButton.ShowSuccess();
    }
}

public sealed class CommandLabel
{
    public required string Title { get; init; }

    public required string Detail { get; init; }
}
