using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SkiaSharp.QrCode;
using SkiaSharp;
using System.Drawing.Imaging;
using System.IO;
using Zen.Barcode;
using System;
using SkiaSharp.QrCode.Image;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GIP_av.Views;
using System.Threading;
using System.Diagnostics;
using Avalonia.Threading;
using System.Threading.Tasks;
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
			info.Text = "Kijk in uw mailbox voor een verificatie link.";//verander tekst
			Dispatcher.UIThread.Post(() => info.Text = "Scan de QR-code of ga naar de site (tabblad \"Registreren via terminal\") en geef deze code in: ");
			Dispatcher.UIThread.Post(() => code.Text = "");
			Dispatcher.UIThread.Post(() => qrcodeIMG.IsVisible = false);
			//code.Text = "";//doe de code weg
			//qrcodeIMG.IsVisible = false;//maak de QR-code onzichtbaar
		});
		client.On("scan-status", response =>//als de gebruiker de QR-code gescant heeft:
		{
			text = response.GetValue<string>();
			Debug.WriteLine("received code: " + text);
			Dispatcher.UIThread.Post(() => info.Text = "Voer de gegevens in op uw apparaat.");
			//info.Text = "Voer de gegevens in op uw apparaat.";
			Dispatcher.UIThread.Post(() => code.Text = "");
			//code.Text = "";
			Dispatcher.UIThread.Post(() => qrcodeIMG.IsVisible = false);
			//qrcodeIMG.IsVisible = false;
		});
		client.On("end-connection", response =>//eindig de verbinding:
		{
			text = response.GetValue<string>();//lees code uit (overbodig)
			Debug.WriteLine("received code: " + text);
			Dispatcher.UIThread.Post(() => info.Text = text);
			Dispatcher.UIThread.Post(() => code.Text = "");
			Dispatcher.UIThread.Post(() => qrcodeIMG.IsVisible = false);
			//info.Text = text;//toon code op scherm
			//code.Text = "";//verberg code
			//qrcodeIMG.IsVisible = false;//verberg QR-code
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
		BarcodeDraw barcodeDraw = BarcodeDrawFactory.CodeQr;
		var barcodeBitmap = barcodeDraw.Draw("https://gip.jerkolannoo.com/register/remote-registration?code="+code, 60);
		using (MemoryStream stream = new MemoryStream())//moet tijdenlijk opgeslagen worden in geheugen stream om van system.drawing.image naar avalonia.media.iimage te gaan
		{
			barcodeBitmap.Save(stream, ImageFormat.Png);//opslaan in geheugen
			stream.Position = 0;//lezen vanaf byte 0
								// Use Avalonia's Bitmap class to create a bitmap from the stream
			var bitmap = new Bitmap(stream);
			Dispatcher.UIThread.Post(() => qrcodeIMG.Source = bitmap);
		}
	}
}