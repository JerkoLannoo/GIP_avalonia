using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GIP_av.Views;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static GIP_av.BeurtenBekijken;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Text.Json.Nodes;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;

namespace GIP_av;

public partial class GastBuertenBekijken : Window
{
	ObservableCollection<BEURTINFO> GBeurtenGRID { get; set; } = new ObservableCollection<BEURTINFO>();//in top of code
	public int beurtenGridSelected { get; set; }
	JSON[] jsonObject;
	private static readonly HttpClient client = new HttpClient();
	public GastBuertenBekijken()
    {
		DataContext = this;
		InitializeComponent();
    }

	private void Close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		Dashboard da = new Dashboard();
		da.Show();
		this.Close();
	}

	private void showPassword_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (beurtenGridSelected!=null)
		{
			int index = beurtenGridSelected+jsonObject.Length-GBeurtenGRID.Count;//ga verschuiving van index tegen als je 'toon alleen geldig' aanzet. 
			//geselecteerde index + totale rijen - gefilterde rijen
			//omdat er minder rijen zijn dan de lengte van het object als je filtert
			GBeurtenGRID.Clear();
			for(int i = 0; i < jsonObject.Length; i++)
			{
				if (i == index)
				{
					if (filterChk.IsChecked == true)
					{
						if (jsonObject[i].used == 0) GBeurtenGRID.Add(new BEURTINFO(jsonObject[i].username, formatTime(Convert.ToInt32(jsonObject[i].time), Convert.ToInt32(jsonObject[i].data)), jsonObject[i].devices.ToString(), jsonObject[i].password.ToString()));//voeg rij toe aan tabel als de beurt nog niet gebruikt is
					}
					else
					{
						GBeurtenGRID.Add(new BEURTINFO(jsonObject[i].username, formatTime(Convert.ToInt32(jsonObject[i].time), Convert.ToInt32(jsonObject[i].data)), jsonObject[i].devices.ToString(), jsonObject[i].password.ToString()));//voeg rij toe aan tabel als de beurt nog niet gebruikt is
					}
				}
				else
				{
					if (filterChk.IsChecked == true)
					{
							if (jsonObject[i].used == 0) GBeurtenGRID.Add(new BEURTINFO(jsonObject[i].username, formatTime(Convert.ToInt32(jsonObject[i].time), Convert.ToInt32(jsonObject[i].data)), jsonObject[i].devices.ToString(), "********"));//voeg rij toe aan tabel als de beurt nog niet gebruikt is
					}
					else
					{
						GBeurtenGRID.Add(new BEURTINFO(jsonObject[i].username, formatTime(Convert.ToInt32(jsonObject[i].time), Convert.ToInt32(jsonObject[i].data)), jsonObject[i].devices.ToString(), "********"));//voeg rij toe aan tabel als de beurt nog niet gebruikt is
					}
				}
			}
		}
	}

	private async void DataGrid_Loaded_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		await getUserInfo();
	}

	private void filterChk_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		Debug.WriteLine("clicked show only valid");
		if (filterChk.IsChecked == true)
		{
			GBeurtenGRID.Clear();//verwijder alle rijen van de tabel
			for (int i = 0; i < jsonObject.Length; i++)//doorloop alle rijen
			{
				if (jsonObject[i].used == 0) GBeurtenGRID.Add(new BEURTINFO(jsonObject[i].username, formatTime(Convert.ToInt32(jsonObject[i].time), Convert.ToInt32(jsonObject[i].data)), jsonObject[i].devices.ToString(), "********"));//voeg rij toe aan tabel als de beurt nog niet gebruikt is
			}
		}
		else
		{
			GBeurtenGRID.Clear();//verwijder alle rijen van de tabel
			for (int i = 0; i < jsonObject.Length; i++)//doorloop alle rijen
			{
				GBeurtenGRID.Add(new BEURTINFO(jsonObject[i].username, formatTime(Convert.ToInt32(jsonObject[i].time), Convert.ToInt32(jsonObject[i].data)), jsonObject[i].devices.ToString(), "********"));//voeg rij toe aan tabel als de beurt nog niet gebruikt is
			}
		}
	}
	private async Task getUserInfo()
	{
		var values = "{\"pincode\":\"" + Data.pin + "\", \"bcode\":\"" + Data.bcode + "\"}";//maak JSON object
		JObject json = JObject.Parse(values);
		var jsonString = JsonConvert.SerializeObject(json);//omvormen naar JSON
		var content = new StringContent(values, Encoding.UTF8, "application/json");//zeggen wat de content is tegen de server
		var response = await client.PostAsync(Data.server_address + "/get-guest-beurten", content);//POST request verzenden
		Debug.WriteLine("fetching...");
		var responseString = await response.Content.ReadAsStringAsync();
		if (response.StatusCode == HttpStatusCode.OK)//als de server een geldige reactie heeft verzonden
		{
			Debug.WriteLine(responseString);
			Debug.WriteLine("fetched user beurten");
			try
			{//probeer eerst dit uit te voeren:
				JSON[] jsonObj = JsonConvert.DeserializeObject<JSON[]>(responseString); //vorm het JSON object om naar een C# object
				for (int i = 0; i < jsonObj.Length; i++)//doorloop alle rijen die server heeft doorgegeven
				{
					Debug.WriteLine("going trough loop, username: " + jsonObj[i].username);
					GBeurtenGRID.Add(new BEURTINFO(jsonObj[i].username, formatTime(Convert.ToInt32(jsonObj[i].time), Convert.ToInt32(jsonObj[i].data)).ToString(), jsonObj[i].used.ToString() + "/" + jsonObj[i].devices.ToString(), "********"));//voeg rij toe
				}
				Debug.WriteLine(GBeurtenGRID[0].Username.ToString() + " at try and has " + GBeurtenGRID.Count + " rows");
				jsonObject = jsonObj;
			}
			catch//als het bovenste niet lukt (er is maar één rij):
			{
				GBeurtenGRID.Add(new BEURTINFO(JObject.Parse(responseString)["username"].ToString(), JObject.Parse(responseString)["time"].ToString(), JObject.Parse(responseString)["used"].ToString() + "/" + JObject.Parse(responseString)["devices"].ToString(), "********"));
				Debug.WriteLine(GBeurtenGRID[0].Username.ToString());
				JSON jsonObj = new JSON
				{//maak nieuw JSON object
					username = JObject.Parse(responseString)["username"].ToString(),
					time = JObject.Parse(responseString)["time"].ToString(),
					devices = (int)JObject.Parse(responseString)["devices"]
				};
				jsonObject[0] = jsonObj;//JSON object opslaan in variabele
			}

		}
		else
		{
			//this.Text = "Er ging iets mis." + response.StatusCode;//toon deze tekst in de linkerbovenhoek
		}
	}
	private string formatTime(int time, int data)//tijd formatteren
	{
		if (time > 0)
		{
			if (time < 24) return time + "h";
			else if (time >= 24 && time < 720)
			{
				return (time / 24) + " Dag(en)";
			}
			else if (time >= 720)
			{
				return (time / 720) + " Maand(en)";
			}
			else return null;
		}
		else
		{
			return data + " GB";
		}

	}
	class BEURTINFO
	{
		public string Username { get; set; }
		public string Time { get; set; }
		public string Devices { get; set; }
		public string Password { get; set; }	
		public BEURTINFO(string username, string time, string devices, string password)
		{
			Username = username;
			Time = time;
			Devices = devices;
			Password = password;
		}
	}
}