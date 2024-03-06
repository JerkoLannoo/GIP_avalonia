using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GIP_av.Views;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Text.Json.Nodes;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GIP_av;

public partial class BeurtenBekijken : Window
{
	ObservableCollection<BEURTINFO> BeurtenGRID { get; set; } = new ObservableCollection<BEURTINFO>();//in top of code
	private static readonly HttpClient client = new HttpClient();
	public BeurtenBekijken()
    {
		DataContext = this;
		InitializeComponent();
    }

	private async void DataGrid_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		await getUserInfo();
	}
	private async Task getUserInfo()
	{
		var values = "{\"pincode\":\""+Data.pin + "\", \"bcode\":\""+Data.bcode + "\"}";//maak JSON object
		JObject json = JObject.Parse(values);
		var jsonString = JsonConvert.SerializeObject(json);//omvormen naar JSON
		var content = new StringContent(values, Encoding.UTF8, "application/json");//zeggen wat de content is tegen de server
		var response = await client.PostAsync(Data.server_address + "/get-user-beurten", content);//POST request verzenden
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
					BeurtenGRID.Add(new BEURTINFO( jsonObj[i].username, formatTime(Convert.ToInt32(jsonObj[i].time), Convert.ToInt32(jsonObj[i].data)).ToString(), jsonObj[i].devices.ToString()));//voeg rij toe
				}
				Debug.WriteLine(BeurtenGRID[0].Username.ToString()+" at try and has "+BeurtenGRID.Count + " rows");
			}
			catch//als het bovenste niet lukt (er is maar één rij):
			{
				BeurtenGRID.Add(new BEURTINFO(JObject.Parse(responseString)["username"].ToString(), JObject.Parse(responseString)["time"].ToString(), JObject.Parse(responseString)["devices"].ToString()));
				Debug.WriteLine(BeurtenGRID[0].Username.ToString());
				//jsonObjects[0] = jsonObj;//JSON object opslaan in variabele
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
		else{
			return data + " GB";
		}

	}
	public class BEURTINFO
	{
		public string Username { get; set; }
		public string Time { get; set; }
		public string Devices { get; set; }
		public BEURTINFO(string username, string time, string devices) { 
			Username = username;
			Time = time;	
			Devices = devices;	
		}
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
	class JSON//C# object met naam JSON
	{
		public string email { get; set; }//e-mail
		public int devices { get; set; }//apparaten
		public string time { get; set; }//tijd
		public string loginDate { get; set; }//login datum
		public string creationDate { get; set; }//aanmaak datum
		public float price { get; set; }//prijs
		public int type { get; set; }//type
		public long activeDate { get; set; }//activatie datum
		public int used { get; set; }//gebruikt
		public string username { get; set; }//gebruikersnaam
		public string password { get; set; }//wachtwoord
		public string data { get; set; }//data
	}

	private void Sluiten_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		Dashboard da= new Dashboard();
		da.Show();
		this.Close();
	}
}