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
using System.IO.Ports;
using HidSharp;
using Avalonia.Threading;

namespace GIP_av;

public partial class PIN : Window
{
	SerialPort sport = new SerialPort();
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
					sport.Close();
					Dashboard dash = new Dashboard();//nieuwe instantie van dashboard aanmaken
					dash.Show();//toon het dashboard
					this.Close();//sluit dit
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
	private async Task quickRegister()
	{
		try//probeer eerst dit:
		{
			info.Text = "Wacht even...";//verander tekst
			var values = "{\"bcode\":\"" + Data.bcode + "\"}";//maak JSON object
			Debug.WriteLine(values);
			JObject json = JObject.Parse(values);
			var jsonString = JsonConvert.SerializeObject(json);//object omvormer naar JSON
			var content = new StringContent(values, Encoding.UTF8, "application/json");//content type zeggen tegen server
			var response = await client.PostAsync(Data.server_address + "/quick-register", content);//POST request sturen naar server
			Debug.WriteLine("fetching");
			var responseString = await response.Content.ReadAsStringAsync();//lees reactie als string
			Debug.WriteLine(response.StatusCode.ToString());
			if (response.StatusCode == HttpStatusCode.OK)//als de server een geldig antwoord heeft gegeven
			{
				JObject jsonObject = JObject.Parse(responseString);//reactie omvormer naar JSON
				bool success = Convert.ToBoolean(jsonObject["success"]);//login veld uit object halen
				if (success)
				{//kijken of success true is
					sport.Close();
					QuickRegister quickRegister = new QuickRegister();
					quickRegister.Show();
					this.Close();
				}
				else info.Text = jsonObject["msg"].ToString();//ander is de pin-code verkeerd
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
		if (Data.doubleTap > 0) doubleTapLbl.Text = "Scan je leerlingenkaart voor snel registreren.";
		DeviceList devices = DeviceList.Local;

		sport.BaudRate = 9600;
		string dev = "";
		//info.Text += "\n";
		foreach (SerialDevice s in DeviceList.Local.GetSerialDevices())
		{
			//info.Text += s.GetFriendlyName() + " - " + s.GetFileSystemName() + "\n";
			Debug.WriteLine("Device: " + s.GetFriendlyName());
			if (s.GetFriendlyName().StartsWith("USB-SERIAL CH340"))
			{
				Debug.WriteLine(s.GetFriendlyName() + " is the device name");
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
		//	info.Text = "Scan je leerlingenkaart.";
			sport.PortName = dev;
			sport.Open();
			sport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
		}
		else doubleTapLbl.Text += "Kon geen NFC reader vinden.";
	}

	public void DataReceivedHandler(object? sender, SerialDataReceivedEventArgs e)//aparte thread
	{
		SerialPort sp = (SerialPort)sender;
		string indata = sp.ReadExisting();
		indata = indata.Replace("\r\n", "");//replace gebruiken om de ENTER uit 'indata' te verwijderen
		Debug.WriteLine("Data Received:");
		Debug.Write(indata);
		if (indata.Length > 0) Data.bcode = indata;
		Dispatcher.UIThread.Post(async () => await quickRegister());
	}
	private void Button_Click_2(object sender, Avalonia.Interactivity.RoutedEventArgs e)//als een van de cijfers wordt ingedrukt
	{
		Button button = sender as Button;
		if (button.Tag.ToString() == "D" && pincode.Length > 0)
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
		sport.Close();
		MainWindow window = new MainWindow();
		window.Show();
		this.Close();
	}
}