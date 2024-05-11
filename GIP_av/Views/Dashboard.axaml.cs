using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GIP_av.Views;
using HidSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
namespace GIP_av;
public partial class Dashboard : Window
{
	private static readonly HttpClient client = new HttpClient();
	string saldo = "";
	string username = "";
	int devices = 0;
	int gDevices = 0;
	int nDevices = 0;
	int nGDevices = 0;
	public Dashboard()
    {
        InitializeComponent();
    }
	private async void StackPanel_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		gebruikersnaam.Text = "Laden...";
		await GetUserInfo();//voer deze functie uit wacht totdat deze klaar is
		gebruikersnaam.Text = username + " - " +  saldo;
		activeDevicesLbl.Text = devices.ToString();
		activeGdevicesLbl.Text = gDevices.ToString();
		nonActiveDevicesLbl.Text = nDevices.ToString();
		nonActiveGDevicesLbl.Text = nGDevices.ToString();
		if(devices == 0&&nDevices==0) {
			viewBeurtenbtn.IsEnabled = false;
		}
		if (gDevices == 0 && nGDevices == 0)
		{
			viewGBeurtenbtn.IsEnabled = false;//nog veranderen
		}
	}
	private async Task GetUserInfo()
	{
		try//probeer dit uit te voeren:
		{
			var values = "{\"pincode\":\"" + Data.pin + "\", \"bcode\":\"" + Data.bcode + "\",\"key\":\"" + Data.key + "\"}";//JSON object aanmaken
			JObject json = JObject.Parse(values);
			var jsonString = JsonConvert.SerializeObject(json);//omvormen naar JSON object
			var content = new StringContent(values, Encoding.UTF8, "application/json");//zeggen tegen server wat content type het is
			var response = await client.PostAsync(Data.server_address + "/get-user-info", content);//POST request sturen
			Debug.WriteLine("fetching");
			var responseString = await response.Content.ReadAsStringAsync();//reactie lezen als string
			if (response.StatusCode == HttpStatusCode.OK)//als server een geldige reactie heeft verzonden
			{
				JObject jsonObject = JObject.Parse(responseString);//JSON uit reactie halen
										//opslaan in variabelen:
				saldo = "€" + Math.Round(Convert.ToDouble(jsonObject["saldo"]), 2).ToString();//saldo opslaan en afronden tot op twee na komma
				devices = Convert.ToInt32(jsonObject["devices"]);//aantal apparaten opslaan
				gDevices = Convert.ToInt32(jsonObject["gDevices"]);//aantal gast apparaten opslaan
				nGDevices = Convert.ToInt32(jsonObject["nGDevices"]);//aantal niet geactiveerde gast apparaten opslaan
				nDevices = Convert.ToInt32(jsonObject["nDevices"]);//aantal niet geactiveerde apparaten opslaan
				username = jsonObject["username"].ToString();//gebruikersnaam opslaan
			}
			else
			{
				gebruikersnaam.Text = "Er ging iets mis";//IPV gebruikersnaam "er ging iets mis tonen"
			}
		}
		catch (Exception ex)//als bovenste niet lukt
		{
			gebruikersnaam.Text = "Er ging iets mis";//IPV gebruikersnaam "er ging iets mis tonen"
		}
	}

	private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)//log uit
	{
		MainWindow window = new MainWindow();
		window.Show();
		this.Close();
	}

	private void ViewBeurtenbtn(object? sender, Avalonia.Interactivity.RoutedEventArgs e)//bekijk beurten
	{
		BeurtenBekijken beurtenBekijken = new BeurtenBekijken();
		beurtenBekijken.Show();
		this.Close();
	}
	private void ViewGBeurtenbtn(object? sender, Avalonia.Interactivity.RoutedEventArgs e)//bekijk gast beurten
	{
		GastBuertenBekijken beurtenBekijken = new GastBuertenBekijken();
		beurtenBekijken.Show();
		this.Close();
	}
	private void addBeurt_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)//voeg beurt toe
	{
		AddBeurt addBeurt = new AddBeurt();
		addBeurt.Show();
		this.Close();
	}
	private void addGBeurt_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)//voeg gast beurt toe
	{
		AddGBeurt addGBeurt = new AddGBeurt();
		addGBeurt.Show();
		this.Close();
	}
}