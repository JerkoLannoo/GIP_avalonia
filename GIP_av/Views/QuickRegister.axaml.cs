using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GIP_av.Views;

namespace GIP_av;

public partial class QuickRegister : Window
{
    public QuickRegister()
    {
        InitializeComponent();
    }

	private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		MainWindow window = new MainWindow();
		window.Show();
		this.Close();
	}
}