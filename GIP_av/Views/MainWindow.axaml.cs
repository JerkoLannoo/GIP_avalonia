using Avalonia.Controls;
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
	private static readonly HttpClient client = new HttpClient();
	bool login = false;
	int status = 0;
	public MainWindow()
    {
        InitializeComponent();
    }

	private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
	}

	private void Grid_Loaded_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		DeviceList devices = DeviceList.Local;

		sport.BaudRate = 9600;
		string dev = "";
		info.Text += "\n";
		foreach (SerialDevice s in DeviceList.Local.GetSerialDevices())
		{
			info.Text += s.GetFriendlyName() +" - "+s.GetFileSystemName()+"\n";
			Debug.WriteLine("Device: "+s.GetFriendlyName());
			if (s.GetFriendlyName().StartsWith("USB-SERIAL CH340"))
			{
				Debug.WriteLine(s.GetFriendlyName()+" is the device name");
				dev = s.GetFriendlyName().Substring(s.GetFriendlyName().IndexOf("(") + 1);
				dev = dev.Substring(0, dev.IndexOf(")"));
				dev = dev.ToUpper();
			}
			if (s.GetFriendlyName().StartsWith("/dev/ttyUSB"))
			{
				Debug.WriteLine(s.GetFriendlyName() + " is the device name");
				dev = s.GetFriendlyName();
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
			Debug.WriteLine(Data.bcode);
			string values = "{\"bcode\":\"" + Data.bcode + "\"}";//maak JSON string
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
				if (login)
				{
					Data.doubleTap = Convert.ToInt32(jsonObject["doubleTap"]);
					sport.Close();
					info.Text = "OK.";
					PIN pin = new PIN();
					pin.Show();
					this.Close();
					info.Text = "Scan je leerlingenkaart.";
				}
				else
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
			/* this.Invoke((MethodInvoker)delegate {//toegang krijgen tot andere taak
				 info.Text = "Scan je leerlingenkaart.";
				 loading_icon.Visible = false;//verberg laad icoon
				 info.Update();//update de tekst
			 });*/
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
		// `Run` blocks until all async work is done.
	}
}
public class Data //globaal opslaan van data
{
	static public string bcode = "";//barcode
	static public string server_address = "http://192.168.100.3:80";//IP-adres van server
	static public int pin;//pin-code
	static public int doubleTap;
}