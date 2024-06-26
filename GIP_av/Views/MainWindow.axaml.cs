﻿using Avalonia.Controls;
using HidSharp.Reports;
using HidSharp;
using System.Diagnostics;
using System.IO.Ports;
using Avalonia.Threading;
using static GIP_av.Views.MainView;
using DynamicData;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;

namespace GIP_av.Views;

public partial class MainWindow : Window
{
	SerialPort sport = new SerialPort();
	public string code = "";
	bool login = false;
	int status = 0;
	public MainWindow()
    {
        InitializeComponent();
    }
	private void Grid_Loaded_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)//als de grid geladen wordt zoeken we naar de NFC reader
	{
		SendInfo();
		DeviceList devices = DeviceList.Local;
		sport.BaudRate = 9600;
		string dev = "";
		info.Text += "\n";
		foreach (SerialDevice s in DeviceList.Local.GetSerialDevices())//gebruik HidSharp om de naam de verkrijgen van alle seriële poorten
		{
			info.Text += s.GetFriendlyName() +" - "+s.GetFileSystemName()+"\n";
			Debug.WriteLine("Device: "+s.GetFriendlyName());
			if (s.GetFriendlyName().StartsWith("USB-SERIAL CH340"))//voor windows (voor testen)
			{
				Debug.WriteLine(s.GetFriendlyName()+" is the device name");
				dev = s.GetFriendlyName().Substring(s.GetFriendlyName().IndexOf("(") + 1);//het woord '(COM)' eruit halen
				dev = dev.Substring(0, dev.IndexOf(")"));//het woord '(COM)' eruit halen
				dev = dev.ToUpper();//alles naar grote letters, voor de zekerheid
			}
			if (s.GetFriendlyName().StartsWith("/dev/ttyUSB"))//voor linux
			{
				Debug.WriteLine(s.GetFriendlyName() + " is the device name");
				dev = s.GetFriendlyName();//geen stripping van naam nodig in linux (woord '(COM)' komt er niet in voor)
			}
		}
		Debug.WriteLine("De gevonden COM poort is:" + dev);
		if (dev.Length > 0)
		{
			info.Text = "Scan je leerlingenkaart.";
			sport.PortName = dev;
			sport.Open();
			sport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
		}
		else info.Text += "Kon geen NFC reader vinden.";
		//  code = sport.ReadLine();
	}
	public async Task SendInfo()
	{
		info.Text = "Wacht even...";
		try//probeer dit:
		{
			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
				{
					Console.WriteLine("SSL error skipped");
					return true;
				}
			};
			HttpClient client = new HttpClient(handler);
			Debug.WriteLine(Data.bcode);
			string values = "{\"bcode\":\"" + Data.bcode + "\",\"key\":\"" + Data.key + "\"}";//maak JSON string
			JObject json = JObject.Parse(values);
			var jsonString = JsonConvert.SerializeObject(json);//omvormen naar JSON
			var content = new StringContent(values, Encoding.UTF8, "application/json");//content type zeggen tegen server
			var response = await client.PostAsync(Data.server_address + "/check-code", content);//POST request versturen
			Debug.WriteLine("fetching, bcode: " + Data.bcode);
			var responseString = await response.Content.ReadAsStringAsync();//lees de reactie als string
			Debug.WriteLine("fetching: " + response.StatusCode.ToString());
			if (response.StatusCode == HttpStatusCode.OK)//als er een geldige reactie is ontvangen
			{
				JObject jsonObject = JObject.Parse(responseString);//vorm reactie om naar JSON
				string value = jsonObject["login"].ToString();//neem login attribuut van object en sla op als string
				login = Convert.ToBoolean(value);//sla login op als bool (true of false)
				Debug.WriteLine("login: " + login);
				if (login)//als login 'true' is, sluit COM poort, dit venster en toon login scherm
				{
					Data.doubleTap = Convert.ToInt32(jsonObject["doubleTap"]);
					sport.Close();
					info.Text = "OK.";
					PIN pin = new PIN();
					pin.Show();
					this.Close();
					info.Text = "Scan je leerlingenkaart.";
				}
				else//anders: toon geen account scherm
				{
					sport.Close();
					GeenAccount account = new GeenAccount();
					account.Show();
					this.Close();
					//sport.Open();
					info.Text = "Geen geldige leerlingenkaart.";
				}
				//loading_icon.Visible = false;//verberg laad icoon
				status = 1;//zet status op 1
			}
			else//geen geldige reactie:
			{
				info.Text = "Kan de server niet bereiken.\nFoutcode: "+response.StatusCode;
				// MessageBox.Show("Er ging iets mis.");//toon pop-up
				code = "";//leeg de barcode
						  // loading_icon.Visible = false;//verberg laad icoon
				status = 2;//zet status op 2
			}
		}
		catch (Exception ex)//als bovenste code mislukt:
		{
			Debug.WriteLine("ERROR: " + ex);
			info.Text = "Kon server niet bereiken.\n"+ex.Message;
			code = "";//leeg barcode
			status = 2;//zet status op 2
		}
	}

	public void DataReceivedHandler(object? sender, SerialDataReceivedEventArgs e)//aparte thread
	{
		SerialPort sp = (SerialPort)sender;
		string indata = sp.ReadExisting();
		indata = indata.Replace("\r\n", "");//replace gebruiken om de ENTER uit 'indata' te verwijderen
		Debug.WriteLine("Data Received:");
		Debug.Write(indata);
		if(indata.Length>0) Data.bcode = indata;
		Dispatcher.UIThread.Post(async () => await SendInfo());
	}
}
public class Data //globaal opslaan van data
{
	static public string bcode = "D4 C8 02 2A";//barcode
	static public string server_address = "http://192.168.100.3";//IP-adres van server
	static public string key = "TG9KNHRJRDhSaUtMcjdueFZRU1RUREU5ZEs3a1Zo";
	static public int pin;//pin-code
	static public int doubleTap;//snel registreren (hier wordt MAC-adres in opgeslagen)
}