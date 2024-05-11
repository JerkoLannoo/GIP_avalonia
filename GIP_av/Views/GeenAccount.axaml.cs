using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GIP_av.Views;

namespace GIP_av;

public partial class GeenAccount : Window
{
    public GeenAccount()
    {
        InitializeComponent();
    }
	private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)//als er op annuleren wordt gedrukt (open nieuw scherm en sluit dit)
	{
		MainWindow window = new MainWindow();
		window.Show();
		this.Close();
	}
	private void Ja_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)//als er op ja wordt gedrukt
	{
		AccountToevoegen account = new AccountToevoegen();
		account.Show();
		this.Close();
	}
}