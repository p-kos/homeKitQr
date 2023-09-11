<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <NuGetReference>System.Data.SqlClient</NuGetReference>
  <NuGetReference>System.Drawing.Common</NuGetReference>
  <NuGetReference>ZXing.Net</NuGetReference>
  <NuGetReference>ZXing.Net.Bindings.Windows.Compatibility</NuGetReference>
  <Namespace>Microsoft.AspNetCore.Mvc</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Configuration</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>ZXing</Namespace>
  <Namespace>ZXing.Windows.Compatibility</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Drawing.Drawing2D</Namespace>
  <Namespace>ZXing.Common</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

string password = "031-45-154";
string setupId = "1QJ8";

readonly string path = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "homekitCode.png");
readonly string qrBg = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "qrcode.png");

void Main()
{
	var code = createSetupUri(category: HomekitAccessoryCategory.lightbulb, password: password, setupId: setupId);

	generateGuidForTasmotaConfig();	
	generateQr(generatedCode: code);	
	generateHomeKitQr();
}

void generateGuidForTasmotaConfig()
{
	var guid = Guid.NewGuid().ToString().ToUpper().Split("-").Last();
	$"outlet.username = \"{guid[0]}{guid[1]}:{guid[2]}{guid[3]}:{guid[4]}{guid[5]}:{guid[6]}{guid[7]}:{guid[8]}{guid[9]}:{guid[10]}{guid[11]}\"".Dump();
	$"outlet.pincode = \"{password}\"".Dump();
}

void generateQr(string generatedCode)
{
	var barcodeWriter = new BarcodeWriter();
	barcodeWriter.Format = BarcodeFormat.QR_CODE;
	barcodeWriter.Options.Width = 200;
	barcodeWriter.Options.Height = 200;
	var barcodeBitmap = barcodeWriter.Write(generatedCode);
	barcodeBitmap.Save(path, ImageFormat.Png);
}

void generateHomeKitQr()
{
	var passwordLine1 = string.Join("", password.Replace("-", "").Take(4));
	var passwordLine2 = string.Join("", password.Replace("-", "").Skip(4));

	using (var b = new System.Drawing.Bitmap(200, 500))
	using (var g = Graphics.FromImage(b))
	using (var f = new Font("Roboto-Bold", 20))
	{
		Bitmap qr = new Bitmap(path);
		Bitmap bg = new Bitmap(qrBg);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.DrawImage(qr, new Rectangle(-20, 45, 240, 240));
		g.DrawImage(bg, new Rectangle(0, 0, 200, 270));
		g.DrawString(passwordLine1, f, Brushes.Black, 90, 10);
		g.DrawString(passwordLine2, f, Brushes.Black, 90, 45);
		b.Dump();
	}
}

// KUDOS: https://github.com/maximkulkin/esp-homekit/blob/0f3ef2ac2872ffe64dfe4e5d929420af327d48a5/include/homekit/types.h#L41
private string createSetupUri(
  int category,
  string password,
  string setupId,
  int version = 0,
  int reserved = 0,
  int flags = 2
)
{
	var BASE36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	long payload = 0;
	payload |= version & 0x7;

	payload <<= 4;
	payload |= (reserved & 0xf); // reserved bits

	payload <<= 8;
	payload |= (category & 0xff);

	payload <<= 4;
	payload |= (flags & 0xf);

	payload <<= 27;
	payload |= (Convert.ToInt32(password.Replace("-", "")) & 0x7fffffff);

	var encodedPayload = "";
	foreach (var _ in Enumerable.Range(0, 8))
	{
		var index = Convert.ToInt32(payload % 36);
		encodedPayload += BASE36[index];
		payload /= 36;
	}
	var output = string.Join("", encodedPayload.Reverse()).PadLeft(9, '0');
	return $"X-HM://{output}{setupId}";
}
// KUDOS: https://github.com/maximkulkin/esp-homekit/blob/0f3ef2ac2872ffe64dfe4e5d929420af327d48a5/include/homekit/types.h#L41
static class HomekitAccessoryCategory {
	public static int other = 1;
	public static int bridge = 2;
	public static int fan = 3;
	public static int garage = 4;
	public static int lightbulb = 5;
	public static int door_lock = 6;
	public static int outlet = 7;
	public static int homekit_accessory_category_switch = 8;
	public static int thermostat = 9;
	public static int sensor = 10;
	public static int security_system = 11;
	public static int door = 12;
	public static int window = 13;
	public static int window_covering = 14;
	public static int programmable_switch = 15;
	public static int range_extender = 16;
	public static int ip_camera = 17;
	public static int video_door_bell = 18;
	public static int air_purifier = 19;
	public static int heater = 20;
	public static int air_conditioner = 21;
	public static int humidifier = 22;
	public static int dehumidifier = 23;
	public static int apple_tv = 24;
	public static int speaker = 26;
	public static int airport = 27;
	public static int sprinkler = 28;
	public static int faucet = 29;
	public static int shower_head = 30;
	public static int television = 31;
	public static int target_controller = 32;
}