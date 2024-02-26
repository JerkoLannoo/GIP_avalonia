using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GIP_av.Views;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System;
using System.Net.Http;
using static GIP_av.Views.MainView;
using System.Text;
using System.Threading.Tasks;

namespace GIP_av;

public partial class PIN : Window
{
	public bool login = false;
	string pincode = "";
	byte status = 0;
	private static readonly HttpClient client = new HttpClient();
	public PIN()
    {
        InitializeComponent();
    }
	private async void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (pin.Text != null) await SendInfo();
	}
	private async Task SendInfo()
	{
		try//probeer eerst dit:
		{
			info.Text = "Wacht even...";//verander tekst
			var values = "{\"pincode\":\"" + pin.Text + "\", \"bcode\":\"" + Data.bcode + "\"}";//maak JSON object
			Debug.WriteLine(values);
			JObject json = JObject.Parse(values);
			var jsonString = JsonConvert.SerializeObject(json);//object omvormer naar JSON
			var content = new StringContent(values, Encoding.UTF8, "application/json");//content type zeggen tegen server
			var response = await client.PostAsync(Data.server_address + "/check-pin", content);//POST request sturen naar server
			Debug.WriteLine("fetching");
			var responseString = await response.Content.ReadAsStringAsync();//lees reactie als string
			Debug.WriteLine(response.StatusCode.ToString());
			if (response.StatusCode == HttpStatusCode.OK)//als de server een geldig antwoord heeft gegeven
			{
				JObject jsonObject = JObject.Parse(responseString);//reactie omvormer naar JSON
				string value = jsonObject["login"].ToString();//login veld uit object halen
				login = Convert.ToBoolean(value);//login omvormer naar bool
				Debug.WriteLine(login);
				if (login)
				{//kijken of login true is
					info.Text = "OK";//tekst op scherm aanpassen
					okbtn.IsEnabled = false;//'OK' knop uitzetten
					Data.pin = Convert.ToInt32(pin.Text);//pin-code in globale pin-code opslaan
														 // dashboard dash = new dashboard();//nieuwe instantie van dashboard aanmaken
														 //  dash.ShowDialog();//toon het dashboard
					//this.Close();//sluit dit
				}
				else info.Text = "Verkeerde PIN-code";//ander is de pin-code verkeerd
			}
			else//fout tijdens communiceren met sever
			{
				info.Text = "Er ging iets mis.";//tekst op scherm veranderen
			}
		}
		catch (Exception ex)//fout tijdens communiceren met sever
		{
			info.Text = ex.Message;
			/*this.Invoke((MethodInvoker)delegate//toegang krijgen tot andere taak
            {
                info.Text = "Er ging iets mis.";//tekst aanpassen
                info.Update();//tekst updaten
            });*/
		}
	}

	private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		pin.Focus();
	}

	private void Button_Click_2(object sender, Avalonia.Interactivity.RoutedEventArgs e)//als een van de cijfers wordt ingedrukt
	{
		Button button = sender as Button;
		if (button.Tag.ToString() == "C")
		{
			pincode = "";
		}
		else if (button.Tag.ToString() == "D" && pincode.Length > 0)
		{
			pincode = pincode.Substring(0, pincode.Length - 1);
		}
		else if (button.Tag.ToString() != "D")//anders gaat hij bij 2 keer backspace een 'D' in de pincode stoppen
		{
			pincode += button.Tag.ToString();
		}
		pin.Text = pincode;
		Debug.WriteLine("pressed" + pincode);
	}

	private void btnClose_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		MainWindow window = new MainWindow();
		window.Show();
		this.Close();
	}

	private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
	{
		MainWindow window = new MainWindow();
		window.Show();
	}
}