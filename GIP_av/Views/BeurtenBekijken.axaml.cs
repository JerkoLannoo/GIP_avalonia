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
	ObservableCollection<JSON> BeurtenGRID { get; set; }//alleen lezen
	JSON[] jsonObjects;
	private static readonly HttpClient client = new HttpClient();
	public BeurtenBekijken()
    {
        InitializeComponent();
    }

	private async void DataGrid_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		await GetUserInfo();
	}
	private async Task GetUserInfo()
	{
		var values = "{\"pincode\":\"1234" + "\", \"bcode\":\"42107" + "\"}";//JSON object maken
		JObject json = JObject.Parse(values);
		var jsonString = JsonConvert.SerializeObject(json);//omvormen naar JSON
		var content = new StringContent(values, Encoding.UTF8, "application/json");//applicatie type zeggen tegen server
		var response = await client.PostAsync(Data.server_address + "/get-guest-beurten", content);//POST request maken
		Debug.WriteLine("fetching");
		var responseString = await response.Content.ReadAsStringAsync();//lees reactie als string
		if (response.StatusCode == HttpStatusCode.OK)//als server geldige reactie geeft:
		{
			Debug.WriteLine(responseString);
			try//probeer dit (meerdere JSON objecten):
			{
				JSON[] jsonObj = JsonConvert.DeserializeObject<JSON[]>(responseString);//vorm reactie om naar C# objecten
				List<JSON> list = new List<JSON>();
				for (int i = 0; i < jsonObj.Length; i++)//doorloop alle objecten
				{
					string pswd = "";//maak string om wachtwoord te verbergen
					for (int y = 0; y < jsonObj[i].password.Length; y++) pswd += "*";//voeg '*' toe zodat het aantal '*' overeenkomt met de lengte van het wachtwoord 
					list.Add(jsonObj[i]);
				}
				BeurtenGRID = new ObservableCollection<JSON>(list);
				jsonObjects = jsonObj;//sla het object op in een variabele 
			}
			catch//als dat niet lukt (één JSON object):
			{
				JSON jsonObj = new JSON//maak nieuw JSON object
				{
					username = JObject.Parse(responseString)["username"].ToString(),
					time = JObject.Parse(responseString)["time"].ToString(),
					devices = (int)JObject.Parse(responseString)["devices"]
				};
				string pswd = "";//zelfde als hierboven...
				for (int y = 0; y < jsonObj.password.Length; y++) pswd += "*";
				//dataView.Rows.Add(jsonObj.username, formatTime(Convert.ToInt32(jsonObj.time)), jsonObj.devices, pswd);
				jsonObjects[0] = jsonObj;
			}
		}
		else//als server geen geldige reactie geeft:
		{
			//this.Text = "Er ging iets mis." + response.StatusCode;//linksboven tekst veranderen
		}
	}
	private string formatTime(int time)//tijd formatteren
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
	}
}