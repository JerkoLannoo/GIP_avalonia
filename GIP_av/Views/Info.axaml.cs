using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GIP_av;

public partial class Info : Window
{
    public Info()
    {
        InitializeComponent();
    }
	private void Grid_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
        infoLbl.Text = "Beurt toegevoegd.";
	}

	private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		Dashboard dashboard = new Dashboard();
		dashboard.Show();
		this.Close();
	}
}