using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GIP_av.Views;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
using NP.Utilities;

namespace GIP_av;

public partial class AddGBeurt : Window
{
	PRIJZEN[] prijzen;
	SUCCESS success;
	private static readonly HttpClient client = new HttpClient();
	public AddGBeurt()
    {
		this.DataContext = this;
        InitializeComponent();
    }
	private async void Grid_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		lsTijden.Items.Add("Uur");
		lsTijden.Items.Add("Dag(en)");
		lsTijden.Items.Add("Maand(en)");
		lsTijden.SelectedIndex = 0;
		await getPrices();
		calcPrice();
	}
	private async Task getPrices()
	{
		try
		{
			var response = await client.GetAsync(Data.server_address + "/get-prijzen");//get request maken
			Debug.WriteLine("fetching");
			var responseString = await response.Content.ReadAsStringAsync();//response opslaan als string
			if (response.StatusCode == HttpStatusCode.OK)//als er een geldige reactie is ontvangen
			{
				prijzen = JsonConvert.DeserializeObject<PRIJZEN[]>(responseString);//JSON omvormen naar C# object
			}
			else
			{
				prijsLbl.Text = responseString;
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
			prijsLbl.Text = "Kon prijs niet laden.";//Als er een fout is
		}

	}
	private async Task SendInfo()
	{
		addBtn.IsEnabled = false;
		try//proberen aanvraag naar server te versturen
		{
			var values = "{\"pincode\":\"" + Data.pin + "\", \"bcode\":\"" + Data.bcode + "\", \"duration\":\"" + formatTime((decimal)duration.Value) + "\", \"devices\":\"" + devices.Text + "\", \"adblock\":\"" + adblockBtn.IsChecked + "\"}";//JSON object
			JObject json = JObject.Parse(values);//omvormer naar JSON
			var jsonString = JsonConvert.SerializeObject(json);
			var content = new StringContent(values, Encoding.UTF8, "application/json");
			var response = await client.PostAsync(Data.server_address + "/add-guest-beurt", content);//POST request versturen
			Debug.WriteLine("fetching");
			var responseString = await response.Content.ReadAsStringAsync();//lees reactie als tekst (string)
			if (response.StatusCode == HttpStatusCode.OK)//als de server een geldig antwoord heeft verstuurd
			{
				success = JsonConvert.DeserializeObject<SUCCESS>(responseString);
				if (Convert.ToBoolean(success.success) == true)
				{
					infoLbl.Text = "Beurt aangemaakt.";//helaas nog geen manier gevonden om de kleur aan te passen naar groen
					addBtn.IsEnabled = true;
					Info info = new Info();
					info.Show();
					this.Close();
					//	Dashboard dash = new Dashboard();//maak een nieuw dashboard aan
					//	dash.Show();//toon dashboard
					//	this.Close();//sluit dit venster
				}
				else
				{
					infoLbl.Text = success.msg;
					addBtn.IsEnabled = true;//zet de toevoeg knop terug aan
				}

			}
			else//als er een error is
			{
				infoLbl.Text = "Er ging iets mis.";
				addBtn.IsEnabled = true;
			}
		}
		catch (Exception ex)//als er een error is (aanvraag mislukt)
		{
			infoLbl.Text = "Er ging iets mis.";
			addBtn.IsEnabled = true;//ze de toevoeg knop terug aan
		}

	}

	private void Aanmaken_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		SendInfo();
	}

	private void Button_Click_2(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		Dashboard da = new Dashboard();
		da.Show();
		this.Close();
	}
	private decimal formatTime(decimal time)
	{
		if (lsTijden.SelectedIndex == 0) return time;
		else if (lsTijden.SelectedIndex == 1) return time * 24;
		else return time * 720;
	}
	private void calcPrice()
	{
		int maxprice = 0;//zorgen dat de goedkoopste optie wordt berekend
		int tijd = 0;
		if (lsTijden.SelectedIndex == 0) tijd = 1;
		if (lsTijden.SelectedIndex == 1) tijd = 24;
		if (lsTijden.SelectedIndex == 2) tijd = 720;
		if (duration.Value != null && devices.Value != null)
		{
			for (int i = prijzen.Count() - 1; i >= 0; i--)
			{
				Debug.WriteLine("running prices, maxprice: " + maxprice);
				if (prijzen[i].time <= (decimal)duration.Value * (decimal)tijd && prijzen[i].time > maxprice)
				{
					maxprice = prijzen[i].time;
					priceLbl.Text = ((float)prijzen[i].price * (float)devices.Value * (float)duration.Value * (float)tijd / (float)prijzen[i].time).ToString("0.00") + "€";
				}
			}
		}
	}
	private void devices_ValueChanged(object? sender, Avalonia.Controls.NumericUpDownValueChangedEventArgs e)
	{
		calcPrice();
	}

	private void lsTijden_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
	{
		calcPrice();
	}

	private void duration_ValueChanged(object? sender, Avalonia.Controls.NumericUpDownValueChangedEventArgs e)
	{
		calcPrice();
	}
	class PRIJZEN
	{
		public int time { get; set; }
		public float price { get; set; }
		public int devices { get; set; }
	}
	class SUCCESS
	{
		public string success { get; set; }
		public string msg { get; set; }
	}
}