using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Drawing.Imaging;
using System.IO;
using System;
using ZXing;
using ZXing.SkiaSharp;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GIP_av.Views;
using System.Threading;
using System.Diagnostics;
using Avalonia.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SkiaSharp.QrCode.Image;
using SkiaSharp;
//using System.Windows.Media.ImageSource;
namespace GIP_av;

public partial class AccountToevoegen : Window
{
	public string server_address = Data.server_address;//definieer server adres
	public AccountToevoegen()
    {
        InitializeComponent();
    }
	private void StackPanel_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		Debug.WriteLine("loaded...");
		Socket();

		/*using (MemoryStream stream = new MemoryStream())
		{
			data.SaveTo(stream);
			var bitmap = new Image();
			qrcodeIMG.Source = new Avalonia.Media.Imaging.Bitmap(stream);
		}*/
		// Replace the text below with the content you want in the QR code

		/*QRCodeGenerator qrGenerator = new QRCodeGenerator();
		QRCodeData qrCodeData = qrGenerator.CreateQrCode(“Hello MAUI”, QRCodeGenerator.ECCLevel.L);

		PngByteQRCode qRCode = new PngByteQRCode(qrCodeData);
		byte[] qrCodeBytes = qRCode.GetGraphic(20);
		ImageSource qrImageSource = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
		CodeQrBarcodeDraw qrcode = BarcodeDrawFactory.CodeQr;//maak een nieuwe QR-code 
		qrcodeImage.DataContext = qrcode.Draw("https://gip.jerkolannoo.com/register/remote-registration?code=", 60);//teken de QR-code met https://gip.jerkolannoo.be/register/remote-registration?code=+codelbl.Text als waarde
	*/
	}
	private void Socket()
	{
		string text = "";
		var client = new SocketIOClient.SocketIO(server_address);//maak een nieuwe socket tussen C# en server aan
		client.On("get-code", response =>//als dit gebeurt (server zal gebuertenis onder deze naam versturen):
		{
			text = response.GetValue<string>();//reactie uitlezen als string
			createQRCode(text);
			Debug.WriteLine("received code: " + text);
			//info.Text = "Scan de QR-code of ga naar de site (tabblad \"Registreren via terminal\") en geef deze code in: ";//verander tekst
			Dispatcher.UIThread.Post(() => info.Text = "Scan de QR-code of ga naar de site (tabblad \"Registreren via terminal\") en geef deze code in: ");
			Dispatcher.UIThread.Post(() => code.Text = text);
			//code.Text = text;//toon de code
		});
		client.On("register-status", response =>//als de gebruikt geregistreerd heeft:
		{
			text = response.GetValue<string>();
			Debug.WriteLine("received code: " + text);
			
			Dispatcher.UIThread.Post(() =>
			{
				info.Text = "Kijk in uw mailbox voor een verificatie link.";//verander tekst
				Dispatcher.UIThread.Post(() => code.Text = "");
				Dispatcher.UIThread.Post(() => qrcodeIMG.IsVisible = false);
			});
			//code.Text = "";//doe de code weg
			//qrcodeIMG.IsVisible = false;//maak de QR-code onzichtbaar
		});
		client.On("scan-status", response =>//als de gebruiker de QR-code gescant heeft:
		{
			text = response.GetValue<string>();
			Debug.WriteLine("received code: " + text);
			Dispatcher.UIThread.Post(() =>
			{
				info.Text = "Voer de gegevens in op uw apparaat.";
				code.Text = "";
				qrcodeIMG.IsVisible = false;
			});
		});
		client.On("end-connection", response =>//eindig de verbinding:
		{
			text = response.GetValue<string>();//lees code uit (overbodig)
			Debug.WriteLine("received code: " + text);
			Dispatcher.UIThread.Post(() =>
			{
				info.Text = text;
				code.Text = "";
				qrcodeIMG.IsVisible = false;
			});
			Thread.Sleep(5000);
			Dispatcher.UIThread.Post(() =>
			{
				MainWindow window = new MainWindow();
				window.Show();
			});
			Dispatcher.UIThread.Post(() => this.Close());
		});
		client.OnConnected += async (sender, e) =>//verbindt met de server
		{
			await client.EmitAsync("get-code", Data.bcode);//vraag om code aan server
			Debug.WriteLine("request send");
		};
		 client.ConnectAsync();//wacht totdat hij verbonden is met server
	}
	private void createQRCode(string code)
	{
		Debug.WriteLine("generating code");
		QrCode qrCode = new QrCode("https://gip.jerkolannoo.com/register/remote-registration?code="+code, new Vector2Slim(256, 256), SKEncodedImageFormat.Png);
		Debug.WriteLine("code generated");
		MemoryStream stream = new MemoryStream();//geen "using memorystream" gebruiken omdat de QR code anders niet geladen kan worden van de Bitmap
		qrCode.GenerateImage(stream);
		Debug.WriteLine("code saved");
		stream.Seek(0, SeekOrigin.Begin);
		stream.Position = 0;
		Dispatcher.UIThread.Post(() => qrcodeIMG.Source = new Avalonia.Media.Imaging.Bitmap(stream));
	}

	private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		MainWindow window = new MainWindow();
		window.Show();
		this.Close();
	}
}